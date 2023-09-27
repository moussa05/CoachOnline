using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.ApiObject;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Coach;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event = CoachOnline.Model.Event;

namespace CoachOnline.Services
{
    public class EventService : IEvent
    {
        private readonly ILogger<EventService> _logger;
        private readonly IEmailApiService _emailSvc;
        private readonly IStream _streamSvc;
        Random rn = new Random();
        public EventService(ILogger<EventService> logger, IEmailApiService emailSvc, IStream streamSvc)
        {
            _logger = logger;
            _emailSvc = emailSvc;
            _streamSvc = streamSvc;
        }

        public async Task<int> CreateEventDraft(int coachId)
        {
            using (var ctx = new DataContext())
            {
                var evt = new Event();
                evt.CoachId = coachId;
                evt.Status = EventStatus.DRAFT;
                evt.CreationDate = DateTime.Now;

                ctx.Events.Add(evt);

                await ctx.SaveChangesAsync();

                return evt.Id;
            }
        }

        public async Task DeleteMyEvent(int coachId, int eventId)
        {
            using(var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId).Include(c=>c.Coach).Include(p => p.Participants).ThenInclude(u=>u.User).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                if(evt.Status == EventStatus.DELETED || evt.Status == EventStatus.FINISHED)
                {
                    throw new CoachOnlineException($"Wrong event status: {evt.Status.ToString()}. Cannot delete this event.", CoachOnlineExceptionState.CantChange);
                }

                if(evt.Participants != null)
                {
                    foreach(var p in evt.Participants)
                    {
                        if(p.Status == EventPaymentStatus.SUCCESS || p.Status == EventPaymentStatus.PENDING)
                        {
                            await RefundCustomerPaymentForEvent(p.UserId, p.Id, true);
                            //send message
                
                        }

                        await SendEventCancelledMessage(p.User.EmailAddress, p.User.FirstName, p.User.Surname, evt.Coach.FirstName, evt.Coach.Surname, evt.Name);
                    }
                }

                evt.Status = EventStatus.DELETED;
                await ctx.SaveChangesAsync();
            }
        }

        private async Task SendEventCancelledMessage(string userEmail, string userFname, string userLname, string trainerFname, string trainerLname, string eventName)
        {
            string body = "";
            if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EventCancelledInfo.html"))
            {
                body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EventCancelledInfo.html");
                body = body.Replace("###COACHESURL###", $"{Statics.ConfigData.Config.WebUrl}");
                body = body.Replace("###EVENT_NAME###", $"{eventName}");
                body = body.Replace("###TRAINER_NAME###", $"{trainerFname} {trainerLname}");
            }

            if (body != "")
            {
                await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = userEmail,
                    ReceiverName = $"{userFname} {userLname}",
                    Topic = $"L'événement '{eventName}' a été annulé"
                });
            }
        }

        public async Task<string> StartMyLiveEvent(int eventId, int coachId)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.CoachId == coachId && t.Id == eventId).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                if(evt.Status != EventStatus.CREATED)
                {
                    throw new CoachOnlineException("Cannot start live event because event is in a wrong state.", CoachOnlineExceptionState.DataNotValid);
                }

                if(string.IsNullOrEmpty(evt.EventChannelName))
                {
                    evt.EventChannelName = GetEventChannelName(evt.Id, evt.CoachId);
                    await ctx.SaveChangesAsync();
                }

                if(!string.IsNullOrEmpty(evt.CoachLiveToken))
                {
                    return evt.CoachLiveToken;
                }

                uint validUntil = Convert.ToUInt32(ConvertTime.ToUnixTimestamp(evt.EndDate.AddMinutes(30)));

                var hostToken = _streamSvc.CreateStream(new CreateStreamRqs
                {
                    ChannelName = evt.EventChannelName,
                    UserId = evt.CoachId.ToString(),
                    IsHost = true,
                    ValidUntil = validUntil
                });

                evt.CoachLiveToken = hostToken;

                await ctx.SaveChangesAsync();

                return hostToken;
            }
        }

        public async Task<string> JoinLiveEvent(int userId, int eventId)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId).Include(p=>p.Participants).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                if (evt.Status != EventStatus.CREATED)
                {
                    throw new CoachOnlineException("Cannot start live event because event is in a wrong state.", CoachOnlineExceptionState.DataNotValid);
                }

                var participant = evt.Participants.FirstOrDefault(t => t.UserId == userId);
                participant.CheckExist("Participant");

                if(participant.Status != EventPaymentStatus.SUCCESS)
                {
                    throw new CoachOnlineException("Your payment for the event is incomplete", CoachOnlineExceptionState.NotAuthorized);
                }

                if(!string.IsNullOrEmpty(participant.EventLiveToken))
                {
                    return participant.EventLiveToken;
                }

                if(string.IsNullOrEmpty(evt.EventChannelName))
                {
                    throw new CoachOnlineException("Please wait for the coach to join in.", CoachOnlineExceptionState.NotExist);
                }
                uint validUntil = Convert.ToUInt32(ConvertTime.ToUnixTimestamp(evt.EndDate.AddMinutes(30)));

                var userToken = _streamSvc.CreateStream(new CreateStreamRqs
                {
                    ChannelName = evt.EventChannelName,
                    UserId = participant.UserId.ToString(),
                    IsHost = false,
                    ValidUntil = validUntil
                });

                participant.EventLiveToken = userToken;

                await ctx.SaveChangesAsync();

                return userToken;
            }
        }

        public async Task<int> UpdateEventDraft(int coachId, int eventId, string name, string description, string base64Pic, DateTime? startDate, DateTime? endDate, List<EventCategoryRqs> categories)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId && (t.Status == EventStatus.DRAFT || (t.Status == EventStatus.CREATED && t.StartDate > DateTime.Now))).Include(c => c.Categories).FirstOrDefaultAsync();
                evt.CheckExist("Event");
                if (string.IsNullOrEmpty(name.Trim()))
                {
                    throw new CoachOnlineException("Event name is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (string.IsNullOrEmpty(description.Trim()))
                {
                    throw new CoachOnlineException("Event description is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (string.IsNullOrEmpty(base64Pic.Trim()))
                {
                    throw new CoachOnlineException("Cover pictrue is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (categories == null || categories.Count == 0)
                {
                    throw new CoachOnlineException("Categories are not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (!startDate.HasValue)
                {
                    throw new CoachOnlineException("Event start date is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (!endDate.HasValue)
                {
                    throw new CoachOnlineException("Event end date is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (startDate >= endDate)
                {
                    throw new CoachOnlineException("Invalid date range", CoachOnlineExceptionState.DataNotValid);
                }

                string imageUrl = await SaveCoverPic(base64Pic);

                evt.Name = name;
                evt.Description = description;
                evt.EndDate = endDate.Value;
                evt.StartDate = startDate.Value;
                evt.Status = EventStatus.DRAFT;
                evt.CoverPictrueUrl = imageUrl;
                evt.CreationDate = DateTime.Now;

                if (evt.Categories != null)
                {
                    evt.Categories.RemoveAll(t => t.EventId == eventId);
                }
                else
                {
                    evt.Categories = new List<EventCategory>();
                }

                foreach (var c in categories)
                {
                    var cat = ctx.courseCategories.FirstOrDefault(t => t.Id == c.CategoryId);
                    cat.CheckExist("Category");
                    evt.Categories.Add(new EventCategory() { CategoryId = c.CategoryId });
                }

                await ctx.SaveChangesAsync();

                return evt.Id;
            }

        }

      
        public async Task CreateEvent(int eventId, int coachId)
        {
            using (var ctx = new DataContext())
            {
                var evtDraft = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId).Include(c => c.Categories).FirstOrDefaultAsync();
                evtDraft.CheckExist("Event");
                if (evtDraft.Status != EventStatus.DRAFT)
                {
                    throw new CoachOnlineException($"Event has invalid status. Event status {evtDraft.Status.ToString()}", CoachOnlineExceptionState.CantChange);
                }

                var result = CheckState(evtDraft);

                if (result)
                {

                    evtDraft.Status = EventStatus.CREATED;
                    evtDraft.EventChannelName = GetEventChannelName(evtDraft.Id, evtDraft.CoachId);

                    await ctx.SaveChangesAsync();
                }
            }
        }

        private string GetEventChannelName(int eventId, int coachId)
        {
            return $"{eventId}{rn.Next(1000, int.MaxValue)}{coachId}{ConvertTime.ToUnixTimestampLong(DateTime.UtcNow)}{rn.Next(1000, int.MaxValue)}";
        }

        private async Task<EventInfoResponse> FillEventApi(Model.Event ev, bool isPublic = false)
        {
            var ret = new EventInfoResponse();
            ret.Description = ev.Description;
            ret.Name = ev.Name;
            ret.Status = ev.Status;
            ret.StatusStr = ev.Status.ToString();
            ret.EventId = ev.Id;
            ret.CoachId = ev.CoachId;
            ret.CoverPictrueUrl = $"images/{ev.CoverPictrueUrl}";
            ret.Currency = ev.Currency;
            ret.EndDate = ev.EndDate;
            ret.StartDate = ev.StartDate;
            ret.PersonQty = ev.PersonQty;
            ret.Price = ev.Price;
            ret.Attachments = new List<EventAttachmentResponse>();
            ret.Categories = new List<EventCategoryResponse>();
            ret.Partners = new List<EventPartnerResponse>();

            if (ev.Attachments != null)
            {
                foreach (var a in ev.Attachments)
                {
                    ret.Attachments.Add(new EventAttachmentResponse() { Attachmentid = a.Id, AttachmentExtension = a.Extension, AttachmentName = a.Name, AttachmentUrl = $"attachments/{a.Hash}" });
                }
            }

            foreach (var c in ev.Categories)
            {
                ret.Categories.Add(new EventCategoryResponse() { EventCategoryId = c.Id, CategoryId = c.CategoryId, CategoryName = c.Category?.Name, ParentId = c.Category?.ParentId, ParentName = c.Category?.Parent?.Name });
            }

            ret.Coach = await FillUser(ev.CoachId, true);

            if (ev.Participants != null)
            {

                ret.ParticipantAssigned = ev.Participants.Where(t => t.Status == EventPaymentStatus.SUCCESS || t.Status == EventPaymentStatus.PENDING).Count();
            }

            if(ev.Partners != null)
            {
                foreach(var p in ev.Partners)
                {
                    ret.Partners.Add(new EventPartnerResponse() { PartnerId = p.Id, PhotoUrl = p.PhotoUrl, PartnerName = p.PartnerName });
                }
            }


            if (!isPublic)
            {
                ret.EventParticipants = new List<UserAPI>();
                if (ev.Participants != null)
                {
                    foreach (var p in ev.Participants)
                    {
                        ret.EventParticipants.Add(await FillUser(p.UserId));
                    }
                }
            }
            return ret;
        }

        private async Task<UserAPI> FillUser(int userId, bool isOwner = false)
        {
            var ret = new UserAPI();
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                ret.Id = userId;
                if (user != null)
                {

                    ret.Bio = user.Bio;
                    ret.City = user.City;
                    ret.Country = user.Country;
                    ret.Email = user.EmailAddress;
                    ret.FirstName = user.FirstName;
                    ret.LastName = user.Surname;
                    ret.PhotoUrl = user.AvatarUrl;
                    ret.UserRole = user.UserRole;
                    ret.YearOfBirth = user.YearOfBirth;
                    ret.Gender = user.Gender;
                    ret.PhoneNo = user.PhoneNo;
                    ret.SocialLogin = user.SocialLogin.HasValue && user.SocialLogin.Value;
                    if (isOwner)
                    {
                        ret.OwnedCourses = new List<CourseAPI>();

                        var courses = await ctx.courses.Where(t => t.UserId == userId && t.State == CourseState.APPROVED).Include(e => e.Episodes).Include(c => c.Category).ThenInclude(p => p.Parent).ToListAsync();

                        foreach (var c in courses)
                        {
                            var cApi = new CourseAPI();
                            cApi.Description = c.Description;
                            cApi.HasPromo = c.HasPromo.HasValue ? c.HasPromo.Value : false;
                            cApi.Id = c.Id;
                            cApi.Name = c.Name;
                            cApi.PhotoUrl = c.PhotoUrl;
                            cApi.State = c.State;
                            cApi.BannerPhotoUrl = c.BannerPhotoUrl;
                            cApi.Episodes = new List<EpisodeAPI>();
                            if (c.Episodes != null)
                            {
                                foreach (var ep in c.Episodes)
                                {
                                    var epApi = new EpisodeAPI();

                                    epApi.Description = ep.Description;
                                    epApi.Id = ep.Id;
                                    epApi.IsPromo = ep.IsPromo.HasValue ? ep.IsPromo.Value : false;
                                    epApi.Length = ep.MediaLenght;
                                    epApi.MediaId = ep.MediaId;
                                    epApi.OrdinalNumber = ep.OrdinalNumber;
                                    epApi.Title = ep.Title;
                                    epApi.EpisodeState = ep.EpisodeState;

                                    cApi.Episodes.Add(epApi);
                                }
                            }
                            cApi.Category = new CategoryAPI();
                            cApi.Category.AdultOnly = c.Category.AdultOnly;
                            cApi.Category.Id = c.CategoryId;
                            cApi.Category.Name = c.Category.Name;
                            cApi.Category.ParentId = c.Category.ParentId;
                            cApi.Category.ParentName = c.Category.Parent != null ? c.Category.Parent.Name : "";
                            ret.OwnedCourses.Add(cApi);
                        }
                    }
                }
            }

            return ret;
        }

        public async Task<List<EventInfoResponse>> GetUserEvents(int coachId)
        {
            var data = new List<EventInfoResponse>();
            using (var ctx = new DataContext())
            {
                var events = await ctx.Events.Where(t => t.CoachId == coachId)
                    .Include(a => a.Attachments)
                    .Include(p => p.Partners)
                    .Include(p => p.Participants)
                    .Include(c => c.Categories)
                    .ThenInclude(c => c.Category)
                    .ThenInclude(p => p.Parent)
                    .ToListAsync();

                foreach (var ev in events)
                {
                    var ret = await FillEventApi(ev);

                    data.Add(ret);
                }
            }

            return data;
        }

        public async Task<EventInfoResponse> GetUserEvent(int eventId, int coachId)
        {
            using (var ctx = new DataContext())
            {



                var ev = await ctx.Events
                    .Where(t => t.CoachId == coachId && t.Id == eventId)
                    .Include(a => a.Attachments)
                    .Include(p=>p.Partners)
                    .Include(p=>p.Participants)
                    .Include(c => c.Categories)
                    .ThenInclude(c => c.Category)
                    .ThenInclude(p => p.Parent)
                    .FirstOrDefaultAsync();
                ev.CheckExist("Event");

                var ret = await FillEventApi(ev, false);


                return ret;

            }
        }


        public async Task<List<EventInfoResponse>> GetIncomingEvents()
        {
            var retData = new List<EventInfoResponse>();
            using (var ctx = new DataContext())
            {
                var events = await ctx.Events.Where(t => t.Status == EventStatus.CREATED && t.StartDate > DateTime.Now).Include(a => a.Attachments).Include(p => p.Participants).Include(p => p.Partners).Include(c => c.Categories).ThenInclude(c => c.Category).ThenInclude(p => p.Parent).ToListAsync();

                foreach (var ev in events)
                {
                    var ret = await FillEventApi(ev, true);

                    retData.Add(ret);
                }
            }

            return retData;
        }

        public async Task UpdateEventField(int eventId, int coachId, string propertyName, object value)
        {
            propertyName = propertyName.ToLower();
            if (propertyName == "id" || propertyName == "coachid" || propertyName == "coach" || propertyName== "eventchannelname" || propertyName == "coachlivetoken" || propertyName == "status")
            {
                throw new CoachOnlineException($"Field {propertyName} cannot be updated", CoachOnlineExceptionState.DataNotValid);
            }

            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId && (t.Status == EventStatus.DRAFT || (t.Status == EventStatus.CREATED && t.StartDate> DateTime.Now))).Include(c => c.Categories).Include(a => a.Attachments).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                var propInfo = evt.GetType().GetProperties().FirstOrDefault(t => t.Name.ToLower() == propertyName);


                if (propertyName == "categories")
                {
                    var listVal = JsonConvert.DeserializeObject<List<EventCategoryRqs>>(value.ToString());
                    if (listVal == null)
                    {
                        throw new CoachOnlineException("Categories cannot be null", CoachOnlineExceptionState.DataNotValid);
                    }

                    if (evt.Categories != null)
                    {
                        evt.Categories.RemoveAll(t => t.EventId == eventId);
                    }
                    else
                    {
                        evt.Categories = new List<EventCategory>();
                    }

                    foreach (var c in listVal)
                    {
                        var cat = ctx.courseCategories.FirstOrDefault(t => t.Id == c.CategoryId);
                        cat.CheckExist("Category");
                        evt.Categories.Add(new EventCategory() { CategoryId = c.CategoryId });
                    }

                    await ctx.SaveChangesAsync();
                    return;
                }
                else if (propertyName == "attachments")
                {

                    var attachVal = JsonConvert.DeserializeObject<List<EventAttachmentRqs>>(value.ToString());
                    if (attachVal == null)
                    {
                        //remove existing attachments
                        if (evt.Attachments != null)
                        {
                            evt.Attachments.RemoveAll(t => t.EventId == eventId);
                        }
                    }
                    else
                    {
                        if (evt.Attachments == null)
                        {
                            evt.Attachments = new List<EventAttachment>();
                        }
                        foreach (var a in attachVal)
                        {
                            var res = await SaveAttachment(a.AttachmentBase64, a.AttachmentName);

                            evt.Attachments.Add(new EventAttachment() { EventId = eventId, Extension = res.Extension, Hash = res.HashName, Name = res.FileName });
                        }
                    }

                    await ctx.SaveChangesAsync();
                    return;
                }
                else if(propertyName == "partners")
                {
                    var partners = JsonConvert.DeserializeObject<List<EventPartnerRqs>>(value.ToString());
                    if (partners == null)
                    {
                        //remove existing attachments
                        if (evt.Partners != null)
                        {
                            evt.Partners.RemoveAll(t => t.EventId == eventId);
                        }
                    }
                    else
                    {
                        if (evt.Partners == null)
                        {
                            evt.Partners = new List<EventPartner>();
                        }
                        foreach (var a in partners)
                        {
                            var res = await SaveCoverPic(a.PhotoBase64);

                            evt.Partners.Add(new EventPartner() { EventId = eventId, PartnerName = a.PartnerName, PhotoUrl = res});
                        }
                    }

                    await ctx.SaveChangesAsync();
                    return;
                }
                else if (propertyName == "coverpictruebase64")
                {
                    var strVal = value.ToString().Trim();

                    if (strVal == "")
                    {
                        throw new CoachOnlineException($"{propertyName} cannot be empty", CoachOnlineExceptionState.DataNotValid);
                    }

                    var imgUrl = await SaveCoverPic(strVal);

                    evt.CoverPictrueUrl = imgUrl;


                    await ctx.SaveChangesAsync();
                    return;
                }
                else if(propertyName == "startdate")
                {
                    Console.WriteLine("Start date prop value is:" + value);
                    DateTime dtVal = Convert.ToDateTime(value);
                    Console.WriteLine("start date converted");
                    if (evt.Status != EventStatus.DRAFT)
                    {
                        throw new CoachOnlineException("Cannot update start date of a created event", CoachOnlineExceptionState.DataNotValid);
                    }

                    if (dtVal <= DateTime.Now.AddHours(24))
                    {
                        throw new CoachOnlineException("Cannot change event start date because the selected date is set in less than 24 hours.", CoachOnlineExceptionState.DataNotValid);
                    }

                    evt.StartDate = dtVal;



                    await ctx.SaveChangesAsync();

                    if(evt.Status == EventStatus.CREATED && evt.StartDate < evt.EndDate)
                    {
                        await InformUsersAboutEventChangedDate(evt.Id);
                    }

                    return;
                }
                else if(propertyName == "enddate")
                {
                    Console.WriteLine("End date prop value is:" + value);
                    DateTime dtVal = Convert.ToDateTime(value);
                    Console.WriteLine("end date converted");
                    if (evt.Status != EventStatus.DRAFT)
                    {
                        throw new CoachOnlineException("Cannot update start date of a created event", CoachOnlineExceptionState.DataNotValid);
                    }

                    evt.EndDate = dtVal;

                    await ctx.SaveChangesAsync();

                    return;
                }


                if (propInfo == null)
                {
                    throw new CoachOnlineException($"Property {propertyName} does not exist", CoachOnlineExceptionState.DataNotValid);
                }

                if (value == null)
                {
                    throw new CoachOnlineException($"{propertyName} cannot be null", CoachOnlineExceptionState.DataNotValid);
                }



                if (propInfo.PropertyType == typeof(string))
                {
                    var strVal = value.ToString().Trim();
                    if (strVal == "")
                    {
                        throw new CoachOnlineException($"{propertyName} cannot be empty", CoachOnlineExceptionState.DataNotValid);
                    }

                    propInfo.SetValue(evt, strVal);

                }
                else if (propInfo.PropertyType == typeof(decimal))
                {
                    decimal decimalVal = Convert.ToDecimal(value);
                    if (decimalVal < 0)
                    {
                        throw new CoachOnlineException($"Property value {propertyName} cannot be less than 0", CoachOnlineExceptionState.DataNotValid);
                    }
                    propInfo.SetValue(evt, decimalVal);
                }
                else if (propInfo.PropertyType == typeof(int))
                {
                    int intVal = Convert.ToInt32(value);
                    propInfo.SetValue(evt, intVal);
                }
                else if (propInfo.PropertyType == typeof(DateTime))
                {
                    DateTime dtVal = Convert.ToDateTime(value);

                   
                    propInfo.SetValue(evt, dtVal);
                }

                await ctx.SaveChangesAsync();

            }


        }

        public async Task UpdateEventDates(int coachId, int eventId, DateTime startDate, DateTime endDate)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId).FirstOrDefaultAsync();
                evt.CheckExist("Event");
                if(evt.Status != EventStatus.CREATED)
                {
                    throw new CoachOnlineException("The event is in a wrong state to update its dates",CoachOnlineExceptionState.CantChange);
                }

                if(startDate >= endDate)
                {
                    throw new CoachOnlineException("Start date cannot be bigger than end date", CoachOnlineExceptionState.DataNotValid);
                }

                if(startDate<= DateTime.Now.AddHours(24))
                {
                    throw new CoachOnlineException("Event cannot start in less than next 24 hours", CoachOnlineExceptionState.DataNotValid);
                }

                evt.StartDate = startDate;
                evt.EndDate = endDate;

                await ctx.SaveChangesAsync();

                await InformUsersAboutEventChangedDate(evt.Id);


            }
        }

        public async Task<int> UpdateEventDraft(int coachId, int eventId, int? personQty, decimal? price, string currency, List<EventAttachmentRqs> attachments)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId && t.CoachId == coachId && t.Status == EventStatus.DRAFT).Include(a => a.Attachments).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                if (string.IsNullOrEmpty(currency))
                {
                    throw new CoachOnlineException("Currency is not set", CoachOnlineExceptionState.DataNotValid);
                }

                if (!price.HasValue || price.Value <= 0)
                {
                    throw new CoachOnlineException("Price is not set", CoachOnlineExceptionState.DataNotValid);
                }
                if (!personQty.HasValue || personQty.Value <= 0)
                {
                    throw new CoachOnlineException("Person quantity is not set", CoachOnlineExceptionState.DataNotValid);
                }

                evt.PersonQty = personQty.Value;
                evt.Price = price.Value;
                evt.Currency = currency;

                if (evt.Attachments != null)
                {
                    evt.Attachments.RemoveAll(t => t.EventId == eventId);
                }
                else
                {
                    evt.Attachments = new List<EventAttachment>();
                }

                if (attachments != null)
                {
                    foreach (var a in attachments)
                    {
                        var res = await SaveAttachment(a.AttachmentBase64, a.AttachmentName);

                        evt.Attachments.Add(new EventAttachment() { EventId = eventId, Extension = res.Extension, Hash = res.HashName, Name = res.FileName });
                    }
                }



                await ctx.SaveChangesAsync();

                return evt.Id;
            }

        }

        private async Task InformUsersAboutEventChangedDate(int eventId)
        {
            using(var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId).Include(c=>c.Coach).Include(p => p.Participants).ThenInclude(u => u.User).FirstOrDefaultAsync();

                if (evt == null) return;

                if (!evt.Participants.Any()) return;

                foreach(var p in evt.Participants)
                {
                    if(p.Status == EventPaymentStatus.SUCCESS || p.Status == EventPaymentStatus.NOT_STARTED || p.Status == EventPaymentStatus.PENDING)
                    {
                        string body = "";
                        if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/InformAboutNewEventDates.html"))
                        {
                            body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/InformAboutNewEventDates.html");
                            body = body.Replace("###COACHESURL###", $"{Statics.ConfigData.Config.WebUrl}");
                            body = body.Replace("###EVENT_NAME###", $"{evt.Name}");
                            body = body.Replace("###TRAINER_NAME###", $"{evt.Coach.FirstName} {evt.Coach.Surname}");
                            body = body.Replace("###EVENT_START_DATE###", $"{evt.StartDate.ToString("dd.MM.yy HH:mm")}");
                            body = body.Replace("###EVENT_END_DATE###", $"{evt.EndDate.ToString("dd.MM.yy HH:mm")}");
                        }

                        if (body != "")
                        {
                            await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage
                            {
                                AuthorEmail = "info@coachs-online.com",
                                AuthorName = "Coachs-online",
                                Body = body,
                                ReceiverEmail = p.User.EmailAddress,
                                ReceiverName = $"{p.User.FirstName} {p.User.Surname}",
                                Topic = $"Changement des dates de l'événement '{evt.Name}'"
                            });
                        }
                    }
                }
            }
        }



        public async Task RefundCustomerPaymentForEvent(int userId, int assignationId, bool forEventDeletion = false)
        {
            using(var ctx = new DataContext())
            {
                var evtParticipant = await ctx.EventParticipants.Where(t=>t.Id == assignationId && t.UserId == userId).Include(e=>e.Event).Include(u=>u.User).FirstOrDefaultAsync();
                evtParticipant.CheckExist("Participant");
                if(evtParticipant.Event.Status == EventStatus.FINISHED)
                {
                    throw new CoachOnlineException("Event has alredy started. Cannot proceed refund", CoachOnlineExceptionState.CantChange);
                }
                if(!forEventDeletion && evtParticipant.Event.StartDate < DateTime.Now.AddHours(24))
                {
                    throw new CoachOnlineException("Cannot proceed refund because event starts in incoming 24 hours.", CoachOnlineExceptionState.CantChange);
                }
                if(evtParticipant.PayIntentId == null)
                {
                    throw new CoachOnlineException("Payment for event has not been created.", CoachOnlineExceptionState.CantChange);
                }


                if(evtParticipant.Status == EventPaymentStatus.REFUNDED)
                {
                    throw new CoachOnlineException("Payment for event has already been refunded.", CoachOnlineExceptionState.AlreadyChanged);
                }

                PaymentIntentService piSvc = new PaymentIntentService();
                var pIntent = await piSvc.GetAsync(evtParticipant.PayIntentId);

                if (pIntent.Status != "succeeded")
                {
                    if (pIntent.Status == "processing" && !forEventDeletion)
                    {
                        throw new CoachOnlineException("Payment for event has not been fully processed therefore it cannot be refunded.", CoachOnlineExceptionState.CantChange);
                    }
                    else if(pIntent.Status == "canceled" && !forEventDeletion)
                    {
                        throw new CoachOnlineException("Payment for event has already been canceled.", CoachOnlineExceptionState.CantChange);
                    }
                    else
                    {
                        var options = new PaymentIntentCancelOptions { };
                        var canceledIntent = await piSvc.CancelAsync(pIntent.Id, options);

                        evtParticipant.Status = EventPaymentStatus.PAYMENT_CANCELED;
                        await ctx.SaveChangesAsync();
                    }
                }
                else
                {

                    var refunds = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = pIntent.Id
                    };
                    var refund = refunds.CreateAsync(refundOptions);

                    evtParticipant.Status = EventPaymentStatus.REFUNDED;
                    await ctx.SaveChangesAsync();
                }

            }
        }

        public async Task<List<PaymentMethodsResponse>> GetPaymentMethods(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    throw new CoachOnlineException("User is not a stripe customer", CoachOnlineExceptionState.NotExist);
                }

                var options = new PaymentMethodListOptions
                {
                    Customer = user.StripeCustomerId,
                    Type = "card",
                };

                var data = new List<PaymentMethodsResponse>();
                var payService = new PaymentMethodService();
                var paymentMethods = await payService.ListAsync(options);

                foreach (var pm in paymentMethods)
                {
                    var v = new PaymentMethodsResponse();
                    v.PaymentMethodId = pm.Id;
                    v.Last4Digits = pm.Card.Last4;
                    v.Brand = pm.Card.Brand;
                    v.ExpMonth = (int)pm.Card.ExpMonth;
                    v.ExpYear = (int)pm.Card.ExpYear;
                    v.Country = pm.Card.Country;

                    data.Add(v);
                }

                return data;
            }
        }

        private async Task<bool> EventIsAvailableForAssignation(int eventId)
        {
            using (var ctx = new DataContext())
            {
                var evt = await ctx.Events.Where(t => t.Id == eventId).Include(p => p.Participants).FirstOrDefaultAsync();
                evt.CheckExist("Event");

                var enrolled = evt.Participants.Count(t => t.Status == EventPaymentStatus.SUCCESS || t.Status == EventPaymentStatus.PENDING);

                if (enrolled < evt.PersonQty)
                {
                    return true;
                }

                return false;
            }
        }

        public async Task<int> AssignForEvent(int userId, int eventId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");
                var evt = await ctx.Events.FirstOrDefaultAsync(t => t.Id == eventId && t.Status == EventStatus.CREATED);
                evt.CheckExist("Event");

                if (user.Id == evt.CoachId)
                {
                    throw new CoachOnlineException("You cannot participate in your own event", CoachOnlineExceptionState.DataNotValid);
                }



                var existing = await ctx.EventParticipants.FirstOrDefaultAsync(t => t.EventId == eventId && t.UserId == userId);
                if (existing != null)
                {
                    if (existing.Status == EventPaymentStatus.PENDING)
                    {
                        throw new CoachOnlineException("Your payment is processed. Cannot proceed.", CoachOnlineExceptionState.CantChange);
                    }
                    if (existing.Status == EventPaymentStatus.SUCCESS)
                    {
                        throw new CoachOnlineException("You already have paid for the event. Cannot proceed.", CoachOnlineExceptionState.CantChange);
                    }

                    if (!await EventIsAvailableForAssignation(eventId))
                    {
                        throw new CoachOnlineException("Limit of participants in the event was reached. Cannot proceed. Try again later.", CoachOnlineExceptionState.CantChange);
                    }
                    return existing.Id;
                }



                if (!await EventIsAvailableForAssignation(eventId))
                {
                    throw new CoachOnlineException("Limit of participants in the event was reached. Cannot proceed. try again later.", CoachOnlineExceptionState.CantChange);
                }

                EventParticipant ep = new EventParticipant();
                ep.UserId = user.Id;
                ep.EventId = evt.Id;
                ep.AssignDate = DateTime.Now;
                ep.Price = evt.Price;
                ep.Currency = evt.Currency;
                ep.Status = EventPaymentStatus.NOT_STARTED;

                ctx.EventParticipants.Add(ep);

                await ctx.SaveChangesAsync();

                return ep.Id;
            }
        }

        public async Task PayForEvent(int userId, int assignId, string paymentMethodId)
        {
            using (var ctx = new DataContext())
            {
                var ep = await ctx.EventParticipants.Where(t => t.Id == assignId).Include(e => e.Event).Include(u => u.User).FirstOrDefaultAsync();
                ep.CheckExist("Participant");

                if (!await EventIsAvailableForAssignation(ep.EventId))
                {
                    throw new CoachOnlineException("Limit of participants in the event was reached. Cannot proceed. try again later.", CoachOnlineExceptionState.CantChange);
                }

                if (ep.UserId != userId)
                {
                    throw new CoachOnlineException("User is not authorized to perform this action.", CoachOnlineExceptionState.NotAuthorized);
                }

                if (ep.Status == EventPaymentStatus.SUCCESS)
                {
                    throw new CoachOnlineException("Payment for this event already exists", CoachOnlineExceptionState.AlreadyExist);
                }
                if (string.IsNullOrEmpty(ep.User.StripeCustomerId))
                {
                    throw new CoachOnlineException("User is not a stripe customer", CoachOnlineExceptionState.NotExist);
                }

                PaymentIntentService pSvc = new PaymentIntentService();
                if (ep.Status == EventPaymentStatus.PENDING && ep.PayIntentId != null)
                {
                    var pIntPending = await pSvc.GetAsync(ep.PayIntentId);
                    if (pIntPending.Status == "succeeded")
                    {
                        ep.Status = EventPaymentStatus.SUCCESS;
                        await ctx.SaveChangesAsync();
                    }

                    if (ep.Status == EventPaymentStatus.SUCCESS)
                    {
                        throw new CoachOnlineException("Payment for this event already exists", CoachOnlineExceptionState.AlreadyExist);
                    }
                    else
                    {
                        //cancel not finished payment and clear payment data
                        await pSvc.CancelAsync(ep.PayIntentId);
                        ep.Status = EventPaymentStatus.PAYMENT_CANCELED;
                        await ctx.SaveChangesAsync();
                    }
                }

                PaymentIntentCreateOptions pi = new PaymentIntentCreateOptions();
                pi.Customer = ep.User.StripeCustomerId;
                pi.Currency = ep.Currency;
                pi.PaymentMethod = paymentMethodId;
                pi.ReceiptEmail = ep.User.EmailAddress;
                pi.Confirm = true;
                pi.Description = $"Payment for {ep.Event.Name} event participation";
                pi.ReturnUrl = $"{ConfigData.Config.WebUrl}/event/{ep.EventId}";
                pi.Amount = (long)(Math.Round(ep.Price, 2) * 100);


                
                var result = await pSvc.CreateAsync(pi);

                ep.PayIntentId = result.Id;
                ep.Status = EventPaymentStatus.PENDING;
                if (result.Status == "succeeded")
                {
                    ep.Status = EventPaymentStatus.SUCCESS;
                }
                else if (result.Status == "canceled")
                {
                    ep.Status = EventPaymentStatus.DECLINED;
                }

                await ctx.SaveChangesAsync();

            }
        }

        class AttachmentSaveResp
        {
            public string HashName { get; set; }
            public string Extension { get; set; }
            public string FileName { get; set; }
        }
        private async Task<AttachmentSaveResp> SaveAttachment(string base64, string name)
        {
            try
            {

                string hashName = LetsHash.RandomHash(DateTime.Now.ToString());

                var fileData = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
                var extension = "";
                var filename = name;
                if (fileData.Length >= 2)
                {
                    extension = fileData.Last();
                    filename = fileData.First();
                }

                var serverFilename = await Extensions.SaveAttachmentAsync(base64, hashName, extension);
                AttachmentSaveResp resp = new AttachmentSaveResp();
                resp.Extension = extension;
                resp.FileName = filename;
                resp.HashName = serverFilename;

                return resp;

            }
            catch (Exception ex)
            {
                throw new CoachOnlineException("Attachment incorrect data", CoachOnlineExceptionState.IncorrectFormat);
            }
        }

        private async Task<string> SaveCoverPic(string base64)
        {
            try
            {
                string hashName = Statics.LetsHash.RandomHash(DateTime.Now.ToString());
                await Helpers.Extensions.SaveImageAsync(base64, hashName);

                return hashName + ".jpg";
            }
            catch (Exception ex)
            {
                throw new CoachOnlineException("Cover pictrue incorrect data", CoachOnlineExceptionState.IncorrectFormat);
            }
        }

        private bool CheckState(Event e)
        {
            if (string.IsNullOrEmpty(e.Name))
            {
                throw new CoachOnlineException("Event name is not set", CoachOnlineExceptionState.DataNotValid);
            }

            if (string.IsNullOrEmpty(e.Description))
            {
                throw new CoachOnlineException("Event description is not set", CoachOnlineExceptionState.DataNotValid);
            }

            if (string.IsNullOrEmpty(e.CoverPictrueUrl))
            {
                throw new CoachOnlineException("Cover pictrue is not set", CoachOnlineExceptionState.DataNotValid);
            }

            if (e.Categories == null || e.Categories.Count == 0)
            {
                throw new CoachOnlineException("Categories are not set", CoachOnlineExceptionState.DataNotValid);
            }


            if (e.StartDate >= e.EndDate)
            {
                throw new CoachOnlineException("Invalid date range", CoachOnlineExceptionState.DataNotValid);
            }

            if (string.IsNullOrEmpty(e.Currency))
            {
                throw new CoachOnlineException("Currency is not set", CoachOnlineExceptionState.DataNotValid);
            }

            if (e.Price <= 0)
            {
                throw new CoachOnlineException("Price is not set", CoachOnlineExceptionState.DataNotValid);
            }
            if (e.PersonQty <= 0)
            {
                throw new CoachOnlineException("Person quantity is not set", CoachOnlineExceptionState.DataNotValid);
            }

            return true;
        }
    }


}

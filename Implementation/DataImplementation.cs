using CoachOnline.Model;
using CoachOnline.Statics;
using ITSAuth.Exceptions;
using ITSAuth.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using CoachOnline.Implementation.Exceptions;
using System.IO;
using System.Drawing;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Helpers;
using CoachOnline.ElasticSearch.Services;
using CoachOnline.Model.ApiResponses.Admin;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CoachOnline.Implementation
{
    public class DataImplementation : IAuthAsync, IDisposable, ICoachService
    {
        private readonly IUser _userSvc;
        private readonly ISubscription _subscriptionSvc;
        private readonly ISearch _searchSvc;
        private readonly ILogger<DataImplementation> _logger;
        private static DataImplementation _instance = null;
        public DataImplementation(IUser userSvc, ISubscription subscSvc, ISearch searchSvc, ILogger<DataImplementation> logger)
        {
            _userSvc = userSvc;
            _subscriptionSvc = subscSvc;
            _instance = this;
            _searchSvc = searchSvc;
            _logger = logger;
        }

        public async Task AssignCategoryToUser(CategoryToUserRequest request)
        {
            int userId = GetUserIdForToken(request.authToken);

            await AssignUserCategory(userId, request.CategoryId);
        }


        public async Task AssignUserCategory(int userId, int categoryId)
        {
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(p => p.Id == userId)
                    .Include(z => z.AccountCategories)
                    .FirstOrDefaultAsync();

                var category = await cnx.courseCategories.Where(z => z.Id == categoryId).FirstOrDefaultAsync();
                if (category != null)
                {
                    if (!user.AccountCategories.Any(p => p.Id == category.Id))
                    {
                        user.AccountCategories.Add(category);
                    }
                }

                await cnx.SaveChangesAsync();


            }
        }

        public async Task BlockCourse(int userId, int courseId, string userRole)
        {
            using(var ctx = new DataContext())
            {
                Course course = null;
                if (userRole == Model.UserRoleType.COACH.ToString())
                {
                    course = await ctx.courses.Where(t => t.Id == courseId && t.UserId == userId).Include(c=>c.Category).FirstOrDefaultAsync();
                }
                else if(userRole == Model.UserRoleType.ADMIN.ToString())
                {
                    course = await ctx.courses.Where(t => t.Id == courseId).Include(c => c.Category).FirstOrDefaultAsync();
                }
                course.CheckExist("Course");

                if(course.State == CourseState.BLOCKED)
                {
                    return;
                }

                if (course.State == CourseState.PENDING)
                {
                    course.State = CourseState.UNPUBLISHED;
                    await ctx.SaveChangesAsync();
                }
                else if(course.State == CourseState.APPROVED)
                {
                    course.State = CourseState.BLOCKED;
                    await ctx.SaveChangesAsync();

                    await _searchSvc.DeleteCourse(courseId);
                    await _searchSvc.ReaddCategory(course.Category.ParentId.HasValue ? course.Category.ParentId.Value : course.Category.Id);
                    await _searchSvc.ReaddCoach(course.UserId);
                }
                else
                {
                    throw new CoachOnlineException("Cannot block course that is not public or in verification.", CoachOnlineExceptionState.AlreadyChanged);
                }

             
               
            }
        }

        public async Task UnBlockCourse(int userId, int courseId, string userRole)
        {
            using (var ctx = new DataContext())
            {
                Course course = null;
                if (userRole == Model.UserRoleType.COACH.ToString())
                {
                    course = await ctx.courses.Where(t => t.Id == courseId && t.UserId == userId).Include(c=>c.Category).FirstOrDefaultAsync();
                }
                else if (userRole == Model.UserRoleType.ADMIN.ToString())
                {
                    course = await ctx.courses.Where(t => t.Id == courseId).Include(c => c.Category).FirstOrDefaultAsync();
                }
                course.CheckExist("Course");

                if (course.State == CourseState.BLOCKED)
                {
                    Console.WriteLine("unblocking");
                    course.State = CourseState.APPROVED;
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new CoachOnlineException("Cannot unblock course that is not in the state 'BLOCKED'", CoachOnlineExceptionState.DataNotValid);
                }

             

                await _searchSvc.ReaddCourse(courseId);
                await _searchSvc.ReaddCategory(course.Category.ParentId.HasValue? course.Category.ParentId.Value: course.Category.Id);
                await _searchSvc.ReaddCoach(course.UserId);
            }
        }

        public async Task DetachCategoryFromUser(CategoryToUserRequest request)
        {
            int userId = GetUserIdForToken(request.authToken);

            await DetachUserCategory(userId, request.CategoryId);
        }

        public async Task DetachUserCategory(int userId, int categoryId)
        {
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(p => p.Id == userId)
                    .Include(z => z.AccountCategories)
                    .FirstOrDefaultAsync();


                var cat = user.AccountCategories.Where(z => z.Id == categoryId).FirstOrDefault();
                if (cat != null)
                {
                    user.AccountCategories.Remove(cat);
                }

                await cnx.SaveChangesAsync();
            }

        }

        //Company data
        public async Task UpdateCompanyData(string AuthToken, string Name, string City, string SiretNumber,
        string BankAccountNumber, string RegisterAddress, string Country, string VatNumber, string ZipCode, string BICNumber)
        {
            if (!string.IsNullOrEmpty(BankAccountNumber))
            {

                if (!ValidateBankAccount(BankAccountNumber))
                {
                    throw new CoachOnlineException($"Account number {BankAccountNumber} is not valid IBAN.", CoachOnlineExceptionState.DataNotValid);
                }
            }
            int userId = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.companyInfo)
                    .FirstOrDefault();

                CheckExistAuth(user, "User");

                if (user.companyInfo == null)
                {
                    user.companyInfo = new CompanyInfo();
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    user.companyInfo.Name = Name;
                }

                if (!string.IsNullOrEmpty(City))
                {
                    user.companyInfo.City = City;
                }

                if (!string.IsNullOrEmpty(SiretNumber))
                {
                    user.companyInfo.SiretNumber = SiretNumber;
                }

                if (!string.IsNullOrEmpty(BankAccountNumber))
                {
                    user.companyInfo.BankAccountNumber = BankAccountNumber;
                }
                if (!string.IsNullOrEmpty(RegisterAddress))
                {
                    user.companyInfo.RegisterAddress = RegisterAddress;
                }
                if (!string.IsNullOrEmpty(Country))
                {
                    user.companyInfo.Country = Country;
                }
                if (!string.IsNullOrEmpty(VatNumber))
                {
                    user.companyInfo.VatNumber = VatNumber;
                }

                if (!string.IsNullOrEmpty(ZipCode))
                {
                    user.companyInfo.ZipCode = ZipCode;
                }

                if (!string.IsNullOrEmpty(BICNumber))
                {
                    var pattern = @"^[a-zA-Z0-9]+$";
                    if (!Regex.IsMatch(BICNumber, pattern) || BICNumber.Length < 8 || BICNumber.Length > 11)
                    {
                        _logger.LogInformation("Invalid BIC number");
                        throw new CoachOnlineException("Le numéro BIC doit comporter un minimum de 8 caractères, un maximum de 11 et peut comprendre des chiffres et des lettres.", CoachOnlineExceptionState.DataNotValid);
                    }
      
                    user.companyInfo.BICNumber = BICNumber;
                }

                await cnx.SaveChangesAsync();

            }
        }
        //Lessons and courses
        public async Task RemoveMediaFromEpisode(string AuthToken, int CourseId, int EpisodeId)
        {
            int userId = await GetUserIdForTokenAsync(AuthToken);

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes.OrderBy(z => z.OrdinalNumber).ThenBy(y => y.Id))
                    //.ThenInclude(x => x.Attachments)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync();


                if (!user.OwnedCourses.Any(z => z.Id == CourseId))
                {
                    throw new CoachOnlineException("User is not owner of this course.", CoachOnlineExceptionState.PermissionDenied);
                }
                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                if (course == null || !course.Episodes.Any(x => x.Id == EpisodeId))
                {
                    throw new CoachOnlineException($"This Course has no Lesson/Episode with Id {EpisodeId} ", CoachOnlineExceptionState.NotExist);
                }
                var episode = course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                if (episode == null)
                {
                    throw new CoachOnlineException("Episode not exist", CoachOnlineExceptionState.NotExist);
                }

                string FileName = episode.MediaId;
                episode.MediaId = "";
                episode.MediaLenght = 0;
                await cnx.SaveChangesAsync();

                if (!string.IsNullOrEmpty(FileName))
                {

                    try
                    {
                        Helpers.Extensions.RemoveFile(FileName, FileType.Video);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);

                    }
                }


            }

        }
        public async Task RemoveAttachmentFromEpisode(string AuthToken, int CourseId, int EpisodeId, int AttachmentId)
        {
            var u = await _userSvc.GetUserByTokenAsync(AuthToken);
            u.CheckExist("User");
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException($"Wrong user account. Current user is: {u.UserRole}", CoachOnlineExceptionState.DataNotValid);
            }
            int userId = u.Id;
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes.OrderBy(z => z.OrdinalNumber).ThenBy(y => y.Id))
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync();

                if (!user.OwnedCourses.Any(z => z.Id == CourseId))
                {
                    throw new CoachOnlineException("User is not owner of this course.", CoachOnlineExceptionState.PermissionDenied);
                }
                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                if (course == null || !course.Episodes.Any(x => x.Id == EpisodeId))
                {
                    throw new CoachOnlineException($"This Course has no Lesson/Episode with Id {EpisodeId} ", CoachOnlineExceptionState.NotExist);
                }
                var episode = course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                if (episode == null)
                {
                    throw new CoachOnlineException("Episode not exist", CoachOnlineExceptionState.NotExist);
                }

                if (episode.Attachments == null)
                {
                    throw new CoachOnlineException("Can't find any attachments.", CoachOnlineExceptionState.DataNotValid);
                }

                var attachment = episode.Attachments.Where(x => x.Id == AttachmentId).FirstOrDefault();
                if (attachment == null)
                {
                    throw new CoachOnlineException($"Attachment with id {AttachmentId} not exist.", CoachOnlineExceptionState.NotExist);
                }

                string FileName = $"{attachment.Hash}.{attachment.Extension}";

                episode.Attachments.Remove(attachment);
                await cnx.SaveChangesAsync();

                try
                {
                    Helpers.Extensions.RemoveFile(FileName, FileType.Attachment);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.Now} File {FileName} cannot be deleted.");
                }


            }
        }

        //UserData

        public async Task<UserBasicDataResponse> GetUserBasicData(string AuthToken)
        {
            UserBasicDataResponse response = new UserBasicDataResponse();
            int userId = await GetUserIdForTokenAsync(AuthToken);

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.companyInfo)



                    .Include(z => z.AccountCategories)

                    .FirstOrDefaultAsync();

                user.CheckExist("User");

                if (user.UserRole == UserRoleType.ADMIN)
                {
                    throw new CoachOnlineException($"Wrong user account. Current user is: {user.UserRole}", CoachOnlineExceptionState.DataNotValid);
                }



                response.UserId = user.Id;
                response.Email = user.EmailAddress;
                response.Bio = user.Bio ?? "";
                response.City = user.City ?? "";
                response.Gender = user.Gender ?? "";
                response.FirstName = user.FirstName ?? "";
                response.PhotoUrl = user.AvatarUrl ?? "";
                response.LastName = user.Surname ?? "";
                response.Country = user.Country ?? "";
                response.Address = user.Adress ?? "";
                response.PostalCode = user.PostalCode ?? "";
                response.PhoneNo = user.PhoneNo ?? "";
                response.SocialLogin = user.SocialLogin.HasValue && user.SocialLogin.Value;
                response.UserRole = user.UserRole.ToString();
                response.AffiliatorType = user.AffiliatorType;
                response.categories = new List<CategoryAPI>();

                if (user.AccountCategories != null && user.AccountCategories.Count > 0)
                {
                    foreach (var uc in user.AccountCategories)
                    {

                        response.categories.Add(new CategoryAPI { AdultOnly = uc.AdultOnly, Id = uc.Id, Name = uc.Name ?? "" });
                    }
                }



                response.YearOfBirth = user.YearOfBirth;


                if (user.WithdrawalsEnabled)
                {
                    response.StripeVerificationStatus = 3;
                }
                else if (user.PaymentsEnabled)
                {
                    response.StripeVerificationStatus = 2;

                }
                else if (!string.IsNullOrEmpty(user.StripeAccountId))
                {
                    response.StripeVerificationStatus = 1;

                }
                else
                {
                    response.StripeVerificationStatus = 0;

                }
                if (user.companyInfo != null)
                {
                    response.CompanyInfo = new UserBasicDataCompanyInfo();
                    response.CompanyInfo.BankAccountNumber = user.companyInfo.BankAccountNumber ?? "";
                    response.CompanyInfo.City = user.companyInfo.City ?? "";
                    response.CompanyInfo.Country = user.companyInfo.Country ?? "";
                    response.CompanyInfo.Name = user.companyInfo.Name ?? "";
                    response.CompanyInfo.RegisterAddress = user.companyInfo.RegisterAddress ?? "";
                    response.CompanyInfo.SiretNumber = user.companyInfo.SiretNumber ?? "";
                    response.CompanyInfo.VatNumber = user.companyInfo.VatNumber ?? "";
                    response.CompanyInfo.PostCode = user.companyInfo.ZipCode ?? "";
                    response.CompanyInfo.BICNumber = user.companyInfo.BICNumber ?? "";
                }

            }

            return response;
        }

        public async Task UpdateUserData(string AuthToken, string Name, string Surname, int? YearOfBirth, string City, string Gender, string Bio, int UserCategory, string phoneNo, string country, string PostalCode, string address)
        {
            int userId = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)

                    .FirstOrDefaultAsync();

                user.CheckExist("User");
                if (user.UserRole != UserRoleType.COACH)
                {
                    throw new CoachOnlineException($"Wrong user account. Current user is: {user.UserRole}", CoachOnlineExceptionState.DataNotValid);
                }

                if (!string.IsNullOrEmpty(City))
                {
                    user.City = City;
                }
                if (!string.IsNullOrEmpty(country))
                {
                    user.Country = country;
                }

                if (!string.IsNullOrEmpty(PostalCode))
                {
                    user.PostalCode = PostalCode;
                }

                if (!string.IsNullOrEmpty(address))
                {
                    user.Adress = address;
                }
                

                if (!string.IsNullOrEmpty(Name))
                {
                    user.FirstName = Name;
                }

                if (!string.IsNullOrEmpty(Surname))
                {
                    user.Surname = Surname;
                }
                if (!string.IsNullOrEmpty(Gender))
                {
                    user.Gender = Gender;
                }
                if (!string.IsNullOrEmpty(Bio))
                {
                    user.Bio = Bio;
                }
                if (!string.IsNullOrEmpty(phoneNo))
                {
                    user.PhoneNo = phoneNo;
                }



                if (YearOfBirth.HasValue)
                {
                    if (YearOfBirth == 0 || YearOfBirth < 1900 || YearOfBirth >= DateTime.Today.Year - 1)
                    {
                        throw new CoachOnlineException("Wrong date of birth", CoachOnlineExceptionState.DataNotValid);
                    }

                    user.YearOfBirth = YearOfBirth.Value;
                }
                else
                {
                    user.YearOfBirth = null;
                }



                await cnx.SaveChangesAsync();





            }
        }





        public async Task<List<EpisodeAttachment>> AddAttachmentToEpisode(string AuthToken, string AttachmentBase64, int CourseId, int EpisodeId, string Extension, string AttachmentName)
        {
            var u = await _userSvc.GetUserByTokenAsync(AuthToken);
            u.CheckExist("User");
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException($"Wrong user account. Current user is: {u.UserRole}", CoachOnlineExceptionState.DataNotValid);
            }
            //int AttachmentId = 0;
            int userId = u.Id;
            List<EpisodeAttachment> attachments = new List<EpisodeAttachment>();
            if (string.IsNullOrEmpty(AttachmentBase64))
            {
                throw new CoachOnlineException("File weight is 0 bytes. It can't be empty.", CoachOnlineExceptionState.DataNotValid);
            }

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes.OrderBy(z => z.OrdinalNumber).ThenBy(y => y.Id))
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync();

                if (!user.OwnedCourses.Any(z => z.Id == CourseId))
                {
                    throw new CoachOnlineException("User is not owner of this course.", CoachOnlineExceptionState.PermissionDenied);
                }
                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                if (course == null || !course.Episodes.Any(x => x.Id == EpisodeId))
                {
                    throw new CoachOnlineException($"This Course has no Lesson/Episode with Id {EpisodeId} ", CoachOnlineExceptionState.NotExist);
                }
                var episode = course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                if (episode == null)
                {
                    throw new CoachOnlineException("Episode not exist", CoachOnlineExceptionState.NotExist);
                }
                string FileName = LetsHash.RandomHash(DateTime.Now.ToString());

                await SaveFile(FileName, Extension, AttachmentBase64);

                EpisodeAttachment attachment = new EpisodeAttachment
                {
                    Added = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    Extension = Extension,
                    Hash = FileName,
                    Name = AttachmentName ?? ""
                };

                if (episode.Attachments == null)
                {
                    episode.Attachments = new List<EpisodeAttachment>();

                }
                episode.Attachments.Add(attachment);
                var previousCourseState = course.State;
                if (course.State == CourseState.APPROVED || course.State == CourseState.BLOCKED)
                {
                    course.State = CourseState.PENDING;

                }


                await cnx.SaveChangesAsync();

                attachments = episode.Attachments.ToList();


                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(course.Id);
                }


            }
            return attachments;
        }




        private async Task SaveFile(string Hash, string Extension, string FileData)
        {
            byte[] data = await Task.Run(() =>
            {
                return Convert.FromBase64String(FileData);
            });
            await File.WriteAllBytesAsync($"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/{Hash}.{Extension}", data);
        }



        public async Task<List<CourseResponse>> GetCoursesForOwnerAsync(string AuthToken)
        {
            List<CourseResponse> courses = new List<CourseResponse>();

            int userId = await GetUserIdForTokenAsync(AuthToken);

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes.OrderBy(z => z.OrdinalNumber).ThenBy(y => y.Id))
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Category)
                    .ThenInclude(x => x.Parent)
                    .ThenInclude(x => x.Children)

                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.RejectionsHistory)
                    .FirstOrDefaultAsync();




                var coursesDirty = user
                    .OwnedCourses
                    .OrderByDescending(x => x.Id)
                    .ToList();


                //creating response object
                foreach (var c in coursesDirty)
                {
                    CourseResponse courseResponse = new CourseResponse();
                    courseResponse.Episodes = new List<EpisodeResponse>();
                    courseResponse.RejectionsHistory = new List<RejectionResponse>();
                    courseResponse.Category = new CategoryAPI();
                    //courseResponse.Category.children = new List<CategoryChildrenResponse>();
                    courseResponse.Category.ParentsChildren = new List<CategoryAPI>();

                    courseResponse.Created = c.Created;
                    courseResponse.Description = c.Description;
                    courseResponse.Id = c.Id;
                    courseResponse.Name = c.Name ?? "";
                    courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                    courseResponse.State = c.State;
                    courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";

                    if (c.RejectionsHistory != null && c.RejectionsHistory.Count > 0)
                    {
                        foreach (var rj in c.RejectionsHistory)
                        {
                            courseResponse.RejectionsHistory.Add(new RejectionResponse
                            {
                                Id = rj.Id,
                                Date = rj.Date,
                                Reason = rj.Reason ?? ""
                            });
                        }
                    }


                    if (c.Episodes != null && c.Episodes.Count > 0)
                    {
                        foreach (var e in c.Episodes)
                        {
                            EpisodeResponse episodeResponse = new EpisodeResponse();
                            if (e.Attachments != null && e.Attachments.Count > 0)
                            {
                                episodeResponse.Attachments = e.Attachments;
                            }
                            else
                            {
                                e.Attachments = new List<EpisodeAttachment>();
                            }

                            episodeResponse.Created = e.Created;
                            episodeResponse.Description = e.Description ?? "";
                            episodeResponse.Id = e.Id;
                            episodeResponse.MediaId = e.MediaId;
                            episodeResponse.OrdinalNumber = e.OrdinalNumber;
                            episodeResponse.Title = e.Title ?? "";
                            episodeResponse.NeedConversion = e.MediaNeedsConverting;
                            episodeResponse.Length = e.MediaLenght;
                            episodeResponse.CourseId = e.CourseId;
                            episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                            episodeResponse.EpisodeState = e.EpisodeState;

                            courseResponse.Episodes.Add(episodeResponse);



                        }
                    }

                    if (c.Category != null)
                    {
                        courseResponse.Category.Name = c.Category.Name;
                        courseResponse.Category.Id = c.Category.Id;
                        courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                        if (c.Category.Parent != null)
                        {
                            courseResponse.Category.ParentId = c.Category.Parent.Id;
                            courseResponse.Category.ParentName = c.Category.Parent.Name;
                            if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                            {
                                foreach (var pc in c.Category.Parent.Children)
                                {
                                    courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                                }
                            }
                        }
                    }


                    courses.Add(courseResponse);
                }

            }

            return courses;
        }

        public async Task<GetCategoriesResponse> GetCategories()
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            response.items = new List<GetAdminCategoriesItem>();

            using (var cnx = new DataContext())
            {
                var cats = cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Select(x => x).ToList();
                foreach (var c in cats)
                {
                    if (c.Parent == null)
                    {
                        var catToAdd = new GetAdminCategoriesItem
                        {
                            Id = c.Id,
                            Name = c.Name ?? "",

                        };


                        if (c.Children != null)
                        {
                            catToAdd.Children = new List<GetAdminCategoriesFamily>();
                            foreach (var cc in c.Children)
                            {
                                catToAdd.Children.Add(new GetAdminCategoriesFamily { Id = cc.Id, Name = cc.Name });
                            }

                            catToAdd.Children = catToAdd.Children.OrderBy(t => t.Name).ToList();
                        }
                        response.items.Add(catToAdd);
                    }
                }

                response.items = response.items.OrderBy(t => t.Name).ToList();
            }

            return response;
        }

        public async Task<GetCategoriesResponse> GetCategoriesCompleted()
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            response.items = new List<GetAdminCategoriesItem>();

            using (var cnx = new DataContext())
            {
                var cats = cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Include(x => x.CategoryCourses)
                    .Select(x => x).ToList();
                
                var tmpcats = new List<bool>();
                // var tmpIndex = 0;
                foreach (var c in cats)
                {
                    // Console.WriteLine(c);
                    var courseExist = false;
                    if (c.CategoryCourses != null) {
                        foreach (var course in  c.CategoryCourses) {
                            if (course.State == CourseState.APPROVED)
                            {
                                courseExist = true;
                            //     break;
                            }
                        }
                    }
                    tmpcats.Add(courseExist);

                    // Console.WriteLine(courseExist);

                    // if (!courseExist)
                    // {
                    //     tmpcats.Delete(c);
                    // //     cats.Remove(c);
                    // }

                    // tmpIndex += 1;
                }
                
                var index = 0;
                foreach (var c in cats)
                {
                    if (tmpcats[index]) {
                        if (c.Parent == null)
                        {
                            var catToAdd = new GetAdminCategoriesItem
                            {
                                Id = c.Id,
                                Name = c.Name ?? "",

                            };

                            if (c.Children != null)
                            {
                                catToAdd.Children = new List<GetAdminCategoriesFamily>();
                                foreach (var cc in c.Children)
                                {
                                    catToAdd.Children.Add(new GetAdminCategoriesFamily { Id = cc.Id, Name = cc.Name });
                                }

                                catToAdd.Children = catToAdd.Children.OrderBy(t => t.Name).ToList();
                            }
                            response.items.Add(catToAdd);
                        }
                    }
                    index += 1;
                }

                response.items = response.items.OrderBy(t => t.Name).ToList();
            }

            return response;
        }

        public async Task<GetCategoriesResponse> GetCategoriesForUsers()
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            response.items = new List<GetAdminCategoriesItem>();

            using (var cnx = new DataContext())
            {
                var cats = await cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Select(x => x).ToListAsync();
                foreach (var c in cats)
                {
                    //if (c.Parent == null)
                    //{
                    var catToAdd = new GetAdminCategoriesItem
                    {
                        Id = c.Id,
                        Name = c.Name ?? "",

                    };


                    //if (c.Children != null)
                    //{
                    //    catToAdd.Children = new List<GetAdminCategoriesFamily>();
                    //    foreach (var cc in c.Children)
                    //    {
                    //        catToAdd.Children.Add(new GetAdminCategoriesFamily { Id = cc.Id, Name = cc.Name });
                    //    }

                    //    catToAdd.Children = catToAdd.Children.OrderBy(t => t.Name).ToList();
                    //}
                    response.items.Add(catToAdd);
                    //}
                }

                response.items = response.items.OrderBy(t => t.Name).ToList();
            }

            return response;
        }


        public async Task<int> CreateCourseAsync(string AuthToken, string Name, int category_id, string Description, string PhotoUrl)
        {
            var u = await _userSvc.GetUserByTokenAsync(AuthToken);
            u.CheckExist("User");
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException($"Wrong user account. Current user is: {u.UserRole}", CoachOnlineExceptionState.DataNotValid);
            }
            int courseId = 0;

            int UserId = u.Id;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Where(x => x.Id == UserId)
                    .Include(x => x.OwnedCourses)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User");
                //if (user.OwnedCourses == null)
                //{
                //    user.OwnedCourses = new List<Course>();
                //}

                Course course = new Course();


                if (category_id != 0)
                {

                    var category = await cnx.courseCategories.Where(x => x.Id == category_id).FirstOrDefaultAsync();
                    CheckExistAuth(category, "Category");
                }

                course.CategoryId = category_id;
                course.UserId = user.Id;
                course.Description = Description ?? "";
                course.Name = Name ?? "";
                course.PhotoUrl = PhotoUrl ?? "";
                course.State = CourseState.UNPUBLISHED;
                course.Created = ConvertTime.ToUnixTimestamp(DateTime.Now);
                course.PublishedCount = 0;
                await cnx.courses.AddAsync(course);
                // user.OwnedCourses.Add(course);
                await cnx.SaveChangesAsync();
                courseId = course.Id;

            }


            return courseId;
        }


        public async Task SubmitCourse(string AuthToken, int CourseId)
        {
            var u = await _userSvc.GetUserByTokenAsync(AuthToken);
            u.CheckExist("User");
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException($"Wrong user account. Current user is: {u.UserRole}", CoachOnlineExceptionState.DataNotValid);
            }
            int UserId = u.Id;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Where(x => x.Id == UserId)
                    .Include(x => x.OwnedCourses)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User");

                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                if (course == null)
                {
                    throw new CoachOnlineException("User is not owner of this course.", CoachOnlineExceptionState.NotAuthorized);
                }

                if (course.State == CourseState.PENDING)
                {
                    throw new CoachOnlineException("Course is already Pending.", CoachOnlineExceptionState.AlreadyChanged);
                }

                if (course.State != CourseState.UNPUBLISHED && course.State != CourseState.REJECTED)
                {
                    throw new CoachOnlineException("Course state not allows user to send it to verification.", CoachOnlineExceptionState.CantChange);
                }

                course.State = CourseState.PENDING;

                await cnx.SaveChangesAsync();


            }

        }



        public async Task UpdateCourseDetailsAsync(string AuthToken, int CourseId, string Name, int Category, string Description, string PhotoUrl)
        {
            var u = await _userSvc.GetUserByTokenAsync(AuthToken);
            u.CheckExist("User");
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException($"Wrong user account. Current user is: {u.UserRole}", CoachOnlineExceptionState.DataNotValid);
            }
            int UserId = u.Id;
            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                   .Where(x => x.Id == UserId)
                   .Include(x => x.OwnedCourses)
                   .FirstOrDefaultAsync();
                CheckExistAuth(user, "User");

                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                if (course == null)
                {
                    throw new CoachOnlineException("User is not owner of this Course, or Course does not exist.", CoachOnlineExceptionState.PermissionDenied);
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    course.Name = Name;
                }
                if (Category != 0)
                {
                    var category = await cnx.courseCategories.Where(x => x.Id == Category).FirstOrDefaultAsync();
                    CheckExistAuth(category, "Category");
                    course.Category = category;
                }
                else
                {
                    course.Category = null;
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    course.Description = Description;
                }
                if (!string.IsNullOrEmpty(PhotoUrl))
                {
                    course.PhotoUrl = PhotoUrl;
                }

                var previousCourseState = course.State;
                if (course.State == CourseState.APPROVED || course.State == CourseState.BLOCKED)
                {
                    course.State = CourseState.PENDING;
                   
                }

                await cnx.SaveChangesAsync();


                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(course.Id);
                }

            }
        }

        public async Task<string> UpdateProfileAvatar(string AuthToken, string PhotoBase)
        {
            int UserId = await GetUserIdForTokenAsync(AuthToken);
            string generateName = $"{Statics.LetsHash.RandomHash($"{DateTime.Now}")}";
            SaveImage(PhotoBase, generateName);
            generateName = generateName + ".jpg";
            string oldAvatar = "";

            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == UserId).FirstOrDefault();
                CheckExistAuth(user, "User");
                oldAvatar = user.AvatarUrl;
                user.AvatarUrl = generateName;
                await cnx.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(oldAvatar))
            {
                try
                {
                    Helpers.Extensions.RemoveFile(oldAvatar, FileType.Image);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }



            return generateName;
        }


        public async Task RemoveAvatar(string authToken)
        {
            int UserId = await GetUserIdForTokenAsync(authToken);
            string ActualAvatar = "";
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == UserId).FirstOrDefault();
                CheckExistAuth(user, "User");
                if (user.AvatarUrl != null && user.AvatarUrl != "")
                {
                    ActualAvatar = user.AvatarUrl;
                }
                user.AvatarUrl = "";
                await cnx.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(ActualAvatar))
            {
                try
                {
                    Helpers.Extensions.RemoveFile(ActualAvatar, FileType.Image);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }

        }

        public async Task<string> UploadPhoto(string authToken, string PhotoBase)
        {
            int UserId = await GetUserIdForTokenAsync(authToken);
            string generateName = Statics.LetsHash.RandomHash($"{DateTime.Now}");
            SaveImage(PhotoBase, generateName);
            return $"images/{generateName}.jpg";

        }

        public async Task<UploadCoursePhotoResponse> UploadCoursePhoto(string AuthToken, string PhotoBase, int CourseId)
        {
            var resp = new UploadCoursePhotoResponse();
            int UserId = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                   .Where(x => x.Id == UserId)
                   .Include(x => x.OwnedCourses)
                   .FirstOrDefaultAsync();
                CheckExistAuth(user, "User");
                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                course.CheckExist("Course");

                string generateName = Statics.LetsHash.RandomHash($"{DateTime.Now}");
                await Task.Run(() => SaveImage(PhotoBase, generateName));
                var imageUrl = $"images/{generateName}.jpg";

                course.PhotoUrl = imageUrl;
                var previousCourseState = course.State;
                if (course.State == CourseState.APPROVED || course.State == CourseState.BLOCKED)
                {
                    course.State = CourseState.PENDING;

                }

                await cnx.SaveChangesAsync();


                await cnx.SaveChangesAsync();
                resp.PhotoPath = imageUrl;
                resp.CourseId = course.Id;


                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(course.Id);
                }
            }

            return resp;
        }




        public static void SaveImage(string base64, string name)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                using (Bitmap bm2 = new Bitmap(ms))
                {
                    bm2.Save("wwwroot/images/" + $"{name}.jpg");
                }
            }
        }
        public async Task RemoveCourseAsync(string authToken, int CourseId)
        {
            int userId = await GetUserIdForTokenAsync(authToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == userId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.RejectionsHistory)
                    .FirstOrDefaultAsync();
                CheckExistAuth(user, "User");
                var course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExistAuth(course, "Course");
                var courseGlobal = cnx.courses.Where(z => z.Id == CourseId).FirstOrDefault();
                foreach (var f in courseGlobal.Episodes)
                {
                    try
                    {
                        Helpers.Extensions.RemoveFile(f.MediaId, FileType.Video);
                    }
                    catch (Exception) { }
                }
                if (courseGlobal != null)
                {
                    cnx.courses.Remove(courseGlobal);
                }
                //if (course.RejectionsHistory != null && course.RejectionsHistory.Count > 0)
                //{
                //    List<Rejection> rejectionsRemove = new List<Rejection>();
                //    foreach (var ah in course.RejectionsHistory)
                //    {
                //        rejectionsRemove.Add(ah);
                //    }
                //    foreach (var rj in rejectionsRemove)
                //    {
                //        course.RejectionsHistory.Remove(rj);
                //    }
                //}
                //foreach (var e in course.Episodes)
                //{
                //    await RemoveEpisodeFromCourse(authToken, CourseId, e.Id);
                //}
                //cnx.courses.Remove(course);
                await cnx.SaveChangesAsync();

                //await _searchSvc.DeleteCourse(CourseId);
                //await _searchSvc.ReaddCategory(course.CategoryId);
                //await _searchSvc.ReaddCoach(course.UserId);
                await _searchSvc.ReindexAll();
            }

        }

        public async Task<int> AddPromoEpisodeToCourse(string AuthToken, int CourseId, string Title, string Description)
        {
            int LessonId = 0;
            var usr = await _userSvc.GetUserByTokenAllowNullAsync(AuthToken);

            if (usr == null)
            {
                usr = await _userSvc.GetAdminByTokenAsync(AuthToken);
            }

            usr.CheckExist("User");
            using (var cnx = new DataContext())
            {
                Course course = null;
                if (usr.UserRole == UserRoleType.ADMIN)
                {
                    course = await cnx.courses.Where(x => x.Id == CourseId).Include(e => e.Episodes).FirstOrDefaultAsync();
                    course.CheckExist("Course");
                }
                else
                {
                    var user = await cnx.users
                        .Where(x => x.Id == usr.Id)
                        .Include(x => x.OwnedCourses)
                        .ThenInclude(x => x.Episodes)
                        .FirstOrDefaultAsync();
                    CheckExistAuth(user, "User");
                    course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                    if (course == null)
                    {
                        throw new CoachOnlineException("User does not own this Course, or Course does not exist.", CoachOnlineExceptionState.PermissionDenied);
                    }
                }
                if (course.Episodes == null)
                {
                    course.Episodes = new List<Episode>();
                }


                var promoEpisode = course.Episodes.FirstOrDefault(x => x.IsPromo.HasValue && x.IsPromo.Value == true);
                if (promoEpisode != null)
                {
                    Helpers.Extensions.RemoveFile(promoEpisode.MediaId, FileType.Video);
                    cnx.Episodes.Remove(promoEpisode);
                }

                Episode episode = new Episode
                {
                    Description = Description ?? "",
                    Title = Title ?? ""
                };


                episode.OrdinalNumber = -1;
                episode.IsPromo = true;

                course.HasPromo = true;

                episode.Created = ConvertTime.ToUnixTimestamp(DateTime.Now);
                episode.EpisodeState = EpisodeState.BEFORE_UPLOAD;
                course.Episodes.Add(episode);
                var previousCourseState = course.State;
                if (course.State == CourseState.APPROVED || course.State == CourseState.BLOCKED)
                {
                    course.State = CourseState.PENDING;

                }


                await cnx.SaveChangesAsync();
                LessonId = episode.Id;


                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(course.Id);
                }
            }



            return LessonId;
        }

        public async Task<int> AddEpisodeToCourse(string AuthToken, int CourseId, string Title, string Description)
        {
            int LessonId = 0;
            var usr = await _userSvc.GetUserByTokenAllowNullAsync(AuthToken);

            if (usr == null)
            {
                usr = await _userSvc.GetAdminByTokenAsync(AuthToken);
            }
            usr.CheckExist("User");
            using (var cnx = new DataContext())
            {
                User user = null;
                Course course = null;
                if (usr.UserRole == UserRoleType.ADMIN)
                {
                    //leave it be
                    course = await cnx.courses.Where(x => x.Id == CourseId).Include(e => e.Episodes).FirstOrDefaultAsync();
                    course.CheckExist("Course");
                }
                else
                {
                    user = await cnx.users
                        .Where(x => x.Id == usr.Id)
                        .Include(x => x.OwnedCourses)
                        .ThenInclude(x => x.Episodes)
                        .FirstOrDefaultAsync();
                    CheckExistAuth(user, "User");
                    course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                    if (course == null)
                    {
                        throw new CoachOnlineException("User does not own this Course, or Course does not exist.", CoachOnlineExceptionState.PermissionDenied);
                    }
                }
                if (course.Episodes == null)
                {
                    course.Episodes = new List<Episode>();
                }


                Episode episode = new Episode
                {
                    Description = Description ?? "",
                    Title = Title ?? ""
                };
                if (course.Episodes.Count == 0)
                {
                    episode.OrdinalNumber = 0;
                }
                else
                {
                    episode.OrdinalNumber = course.Episodes.OrderBy(x => x.OrdinalNumber).Last().OrdinalNumber + 1;

                }


                episode.Created = ConvertTime.ToUnixTimestamp(DateTime.Now);
                episode.EpisodeState = EpisodeState.BEFORE_UPLOAD;
                course.Episodes.Add(episode);
                var previousCourseState = course.State;
                if (course.State == CourseState.APPROVED || course.State == CourseState.BLOCKED)
                {
                    course.State = CourseState.PENDING;

                }


                await cnx.SaveChangesAsync();
                LessonId = episode.Id;


                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(course.Id);
                }
            }



            return LessonId;
        }



        public async Task UpdateEpisodeInCourse(string AuthToken, int CourseId, int EpisodeId, string Title, string Description, int OrdinalNumber)
        {
            int UserId = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Where(x => x.Id == UserId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes)
                    .FirstOrDefaultAsync();

                var Course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExistAuth(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExistAuth(Lesson, "Episode");
                if (!string.IsNullOrEmpty(Title))
                {
                    Lesson.Title = Title ?? "";
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    Lesson.Description = Description ?? "";
                }

                //if (OrdinalNumber != 0)
                //{
                Lesson.OrdinalNumber = OrdinalNumber;
                //}
                var previousCourseState = Course.State;
                if (Course.State == CourseState.APPROVED || Course.State == CourseState.BLOCKED)
                {
                    Course.State = CourseState.PENDING;

                }

                await cnx.SaveChangesAsync();



                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(Course.Id);
                }




            }
        }

        public async Task RemoveEpisodeFromCourse(string AuthToken, int CourseId, int EpisodeId)
        {
            int UserId = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Where(x => x.Id == UserId)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)
                    .FirstOrDefaultAsync();

                var Course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExistAuth(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExistAuth(Lesson, "Episode");




                if (Lesson.Attachments != null && Lesson.Attachments.Count > 0)
                {
                    List<EpisodeAttachment> attachmentsRemove = new List<EpisodeAttachment>();

                    foreach (var a in Lesson.Attachments)
                    {
                        attachmentsRemove.Add(a);
                    }

                    foreach (var LA in attachmentsRemove)
                    {
                        Lesson.Attachments.Remove(LA);
                    }
                }

                if (Lesson.IsPromo.HasValue && Lesson.IsPromo.Value)
                {
                    Course.HasPromo = false;
                }


                Course.Episodes.Remove(Lesson);
                await cnx.SaveChangesAsync();


            }

        }


        public async Task UpdateEpisodeMedia(string AuthToken, int CourseId, int EpisodeId)
        {
            int UserId = await GetUserIdForTokenAsync(AuthToken);

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                   .Where(x => x.Id == UserId)
                   .Include(x => x.OwnedCourses)
                   .ThenInclude(x => x.Episodes)
                   .FirstOrDefaultAsync();

                var Course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExistAuth(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExistAuth(Lesson, "Episode");
                var previousCourseState = Course.State;

                if (Course.State == CourseState.APPROVED || Course.State == CourseState.BLOCKED)
                {
                    Course.State = CourseState.PENDING;
                }

                await cnx.SaveChangesAsync();



                //Lesson.MediaId = MediaHashId;
                await cnx.SaveChangesAsync();

                if (previousCourseState == CourseState.APPROVED)
                {
                    Console.WriteLine("Reindexing by course");
                    await _searchSvc.ReindexByCourse(Course.Id);
                }
            }
        }

        public async Task UpdateMediaLenght(int MediaId, long Lenght)
        {
            using (var cnx = new DataContext())
            {
                var ep = await cnx.Episodes.Where(z => z.Id == MediaId).FirstOrDefaultAsync();
                if (ep != null)
                {
                    ep.MediaLenght = Lenght;
                }
            }
        }

        public async Task UpdateEpisodeAttachment(string AuthToken, int CourseId, int EpisodeId, string AttachmentHashId, bool needsConverting = false)
        {
            try
            {

                int UserId = await GetUserIdForTokenAsync(AuthToken);
                using (var cnx = new DataContext())
                {
                    var user = await cnx.users
                       .Where(x => x.Id == UserId)
                       .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.Episodes)
                       .FirstOrDefaultAsync();




                    var Course = user.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                    CheckExistAuth(Course, "Course");
                    var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                    CheckExistAuth(Lesson, "Episode");

                    Lesson.MediaId = AttachmentHashId;
                    Lesson.MediaNeedsConverting = needsConverting;
                    Lesson.EpisodeState = EpisodeState.UPLOADED;
                    var previousCourseState = Course.State;
                    if (Course.State == CourseState.APPROVED || Course.State == CourseState.BLOCKED)
                    {
                        Course.State = CourseState.PENDING;

                    }

                    await cnx.SaveChangesAsync();

                    //Lesson.Attachments = AttachmentHashId;
                    await cnx.SaveChangesAsync();


                    if (previousCourseState == CourseState.APPROVED)
                    {
                        Console.WriteLine("Reindexing by course");
                        await _searchSvc.ReindexByCourse(Course.Id);
                    }
                }
            }
            catch (CoachOnlineException e)
            {
                Console.WriteLine("Trying work like admin.");
                using (var cnx = new DataContext())
                {


                    var admin = await cnx.Admins.Where(x => x.AdminLogins.Any(p => p.AuthToken == AuthToken))
                      .FirstOrDefaultAsync();
                    //.Include(x => x.AdminLogins)
                    //var admin = await cnx.twoFATokens.Where(x => x.)
                    if (admin == null)
                    {

                        throw new CoachOnlineException("Auth Token never existed. Couldn't login as admin.,", CoachOnlineExceptionState.NotExist);

                    }
                    else
                    {
                        Console.WriteLine("Working as admin.");
                        var adminAsUser = cnx.users.Where(x => x.OwnedCourses.Any(p => p.Id == CourseId && p.Episodes.Any(o => o.Id == EpisodeId)))
                             .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.Episodes)
                            .FirstOrDefault();

                        if (adminAsUser == null)
                        {
                            throw new CoachOnlineException($"Unauthorized.", CoachOnlineExceptionState.NotExist);

                        }
                        else
                        {

                            var Course = adminAsUser.OwnedCourses.Where(x => x.Id == CourseId).FirstOrDefault();
                            CheckExistAuth(Course, "Course");
                            var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                            CheckExistAuth(Lesson, "Episode");

                            Lesson.MediaId = AttachmentHashId;
                            Lesson.MediaNeedsConverting = needsConverting;
                            var previousCourseState = Course.State;
                            if (Course.State != CourseState.UNPUBLISHED)
                            {
                                Course.State = CourseState.PENDING;
                            }



                            //Lesson.Attachments = AttachmentHashId;
                            await cnx.SaveChangesAsync();

                            if(previousCourseState == CourseState.APPROVED)
                            {
                                await _searchSvc.ReindexByCourse(CourseId);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                int UserId = await GetUserIdForTokenAsync(AuthToken);

            }

        }



        public async Task<int> CreateCategoryAsync(string name, string AuthToken)
        {
            int catId = 0;
            int UserId = await GetUserIdForTokenAsync(AuthToken);

            using (var cnx = new DataContext())
            {

                var categoryExisting = await cnx.courseCategories
                    .Where(x => x.Name == name)
                    .FirstOrDefaultAsync();
                if (categoryExisting != null)
                {
                    throw new CoachOnlineException("Category with this name already exist.", CoachOnlineExceptionState.AlreadyExist);
                }
                Category category = new Category { Name = name };
                cnx.courseCategories.Add(category);
                await cnx.SaveChangesAsync();
                catId = category.Id;



            }
            return catId;
        }





        //Authentication

        public void CreateCoachAccount(string id, string secret, string secretRepeated)
        {
            if (secret != secretRepeated)
            {
                throw new CoachOnlineException("Password must have more than 5 digits and contains one of '!@#$%^&*()', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }
            if (!PasswordSecure(secret))
            {
                throw new CoachOnlineException("Password must have more than 5 digits and contains one of '!@#$%^&*()', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }
            var correct = Helpers.Extensions.IsEmailCorrect(id);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.EmailAddress == id).FirstOrDefault();
                if (user != null)
                {
                    throw new CoachOnlineException("User already exist.", CoachOnlineExceptionState.AlreadyExist);
                }
                cnx.users.Add(new User
                {
                    EmailAddress = id,
                    Password = LetsHash.ToSHA512(secret),
                    Status = UserAccountStatus.AWAITING_EMAIL_CONFIRMATION,
                    UserRole = UserRoleType.COACH,
                    AccountCreationDate = DateTime.Now
                });
                cnx.SaveChanges();
            }



        }

        public async Task<dynamic> GetInfoAboutAffiliateHost(string affLink)
        {
            using(var ctx = new DataContext())
            {
                var affdata = await ctx.AffiliateLinks.Where(t => t.GeneratedToken == affLink).Include(u => u.User).FirstOrDefaultAsync();
                affdata.CheckExist("Token");

                affdata.User.CheckExist("User");

                return new { FirstName = affdata.User.FirstName, LastName = affdata.User.Surname, Email = affdata.User.EmailAddress };
            }
        }

        public async Task CreateStudentAccountAsync(string id, string secret, string secretRepeated, string firstName, string lastName, string phoneNo, string affiliateLink = null)
        {
            if (secret != secretRepeated)
            {
                throw new CoachOnlineException($"Password must have more than 5 digits and contains one of '{new string(Helpers.Extensions.PasswordMandatory)}', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }
            if (!Helpers.Extensions.IsPasswordSecure(secret))
            {
                throw new CoachOnlineException($"Password must have more than 5 digits and contains one of '{new string(Helpers.Extensions.PasswordMandatory)}', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }
            id = id.ToLower().Trim();
            var correct = Helpers.Extensions.IsEmailCorrect(id);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.FirstOrDefaultAsync(x => x.EmailAddress.ToLower().Trim() == id);
                if (user != null)
                {
                    throw new CoachOnlineException("User already exist.", CoachOnlineExceptionState.AlreadyExist);
                }

                var lastTerms = await cnx.Terms.OrderBy(x => x.Created).LastOrDefaultAsync();
                if (lastTerms == null)
                {
                    lastTerms = new Terms();
                }

                if (!string.IsNullOrEmpty(affiliateLink))
                {
                    var affiliate = await cnx.AffiliateLinks.Where(t => t.GeneratedToken == affiliateLink).FirstOrDefaultAsync();

                    if (affiliate == null)
                    {
                        throw new CoachOnlineException("Cannot register with from this affiliate link. It does not exist.", CoachOnlineExceptionState.NotExist);
                    }

                    string couponCode = null;
                    DateTime? couponExpiryDate = null;
                    if(affiliate.CouponCode != null)
                    {
                        var coupon = await cnx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == affiliate.CouponCode);

                        if(coupon != null)
                        {
                            couponCode = coupon.Id;
                            couponExpiryDate = DateTime.Now.AddMonths(1).AddDays(-1);
                        }
                    }

                    var modelType = AffiliateModelType.Regular;
                    var hostUser = await cnx.users.FirstOrDefaultAsync(x => x.Id == affiliate.UserId);
                    if(hostUser != null)
                    {
                        modelType = hostUser.AffiliatorType;
                    }

                    var newUser = new User
                    {
                        EmailAddress = id,
                        Password = LetsHash.ToSHA512(secret),
                        Status = UserAccountStatus.CONFIRMED,
                        UserRole = UserRoleType.STUDENT,
                        TermsAccepted = lastTerms,
                        AccountCreationDate = DateTime.Now,
                        FirstName = firstName,
                        Surname = lastName,
                        PhoneNo = phoneNo,
                        EmailConfirmed = false,
                        AffiliatorType = AffiliateModelType.Regular,
                        CouponId = couponCode,
                        CouponValidDate = couponExpiryDate,
                        AllowHiddenProducts = affiliate.WithTrialPlans
                    };

                    cnx.users.Add(newUser);
                    await cnx.SaveChangesAsync();

                    await _userSvc.GenerateNick(newUser.Id);

                    var newAffiliate = new Affiliate();
                    newAffiliate.CreationDate = DateTime.Now;
                    newAffiliate.HostUserId = affiliate.UserId;
                    newAffiliate.AffiliateUserId = newUser.Id;
                    newAffiliate.IsAffiliateACoach = false;
                    newAffiliate.AffiliateModelType = modelType;
                    cnx.Affiliates.Add(newAffiliate);
                    await cnx.SaveChangesAsync();

                }
                else {

                    var newUser = new User
                    {
                        EmailAddress = id,
                        Password = LetsHash.ToSHA512(secret),
                        Status = UserAccountStatus.CONFIRMED,
                        UserRole = UserRoleType.STUDENT,
                        FirstName = firstName,
                        Surname = lastName,
                        PhoneNo = phoneNo,
                        TermsAccepted = lastTerms,
                        AccountCreationDate = DateTime.Now,
                        EmailConfirmed = false,
                        AffiliatorType = AffiliateModelType.Regular
                        
                    };

                    cnx.users.Add(newUser);
                    await cnx.SaveChangesAsync();

                    await _userSvc.GenerateNick(newUser.Id);
                }
              
            }


        }


        public string GetAuthToken(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "")
        {
            string authToken = "";
            string secretHashed = LetsHash.ToSHA512(secret);

            id = id.ToLower().Trim();

            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.EmailAddress == id)
                    .Include(x => x.UserLogins)
                    .FirstOrDefault();

                if (user.Status == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION)
                {
                    throw new CoachOnlineException("Please confirm your email.", CoachOnlineExceptionState.PermissionDenied);
                }

                if (user.Status == UserAccountStatus.BANNED)
                {
                    throw new CoachOnlineException("You are banned. Can't access your account.", CoachOnlineExceptionState.UserIsBanned);
                }


                CheckExistAuth(user, "User");
                CompareHashes(secretHashed, user.Password, "Password");


                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    _subscriptionSvc.CreateUserStripeCustomerAccount(user).Wait();
                }


                authToken = LetsHash.RandomHash();
                if (user.UserLogins == null)
                {
                    user.UserLogins = new List<UserLogins>();
                }
                user.UserLogins.Add(new UserLogins
                {
                    AuthToken = authToken,
                    Created = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    DeviceInfo = deviceInfo,
                    IpAddress = IpAddress,
                    PlaceInfo = PlaceInfo,
                    Disposed = false,
                    ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30))
                });
                cnx.SaveChanges();

                _userSvc.Authenticate(id, secret);

            }


            return authToken;

        }
        public void ChangePassword(string login, string password, string oldPassword)
        {
            if (!PasswordSecure(password))
            {
                Log.Error("Insecure password");
                throw new CoachOnlineException("Password must have more than 5 digits and contains one of '!@#$%^&*()', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }

            login = login.ToLower().Trim();

            string hashedPassword = LetsHash.ToSHA512(password);
            string hashedOldPassword = LetsHash.ToSHA512(oldPassword);

            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.EmailAddress == login).FirstOrDefault();
                CheckExistAuth(user, "User");
                CompareHashes(user.Password, hashedOldPassword, "Old password");
                user.Password = hashedPassword;
                cnx.SaveChanges();
            }
        }


        public void DisposeLogin(string token)
        {
            using (var cnx = new DataContext())
            {
                var loginToDispose = cnx.userLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                CheckExistAuth(loginToDispose, "Login token");
                loginToDispose.Disposed = true;
                loginToDispose.ValidTo = 0;
                cnx.SaveChanges();
            }
        }


        public string ResetPassword(string login)
        {
            string resetPasswordHash = "";

            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.EmailAddress == login)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefault();

                if (user.TwoFATokens == null)
                {
                    user.TwoFATokens = new List<TwoFATokens>();
                }

                string resetHash = LetsHash.RandomHash(login);
                resetPasswordHash = resetHash;

                user.TwoFATokens.Add(new TwoFATokens
                {
                    Deactivated = false,
                    Token = resetHash,
                    Type = TwoFaTokensTypes.RESET_PASSWORD,
                    ValidateTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddHours(2))
                });


                cnx.SaveChanges();
            }

            return resetPasswordHash;
        }

        public void ResetPasswordConfirmation(string login, string password, string passwordRepeated, string resetToken)
        {
            if (password != passwordRepeated)
            {
                throw new CoachOnlineException("Passwords not match.", CoachOnlineExceptionState.PasswordsNotMatch);
            }
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.EmailAddress == login)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefault();

                CheckExistAuth(user, "Users");
                CheckExistAuth(user.TwoFATokens, "Reset password attemptions");

                var reset = user.TwoFATokens.Where(x => x.ValidateTo > ConvertTime.ToUnixTimestampLong(DateTime.Now) && !x.Deactivated)
                    .FirstOrDefault();
                CheckExistAuth(reset, "Reset password attemption");
                user.Password = LetsHash.ToSHA512(password);
                reset.Deactivated = true;
                cnx.SaveChanges();


            }
        }


        public async Task<string> CreateEmailConfirmationToken(string email)
        {
            string token = LetsHash.RandomHash("email");

            email = email.ToLower().Trim();

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == email)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User with this email address");

                if(user.EmailConfirmed)
                {
                    throw new CoachOnlineException("Email is alredy confirmed", CoachOnlineExceptionState.DataNotValid);
                }
                if (user.Status == UserAccountStatus.DELETED)
                {
                    throw new CoachOnlineException("Operation not allowed", CoachOnlineExceptionState.DataNotValid);
                }
                if (user.TwoFATokens.Any(x => x.Type == TwoFaTokensTypes.EMAIL_CONFIRMATION && !x.Deactivated))
                {
                    foreach (var t in user.TwoFATokens)
                    {
                        t.Deactivated = true;
                    }
                }

                if (user.TwoFATokens == null)
                {
                    user.TwoFATokens = new List<TwoFATokens>();
                }

                user.TwoFATokens.Add(new TwoFATokens
                {
                    Deactivated = false,
                    Token = token,
                    Type = TwoFaTokensTypes.EMAIL_CONFIRMATION,
                    ValidateTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(10))
                });

                await cnx.SaveChangesAsync();


            }


            return token;
        }


        public async Task ValidateEmailToken(string token)
        {
            using (var cnx = new DataContext())
            {

                var user = await cnx.users.Where(x => x.TwoFATokens.Any(x => x.Token == token))
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();
                CheckExistAuth(user, "Email token");

                var userToken = user.TwoFATokens.Where(x => x.Token == token).FirstOrDefault();
                CheckExistAuth(user, "Email token");

                if (userToken.Type != TwoFaTokensTypes.EMAIL_CONFIRMATION)
                {

                    throw new CoachOnlineException("Token type unsupported here.", CoachOnlineExceptionState.DataNotValid);
                }


                if (userToken.Deactivated)
                {
                    throw new CoachOnlineException("Token not valid.", CoachOnlineExceptionState.DataNotValid);
                }

                if (ConvertTime.ToUnixTimestampLong(DateTime.Now) >= userToken.ValidateTo)
                {
                    throw new CoachOnlineException($"Token expired at {ConvertTime.FromUnixTimestamp(userToken.ValidateTo)}.", CoachOnlineExceptionState.Expired);
                }

                userToken.Deactivated = true;

                user.Status = UserAccountStatus.CONFIRMED;
                await cnx.SaveChangesAsync();

            }
        }

        //Extensions
        //private void CheckEmailCorrect(string email)
        //{
        //    try
        //    {
        //        var address = new System.Net.Mail.MailAddress(email);
        //    }
        //    catch
        //    {
        //        throw new CoachOnlineException("Provided email address has an incorrect format.", CoachOnlineExceptionState.IncorrectFormat);
        //    }
        //}
        private void CheckExistAuth(object obj, string FieldName)
        {
            if (obj == null)
            {
                Log.Error($"{FieldName} does not exist.");
                throw new CoachOnlineException($"{FieldName} does not exist.", CoachOnlineExceptionState.NotExist);
            }
        }

        private void CompareHashes(string first, string second, string HashFieldName)
        {
            if (first != second)
            {
                throw new CoachOnlineException($"Wrong {HashFieldName} provided.", CoachOnlineExceptionState.WrongPassword);
            }
        }
        private char[] PasswordMandatory = { '$', '!', '@', '%', '^', '&', '*', '(', ')' };

        private bool PasswordSecure(string password)
        {
            if (!password.Any(char.IsLower)
                    || !password.Any(char.IsDigit)
                    || !password.Any(char.IsUpper)
                    || password.Length < 5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private int GetUserIdForToken(string token)
        {
            int id = 0;

            using (var cnx = new DataContext())
            {
                var user = cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefault();
                if (user == null)
                {
                    throw new CoachOnlineException("Auth Token never existed. Or not match with any user.", CoachOnlineExceptionState.NotExist);
                }
                var interestingLogin = user.UserLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                if (interestingLogin == null)
                {
                    throw new CoachOnlineException("AuthToken never existed.", CoachOnlineExceptionState.NotExist);
                }
                if (interestingLogin.Disposed)
                {
                    throw new CoachOnlineException("AuthToken is Disposed", CoachOnlineExceptionState.Expired);
                }
                if (interestingLogin.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    interestingLogin.Disposed = true;
                    cnx.SaveChanges();
                    throw new CoachOnlineException("AuthToken is Outdated", CoachOnlineExceptionState.Expired);

                }
                id = user.Id;
            }

            return id;
        }

        public async Task<int> GetUserIdForTokenAsync(string token)
        {
            int id = 0;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new CoachOnlineException("Auth Token never existed. Or not match with any user.", CoachOnlineExceptionState.NotExist);
                }
                var interestingLogin = user.UserLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                if (interestingLogin == null)
                {
                    throw new CoachOnlineException("AuthToken never existed.", CoachOnlineExceptionState.NotExist);
                }
                if (interestingLogin.Disposed)
                {
                    throw new CoachOnlineException("AuthToken is Disposed", CoachOnlineExceptionState.Expired);
                }
                if (interestingLogin.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    interestingLogin.Disposed = true;
                    await cnx.SaveChangesAsync();
                    throw new CoachOnlineException("AuthToken is Outdated", CoachOnlineExceptionState.Expired);

                }
                id = user.Id;
            }

            return id;
        }

        //Async
        public async Task<string> GetAuthTokenAsync(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "")
        {
            string authToken = "";
            string secretHashed = LetsHash.ToSHA512(secret);

            id = id.ToLower().Trim();

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == id && x.Status != UserAccountStatus.DELETED)
                    .Include(x => x.UserLogins)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User");
                CompareHashes(secretHashed, user.Password, "Password");


                if (user.Status == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION)
                {
                    throw new CoachOnlineException("Please confirm your email.", CoachOnlineExceptionState.PermissionDenied);
                }

                if (user.Status == UserAccountStatus.BANNED)
                {
                    throw new CoachOnlineException("You are banned. Can't access your account.", CoachOnlineExceptionState.UserIsBanned);
                }

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    await _subscriptionSvc.CreateUserStripeCustomerAccount(user);
                }


                authToken = LetsHash.RandomHash(user.Password);
                if (user.UserLogins == null)
                {
                    user.UserLogins = new List<UserLogins>();
                }
                user.UserLogins.Add(new UserLogins
                {
                    AuthToken = authToken,
                    Created = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    DeviceInfo = deviceInfo,
                    IpAddress = IpAddress,
                    PlaceInfo = PlaceInfo,
                    Disposed = false,
                    ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30))
                });
                await cnx.SaveChangesAsync();

            }


            return authToken;
        }


        public async Task<GetAuthTokenResponse> GetAuthTokenWithUserDataAsync(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "")
        {
            string authToken = "";
            GetAuthTokenResponse response = new GetAuthTokenResponse();
            response.UserInfo = new UserAuthInfo();
            string secretHashed = LetsHash.ToSHA512(secret);

            id = id.ToLower().Trim();

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == id && x.Status != UserAccountStatus.DELETED)
                    .Include(x => x.UserLogins)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User");
                CompareHashes(secretHashed, user.Password, "Password");


                //if (user.Status == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION)
                //{
                //    throw new CoachOnlineException("Account not confirmed", CoachOnlineExceptionState.PermissionDenied);
                //}
                if (user.Status == UserAccountStatus.BANNED)
                {
                    throw new CoachOnlineException("Account is banned", CoachOnlineExceptionState.PermissionDenied);
                }

                authToken = LetsHash.RandomHash(user.Password);
                response.AuthToken = authToken;
                response.UserInfo.Email = user.EmailAddress ?? "";
                response.UserInfo.Name = user.FirstName ?? "";
                response.UserInfo.UserRole = user.UserRole.ToString();
                response.UserInfo.SubscriptionActive = user.SubscriptionActive;



                if (user.WithdrawalsEnabled)
                {
                    response.UserInfo.StripeVerificationStatus = 3;
                }
                else if (user.PaymentsEnabled)
                {
                    response.UserInfo.StripeVerificationStatus = 2;

                }
                else if (!string.IsNullOrEmpty(user.StripeAccountId))
                {
                    response.UserInfo.StripeVerificationStatus = 1;

                }
                else
                {
                    response.UserInfo.StripeVerificationStatus = 0;

                }



                response.UserInfo.StripeCustomerId = user.StripeCustomerId;
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    await _subscriptionSvc.CreateUserStripeCustomerAccount(user);

                    response.UserInfo.StripeCustomerId = (await _userSvc.GetUserById(user.Id)).StripeCustomerId;
                }




                if (user.UserLogins == null)
                {
                    user.UserLogins = new List<UserLogins>();
                }
                user.UserLogins.Add(new UserLogins
                {
                    AuthToken = authToken,
                    Created = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    DeviceInfo = deviceInfo,
                    IpAddress = IpAddress,
                    PlaceInfo = PlaceInfo,
                    Disposed = false,
                    ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30))
                });
                await cnx.SaveChangesAsync();

            }


            await _userSvc.Authenticate(authToken);

            return response;
        }

        public async Task DisposeLoginAsync(string token)
        {
            using (var cnx = new DataContext())
            {
                var loginToDispose = await cnx.userLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefaultAsync();
                CheckExistAuth(loginToDispose, "Login token");
                loginToDispose.Disposed = true;
                loginToDispose.ValidTo = 0;
                await cnx.SaveChangesAsync();
            }
        }

        public async Task ChangePasswordAsync(string Authtoken, string password, string passwordRepeated, string oldPassword)
        {
            string hashedPassword = LetsHash.ToSHA512(password);
            string hashedOldPassword = LetsHash.ToSHA512(oldPassword);

            var u = GetUserIdForToken(Authtoken);


            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == u).FirstOrDefaultAsync();
                CheckExistAuth(user, "User");

                if (user.SocialLogin.HasValue && user.SocialLogin.Value)
                {
                    throw new CoachOnlineException("Cannot update user password because the user is authenticated by external social account.", CoachOnlineExceptionState.CantChange);
                }

                if (!PasswordSecure(password))
                {
                    Log.Error("Insecure password");
                    throw new CoachOnlineException("Password must have more than 5 digits and contains one of '!@#$%^&*()', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
                }

                if (password != passwordRepeated)
                {
                    throw new CoachOnlineException("Password has to be the same as repeated.", CoachOnlineExceptionState.DataNotValid);

                }

                CompareHashes(user.Password, hashedOldPassword, "Old password");
                user.Password = hashedPassword;
                await cnx.SaveChangesAsync();
            }
        }

        public async Task<string> ResetPasswordAsync(string login)
        {
            string resetPasswordHash = "";

            login = login.ToLower().Trim();

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == login)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "User");

                if (user.SocialLogin.HasValue && user.SocialLogin.Value)
                {
                    throw new CoachOnlineException("Cannot update user password because the user is authenticated by external social account.", CoachOnlineExceptionState.CantChange);
                }

                if (user.TwoFATokens == null)
                {
                    user.TwoFATokens = new List<TwoFATokens>();
                }

                string resetHash = LetsHash.RandomHash(login);
                resetPasswordHash = resetHash;

                user.TwoFATokens.Add(new TwoFATokens
                {
                    Deactivated = false,
                    Token = resetHash,
                    Type = TwoFaTokensTypes.RESET_PASSWORD,
                    ValidateTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddHours(2))
                });


                await cnx.SaveChangesAsync();
            }

            return resetPasswordHash;
        }

        public async Task ResetPasswordConfirmationAsync(string login, string password, string passwordRepeated, string resetToken)
        {
            if (password != passwordRepeated)
            {
                throw new CoachOnlineException("Passwords not match.", CoachOnlineExceptionState.PasswordsNotMatch);
            }

            login = login.ToLower().Trim();
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == login)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();

                CheckExistAuth(user, "Users");
                CheckExistAuth(user.TwoFATokens, "Reset password attemptions");

                var reset = user.TwoFATokens.Where(x => x.ValidateTo > ConvertTime.ToUnixTimestampLong(DateTime.Now) && !x.Deactivated)
                    .FirstOrDefault();
                CheckExistAuth(reset, "Reset password attemption");
                user.Password = LetsHash.ToSHA512(password);
                reset.Deactivated = true;
                await cnx.SaveChangesAsync();
            }


        }

        public async Task CreateCoachAccountAsync(string id, string secret, string secretRepeated, string firstName, string lastName, string phoneNo, string affiliateLink = null)
        {
            if (secret != secretRepeated)
            {
                throw new CoachOnlineException("Passwords not match.", CoachOnlineExceptionState.PasswordsNotMatch);
            }
            if (!PasswordSecure(secret))
            {
                throw new CoachOnlineException("Password must have more than 5 digits and contains one of '!@#$%^&*()', number, low letter, and big letter.", CoachOnlineExceptionState.WeakPassword);
            }
            id = id.ToLower().Trim();
            var correct = Helpers.Extensions.IsEmailCorrect(id);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == id).FirstOrDefaultAsync();
                if (user != null)
                {
                    throw new CoachOnlineException("User already exist.", CoachOnlineExceptionState.AlreadyExist);
                }

                var lastTerms = cnx.Terms.OrderBy(x => x.Created).LastOrDefault();
                if (lastTerms == null)
                {
                    lastTerms = new Terms();
                }


                if (!string.IsNullOrEmpty(affiliateLink))
                {
                    var affiliate = await cnx.AffiliateLinks.Where(t => t.GeneratedToken == affiliateLink && t.ForCoach).FirstOrDefaultAsync();

                    if (affiliate == null)
                    {
                        throw new CoachOnlineException("Cannot register with from this affiliate link. It does not exist.", CoachOnlineExceptionState.NotExist);
                    }

                    var newUser = new User
                    {
                        EmailAddress = id,
                        Password = LetsHash.ToSHA512(secret),
                        Status = UserAccountStatus.CONFIRMED,
                        UserRole = UserRoleType.COACH,
                        TermsAccepted = lastTerms,
                        AccountCreationDate = DateTime.Now,
                        FirstName = firstName,
                        Surname = lastName,
                        PhoneNo = phoneNo,
                        EmailConfirmed = false,
                        AffiliatorType = AffiliateModelType.Regular
                    };

                    cnx.users.Add(newUser);
                    await cnx.SaveChangesAsync();

                    await _userSvc.GenerateNick(newUser.Id);

                    var newAffiliate = new Affiliate();
                    newAffiliate.CreationDate = DateTime.Now;
                    newAffiliate.HostUserId = affiliate.UserId;
                    newAffiliate.AffiliateUserId = newUser.Id;
                    newAffiliate.IsAffiliateACoach = true;
                    newAffiliate.AffiliateModelType = AffiliateModelType.Regular;
                    cnx.Affiliates.Add(newAffiliate);
                    await cnx.SaveChangesAsync();

                }
                else
                {
                    var newUser = new User
                    {
                        EmailAddress = id,
                        Password = LetsHash.ToSHA512(secret),
                        TermsAccepted = lastTerms,
                        UserRole = UserRoleType.COACH,
                        FirstName = firstName,
                        Surname = lastName,
                        PhoneNo = phoneNo,
                        Status = UserAccountStatus.CONFIRMED,
                        EmailConfirmed = false,
                        AccountCreationDate = DateTime.Now,
                        AffiliatorType = AffiliateModelType.Regular
                    };
                    cnx.users.Add(newUser);
                    await cnx.SaveChangesAsync();


                    await _userSvc.GenerateNick(newUser.Id);
                }


            }
        }

        public async Task ConfirmEmailRegistrationAsync(string emailToken)
        {
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.TwoFATokens.Any(x => x.Token == emailToken))
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();
                CheckExistAuth(user, "User");
                var token = user.TwoFATokens.Where(x => x.Token == emailToken).FirstOrDefault();
                CheckExistAuth(token, "Token");
                if (token.ValidateTo <= ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    throw new CoachOnlineException("Email token disposed.", CoachOnlineExceptionState.DataNotValid);
                }
                token.Deactivated = true;
                user.EmailConfirmed = true;
                await cnx.SaveChangesAsync();
            }
        }


        public async Task<string> ResendEmailConfirmation(string emailAddress)
        {
            string tokenAuth = LetsHash.RandomHash("email");
            emailAddress = emailAddress.ToLower().Trim();
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.EmailAddress.ToLower().Trim() == emailAddress)
                    .Include(x => x.TwoFATokens)
                    .FirstOrDefaultAsync();
                CheckExistAuth(user, "User with provided email");
                if (user.EmailConfirmed)
                {
                    throw new CoachOnlineException("User already confirmed.", CoachOnlineExceptionState.DataNotValid);
                }
                if (user.TwoFATokens == null)
                {
                    user.TwoFATokens = new List<TwoFATokens>();
                }

                var emailConfrimationsList = user.TwoFATokens.Where(x => x.Type == TwoFaTokensTypes.EMAIL_CONFIRMATION).ToList();
                if (emailConfrimationsList.Any(x => x.ValidateTo >= ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(10).AddMinutes(10))))
                {
                    throw new CoachOnlineException("Vous pouvez recevoir un courriel toutes les 10 minutes", CoachOnlineExceptionState.DataNotValid);
                }
                var token = user.TwoFATokens.Where(x => x.Type == TwoFaTokensTypes.EMAIL_CONFIRMATION).ToList();
                foreach (var t in token)
                {
                    t.Deactivated = true;
                }

                user.TwoFATokens.Add(new TwoFATokens
                {
                    Deactivated = false,
                    Token = tokenAuth,
                    Type = TwoFaTokensTypes.EMAIL_CONFIRMATION,
                    ValidateTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(10))
                });
                await cnx.SaveChangesAsync();
            }

            return tokenAuth;

        }

        public static bool ValidateBankAccount(string bankAccount)
        {
            if (string.IsNullOrEmpty(bankAccount))
                return false;
            if (bankAccount.Replace(" ", "").Length < 10)
                return false;
            if (bankAccount.Replace(" ", "").Length <= 41)
                return true;

            bankAccount = bankAccount.ToUpper(); //IN ORDER TO COPE WITH THE REGEX BELOW
       
            if (System.Text.RegularExpressions.Regex.IsMatch(bankAccount, "^[A-Z0-9]"))
            {
                bankAccount = bankAccount.Replace(" ", String.Empty);
                string bank =
                bankAccount.Substring(4, bankAccount.Length - 4) + bankAccount.Substring(0, 4);
                int asciiShift = 55;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString());
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
                return false;
        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
}

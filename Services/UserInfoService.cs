using CoachOnline.ElasticSearch.Models;
using CoachOnline.Statics;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.ApiObject;
using CoachOnline.Model.ApiResponses.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class UserInfoService : IUserInfo
    {
        public async Task<CoachInfoResponse> GetCoachData(int coachId, bool forCourse = false, bool forAdmin = false)
        {
            using (var ctx = new DataContext())
            {
                User c;
                if (forAdmin)
                {
                    c = await ctx.users.Where(t => t.Id == coachId).FirstOrDefaultAsync();
                }
                else
                {
                    c = await ctx.users.Where(t => t.Id == coachId && t.UserRole == Model.UserRoleType.COACH && t.Status == UserAccountStatus.CONFIRMED).FirstOrDefaultAsync();
                }


                if (c != null)
                {
                    CoachInfoResponse idx = new CoachInfoResponse();
                    idx.Bio = c.Bio;
                    idx.Country = c.Country;
                    idx.Email = c.EmailAddress;
                    idx.FirstName = c.FirstName;
                    idx.LastName = c.Surname;
                    idx.PhotoUrl = c.AvatarUrl;
                    idx.Gender = c.Gender;
                    idx.Id = c.Id;
                    idx.UserCategories = await GetCoachCategories(c.Id);
                    if (!forCourse)
                    {
                        idx.Courses = await GetCoachCourses(c.Id);
                    }
                    return idx;
                }
                else
                {
                    throw new CoachOnlineException("Coach does not exist", CoachOnlineExceptionState.NotExist);
                }

            }
        }

        public async Task<CoachInfoDocumentResponse> GetCoachDocumentData(int coachId)
        {
            CoachInfoDocumentResponse idx = new CoachInfoDocumentResponse();
            
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == coachId )
                    .Include(x => x.UserCV)
                    .Include(x => x.UserAttestations)
                    .Include(x => x.UserReturns)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();


                if (user != null)
                {
                            
                    if (user.UserCV != null) {
                        idx.UserCV = user.UserCV.DocumentUrl;
                    }
                    if (user.UserReturns != null) {
                        idx.Returns= new List<string>();
                        foreach (var c in user.UserReturns)
                        {
                            idx.Returns.Add(c.DocumentUrl);
                        }
                    }
                    if (user.UserAttestations != null) {
                        idx.Diplomas= new List<string>();
                        foreach (var c in user.UserAttestations)
                        {
                            idx.Diplomas.Add(c.DocumentUrl);
                        }
                    }
                }
                else
                {
                    throw new CoachOnlineException("Coach does not exist", CoachOnlineExceptionState.NotExist);
                }

            }
            // switch (coachId)
            // {
            //     case 6:
            //         idx.UserCV = "documents/Olga-Ciesco.pdf";
            //         idx.ReturnOne = "documents/avis-clients-olga-1.png";
            //         idx.ReturnTwo = "documents/avis-clients-olga-2.png";
            //         idx.ReturnThree = "documents/avis-clients-olga-3.png";
            //         idx.DiplomeOne = "documents/diplome-olga-ciesco-1.jpeg";
            //         idx.DiplomeTwo = "documents/diplome-olga-ciesco-2.jpeg";
            //         idx.DiplomeThree = "documents/diplome-olga-ciesco-3.jpeg";
            //         break;

            //     case 23:
            //         idx.UserCV = "documents/Olga-Ciesco.pdf";
            //         idx.ReturnOne = "documents/avis-clients-olga-1.png";
            //         idx.ReturnTwo = "documents/avis-clients-olga-2.png";
            //         idx.ReturnThree = "documents/avis-clients-olga-3.png";
            //         idx.DiplomeOne = "documents/diplome-olga-ciesco-1.jpeg";
            //         idx.DiplomeTwo = "documents/diplome-olga-ciesco-2.jpeg";
            //         idx.DiplomeThree = "documents/diplome-olga-ciesco-3.jpeg";
            //         break;

            //     case 602:
            //         idx.UserCV = "documents/elise-triquet.pdf";
            //         idx.ReturnOne = "documents/avis-elise-triquet-1.png";
            //         idx.ReturnTwo = "documents/avis-elise-triquet-2.png";
            //         idx.ReturnThree = "";
            //         idx.DiplomeOne = "documents/diplome-elise-triquet-1.pdf";
            //         idx.DiplomeTwo = "";
            //         idx.DiplomeThree = "";
            //         break;
                
            //     case 834:
            //         idx.UserCV = "documents/Georges-Homsi.pdf";
            //         idx.ReturnOne = "documents/avis-georges-homsi.png";
            //         idx.ReturnTwo = "";
            //         idx.ReturnThree = "";
            //         idx.DiplomeOne = "";
            //         idx.DiplomeTwo = "";
            //         idx.DiplomeThree = "";
            //         break;

            //     case 931:
            //         idx.UserCV = "documents/Franck-Tabesse.pdf";
            //         idx.ReturnOne = "";
            //         idx.ReturnTwo = "";
            //         idx.ReturnThree = "";
            //         idx.DiplomeOne = "";
            //         idx.DiplomeTwo = "";
            //         idx.DiplomeThree = "";
            //         break;

            //      default:
            //         idx.UserCV = "";
            //         idx.ReturnOne = "";
            //         idx.ReturnTwo = "";
            //         idx.ReturnThree = "";
            //         idx.DiplomeOne = "";
            //         idx.DiplomeTwo = "";
            //         idx.DiplomeThree = "";
            //         break;
            // }

            return idx;

        }

        public async Task<CourseInfoResponse> GetCourseInfoData(int courseId)
        {
            CourseInfoResponse idx = new CourseInfoResponse();

            using (var ctx = new DataContext())
            {
                var course = await ctx.courses.Where(t => (t.Id == courseId && t.State == CourseState.APPROVED)).FirstOrDefaultAsync();
                if (course != null)
                {
                    idx.Cible = course.PublicTargets;
                    idx.Requis = course.Prerequisite;
                    idx.Objectif = course.Objectives;
                    idx.CertificationQCM = course.CertificationQCM;
                }
            }

            // switch (courseId)
            // {

            //     case 3:
            //         idx.Cible = "Tout public";
            //         idx.Requis = "Aucun ";
            //         idx.Objectif = "Acquérir la connaissance des gestes permettant d'intervenir en cas de malaise, accident, étouffement ou arrêt cardiaque d’une personne. Vous saurez mettre en œuvre les gestes à mettre en œuvre en attendant l'arrivée des secours.";
            //         idx.CertificationQCM = "https://docs.google.com/forms/d/11Ug8QgYZKiAg9oauxYf9Z7BBTQPPYrPcgnsUXiIfr-I/prefill";
            //         break;

            //     case 492:
            //         idx.Cible = "Tout public";
            //         idx.Requis = "Aucun ";
            //         idx.Objectif = "Acquérir la connaissance des gestes permettant d'intervenir en cas de malaise, accident, étouffement ou arrêt cardiaque d’une personne. Vous saurez mettre en œuvre les gestes à mettre en œuvre en attendant l'arrivée des secours.";
            //         idx.CertificationQCM = "https://docs.google.com/forms/d/11Ug8QgYZKiAg9oauxYf9Z7BBTQPPYrPcgnsUXiIfr-I/prefill";
            //         break;

            //      default:
            //         idx.Cible = "";
            //         idx.Requis = "";
            //         idx.Objectif = "";
            //         idx.CertificationQCM = "";
            //         break;
            // }

            return idx;

        }


        public async Task<CourseResponse> GetCourse(int courseId, int userId)
        {
            List<CourseResponse> data = new List<CourseResponse>();
            using (var ctx = new DataContext())
            {
                var c = await ctx.courses.Where(t => (t.Id == courseId && t.State == CourseState.APPROVED) || (t.Id == courseId && t.UserId == userId)).Include(r=>r.RejectionsHistory).FirstOrDefaultAsync();
                if (c != null)
                {
                    CourseResponse courseResponse = new CourseResponse();
                    courseResponse.Category = await GetCourseCategory(c.CategoryId);
                    courseResponse.Created = c.Created;
                    courseResponse.Description = c.Description;
                    courseResponse.Id = c.Id;
                    courseResponse.Name = c.Name ?? "";
                    courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                    courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                    courseResponse.State = c.State;
                    courseResponse.Episodes = await GetCourseEpisodes(c.Id);
                    courseResponse.Coach = await GetCoachData(c.UserId, true);
                    
                    courseResponse.RejectionsHistory = new List<RejectionResponse>();
                    if (c.RejectionsHistory != null)
                    {
                        foreach (var r in c.RejectionsHistory)
                        {
                            var rr = new RejectionResponse();
                            rr.Id = r.Id;
                            rr.Reason = r.Reason;
                            rr.Date = r.Date;
                            courseResponse.RejectionsHistory.Add(rr);
                        }
                    }
                    return courseResponse;

                }

                return null;
            }

        }

        public async Task<List<CourseResponse>> GetCoachCourses(int coachId)
        {
            List<CourseResponse> data = new List<CourseResponse>();
            using (var ctx = new DataContext())
            {
                var courses = await ctx.courses.Where(t => t.UserId == coachId && t.State == CourseState.APPROVED).ToListAsync();
                foreach (var c in courses)
                {
                    CourseResponse courseResponse = new CourseResponse();
                    courseResponse.Category = await GetCourseCategory(c.CategoryId);
                    courseResponse.Created = c.Created;
                    courseResponse.Description = c.Description;
                    courseResponse.Id = c.Id;
                    courseResponse.Name = c.Name ?? "";
                    courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                    courseResponse.State = c.State;
                    courseResponse.Episodes = await GetCourseEpisodes(c.Id);
                    courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";


                    data.Add(courseResponse);
                }
            }


            return data;
        }

        public async Task<CategoryAPI> GetCourseCategory(int categoryId)
        {
            using (var ctx = new DataContext())
            {
                var category = await ctx.courseCategories.Where(t => t.Id == categoryId).Include(p => p.Parent).FirstOrDefaultAsync();
                CategoryAPI resp = new CategoryAPI();
                if (category != null)
                {
                    resp.Name = category.Name;
                    resp.Id = category.Id;
                    resp.AdultOnly = category.AdultOnly;

                    if (category.Parent != null)
                    {
                        resp.ParentId = category.Parent.Id;
                        resp.ParentName = category.Parent.Name;
                    }
                }

                return resp;
            }
        }

        public async Task<List<EpisodeAttachment>> GetEpisodeAttachments(int episodeId)
        {
            var attachments = new List<EpisodeAttachment>();
            using (var ctx = new DataContext())
            {
                var att = await ctx.Episodes.Where(t => t.Id == episodeId).Include(a => a.Attachments).FirstOrDefaultAsync();
                if (att != null && att.Attachments != null && att.Attachments.Count > 0)
                {
                    foreach (var a in att.Attachments)
                    {
                        var x = new EpisodeAttachment();
                        x.Extension = a.Extension;
                        x.Hash = a.Hash;
                        x.Id = a.Id;
                        x.Name = a.Name;
                        x.Added = a.Added;
                        x.QueryString = $"attachments/{a.Hash}.{a.Extension}";

                        attachments.Add(x);
                    }
                }
            }
            return attachments;
        }

        public async Task<List<EpisodeResponse>> GetCourseEpisodes(int courseId)
        {
            using (var ctx = new DataContext())
            {
                List<EpisodeResponse> returnData = new List<EpisodeResponse>();
                var episodes = await ctx.Episodes.Where(t => t.CourseId == courseId).ToListAsync();
                foreach (var e in episodes)
                {
                    EpisodeResponse episodeResponse = new EpisodeResponse();

                    episodeResponse.Created = e.Created;
                    episodeResponse.Description = e.Description ?? "";
                    episodeResponse.Id = e.Id;
                    episodeResponse.MediaId = e.MediaId;
                    episodeResponse.OrdinalNumber = e.OrdinalNumber;
                    episodeResponse.Title = e.Title ?? "";
                    episodeResponse.CourseId = e.CourseId;
                    episodeResponse.Length = e.MediaLenght;
                    episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                    episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                    episodeResponse.EpisodeState = e.EpisodeState;

                    if (episodeResponse.MediaId != null)
                    {

                        if (episodeResponse.MediaId.ToLower().Contains(".mp4"))
                        {
                            episodeResponse.NeedConversion = e.MediaNeedsConverting;
                        }
                        else
                        {
                            episodeResponse.NeedConversion = false;
                        }
                    }


                    returnData.Add(episodeResponse);
                }

                return returnData;
            }
        }

        public async Task<List<CoachCategories>> GetCoachCategories(int coachId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == coachId).Include(c => c.AccountCategories).FirstOrDefaultAsync();
                var categories = user.AccountCategories;
                List<CoachCategories> cats = new List<CoachCategories>();
                if (categories != null)
                {
                    categories.ForEach(el =>
                    {
                        cats.Add(new CoachCategories { Id = el.Id, Name = el.Name });
                    });
                }

                return cats;
            }
        }


    }
}

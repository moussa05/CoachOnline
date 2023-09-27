using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.CommentsService;

namespace CoachOnline.Services
{

    public class CommentsService:IComment
    {
        private readonly ILogger<CommentsService> _logger;
        private readonly IUser _userSvc;
        private readonly IEmailApiService _emailSvc;
        public CommentsService(ILogger<CommentsService> logger, IUser userSvc, IEmailApiService emailSvc)
        {
            _logger = logger;
            _userSvc = userSvc;
            _emailSvc = emailSvc;
        }

        private async Task SendEmailToCourseOwnerAboutNewComment(Course c)
        {
            if(c!=null)
            {
                var user = await _userSvc.GetUserByIdAllowNull(c.UserId);
                if(user!= null)
                {
                    //prepare email message
                    string body = "";
                    if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/NewCourseComment.html"))
                    {
                        body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/NewCourseComment.html");
                        body = body.Replace("###PLATFORM_URL###", $"{Statics.ConfigData.Config.WebUrl}/course?id={c.Id}");
                        body = body.Replace("###COURSE_NAME###", c.Name);

                        Console.WriteLine("body changed");
                    }

                    if (body != "")
                    {
                        await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage
                        {
                            AuthorEmail = "info@coachs-online.com",
                            AuthorName = "Coachs-online",
                            Body = body,
                            ReceiverEmail = user.EmailAddress,
                            ReceiverName = $"{user.FirstName} {user.Surname}",
                            Topic = "Coachs-online - nouveau commentaire dans votre cours"
                        });
                    }
                }
            }
        }

        public async Task<List<CourseCommentResponse>> GetCourseComments(int courseId, int? userId, bool isAdmin)
        {
            using(var ctx = new DataContext())
            {
                var comments = await ctx.CourseComments.Where(x => x.CourseId == courseId).ToListAsync();

                var data = new List<CourseCommentResponse>();
                var parentLevel = comments.Where(x => x.CommentLevel == 0).OrderByDescending(x=>x.CreationDate).ThenByDescending(x=>x.LastUpdateDate).ToList();

                foreach(var itm in parentLevel)
                {
                    var usr = await _userSvc.GetUserByIdAllowNull(itm.UserId);
                    var parentComment = new CourseCommentResponse();
                    parentComment.CommentText = itm.Comment;
                    parentComment.Children = new List<CourseCommentResponse>();
                    parentComment.CommentId = itm.Id;
                    parentComment.CourseId = courseId;
                    parentComment.CreationDate = itm.CreationDate;
                    parentComment.Email = usr!= null? usr.EmailAddress : "Account deleted";
                    parentComment.FullName = usr != null ? $"{usr.FirstName?.ToString()} {usr.Surname?.ToString()}" : "Account deleted";
                    parentComment.IsCourseAuthorComment = itm.IsCommentatorAuthorOfCourse;
                    parentComment.IsDeleted = itm.IsDeleted;
                    parentComment.IsEdited = itm.IsEdited;
                    parentComment.IsMyComment = isAdmin || !userId.HasValue?false: userId.Value == itm.UserId;
                    parentComment.LastUpdateDate = itm.LastUpdateDate.HasValue?itm.LastUpdateDate.Value : itm.CreationDate;
                    parentComment.UserId = itm.UserId;
                    parentComment.CoachPhotoUrl = usr != null ? usr.AvatarUrl:"";
                    parentComment.Nick = usr != null ? usr.Nick: "Account deleted";
                    var children = comments.Where(x=>x.ParentCommentId.HasValue && x.ParentCommentId == itm.Id).OrderBy(x => x.CreationDate).ThenBy(x => x.LastUpdateDate).ToList();

                    if(children.Any())
                    {
                        var childrenMain = children.Where(x =>!x.ReplyCommentId.HasValue || x.ReplyCommentId.Value == x.ParentCommentId).ToList();
                       
                        foreach(var itmChild in childrenMain)
                        {
                            var childUsr = await _userSvc.GetUserByIdAllowNull(itmChild.UserId);
                            var childComment = new CourseCommentResponse();
                            childComment.CommentText = itmChild.Comment;
                            childComment.CommentId = itmChild.Id;
                            childComment.CourseId = courseId;
                            childComment.CreationDate = itmChild.CreationDate;
                            childComment.Email = childUsr != null ? childUsr.EmailAddress : "Account deleted";
                            childComment.FullName = childUsr != null ? $"{childUsr.FirstName?.ToString()} {childUsr.Surname?.ToString()}" : "Account deleted";
                            childComment.IsCourseAuthorComment = itmChild.IsCommentatorAuthorOfCourse;
                            childComment.IsDeleted = itmChild.IsDeleted;
                            childComment.IsEdited = itmChild.IsEdited;
                            childComment.IsMyComment = isAdmin?false: userId == itmChild.UserId;
                            childComment.LastUpdateDate = itmChild.LastUpdateDate.HasValue ? itmChild.LastUpdateDate.Value : itmChild.CreationDate;
                            childComment.UserId = itmChild.UserId;
                            childComment.CoachPhotoUrl = childUsr != null ? childUsr.AvatarUrl : "";
                            childComment.Nick = childUsr != null ?  childUsr.Nick : "Account deleted";

                            parentComment.Children.Add(childComment);

                            var childrenOfChildrenComments = await RecurrentComments(courseId, childComment.CommentId, isAdmin, userId, children.Where(x => x.ReplyCommentId != x.ParentCommentId).ToList(), null);

                            parentComment.Children.AddRange(childrenOfChildrenComments);
                        }
                    }

                    data.Add(parentComment);
                }


                return data;
            }
        }


        private async Task<List<CourseCommentResponse>> RecurrentComments(int courseId, int parentId, bool isAdmin, int? userId, List<CourseComment> comments, List<CourseCommentResponse> retData)
        {
            if(retData == null)
            {
                retData = new List<CourseCommentResponse>();
            }

            var firstLevel = comments.Where(x => x.ReplyCommentId.HasValue && x.ReplyCommentId.Value == parentId).OrderBy(x=>x.CreationDate).ThenBy(x=>x.LastUpdateDate).ToList();


           foreach(var itmChild in firstLevel)
            {
                var childUsr = await _userSvc.GetUserByIdAllowNull(itmChild.UserId);
                var childComment = new CourseCommentResponse();
                childComment.CommentText = itmChild.Comment;
                childComment.CommentId = itmChild.Id;
                childComment.CourseId = courseId;
                childComment.CreationDate = itmChild.CreationDate;
                childComment.Email = childUsr != null ? childUsr.EmailAddress : "Account deleted";
                childComment.FullName = childUsr != null ? $"{childUsr.FirstName?.ToString()} {childUsr.Surname?.ToString()}" : "Account deleted";
                childComment.IsCourseAuthorComment = itmChild.IsCommentatorAuthorOfCourse;
                childComment.IsDeleted = itmChild.IsDeleted;
                childComment.IsEdited = itmChild.IsEdited;
                childComment.IsMyComment = isAdmin || !userId.HasValue ? false : userId == itmChild.UserId;
                childComment.LastUpdateDate = itmChild.LastUpdateDate.HasValue ? itmChild.LastUpdateDate.Value : itmChild.CreationDate;
                childComment.UserId = itmChild.UserId;
                childComment.CoachPhotoUrl = childUsr != null ? childUsr.AvatarUrl : "";
                childComment.Nick = childUsr != null ? childUsr.Nick : "Account deleted";

                retData.Add(childComment);

                var recc = await RecurrentComments(courseId, itmChild.Id, isAdmin, userId, comments, retData);
            }


            return retData;
        }

        public async Task AddComent(int courseId, int userId, string commentTxt)
        {
            using(var ctx = new DataContext())
            {
                var course = await ctx.courses.Where(x => x.Id == courseId && x.State == Model.CourseState.APPROVED).FirstOrDefaultAsync();
                course.CheckExist("Course");

                var comment = new CourseComment();
                comment.Comment = commentTxt;
                comment.CommentLevel = 0;
                comment.UserId = userId;
                comment.CourseId = courseId;
                comment.CreationDate = DateTime.Now;
                comment.LastUpdateDate = DateTime.Now;
                comment.IsDeleted = false;
                comment.IsEdited = false;
                comment.IsCommentatorAuthorOfCourse = userId == course.UserId;
                ctx.CourseComments.Add(comment);
                await ctx.SaveChangesAsync();

                if (userId != course.UserId)
                {
                    await SendEmailToCourseOwnerAboutNewComment(course);
                }
            }
        }

        public async Task ReplyToComment(int commentId, int courseId, int userId, string commentTxt)
        {
            using(var ctx = new DataContext())
            {
                var parentComment = await ctx.CourseComments.Where(x => x.Id == commentId && x.CourseId == courseId).Include(c=>c.Course).FirstOrDefaultAsync();
                parentComment.CheckExist("Comment");

                int? parentId = null;
                if(parentComment.ParentCommentId.HasValue)
                {
                    parentId = parentComment.ParentCommentId.Value;
                }
                else
                {
                    parentId = parentComment.Id;
                }

                var comment = new CourseComment();
                comment.Comment = commentTxt;
                comment.CommentLevel = 1;
                comment.ParentCommentId = parentId;
                comment.UserId = userId;
                comment.CourseId = courseId;
                comment.CreationDate = DateTime.Now;
                comment.LastUpdateDate = DateTime.Now;
                comment.IsDeleted = false;
                comment.IsEdited = false;
                comment.IsCommentatorAuthorOfCourse = userId == parentComment.Course.UserId;
                comment.ReplyCommentId = parentComment.Id;
                ctx.CourseComments.Add(comment);
                await ctx.SaveChangesAsync();

                if (userId != parentComment.Course.UserId)
                {
                    await SendEmailToCourseOwnerAboutNewComment(parentComment.Course);
                }
            }
        }

        public async Task EditComment(int commentId, int courseId, int userId, string commentTxt)
        {
            using(var ctx = new DataContext())
            {
                var comment = await ctx.CourseComments.Where(x => x.Id == commentId && x.CourseId == courseId && x.UserId == userId).FirstOrDefaultAsync();
                comment.CheckExist("Comment");

                comment.Comment = commentTxt;
                comment.IsEdited = true;
                comment.LastUpdateDate = DateTime.Now;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task DeleteComment(int commentId, int courseId, int userId, bool isAdmin)
        {
            using (var ctx = new DataContext())
            {
                CourseComment comment = null;
                if(isAdmin)
                {
                    comment = await ctx.CourseComments.Where(x => x.Id == commentId && x.CourseId == courseId).FirstOrDefaultAsync();
                }
                else
                {
                    comment = await ctx.CourseComments.Where(x => x.Id == commentId && x.CourseId == courseId && x.UserId == userId).FirstOrDefaultAsync();
                }
                
                comment.CheckExist("Comment");

                comment.IsDeleted = true;
               

                await ctx.SaveChangesAsync();
            }
        }
    }
}

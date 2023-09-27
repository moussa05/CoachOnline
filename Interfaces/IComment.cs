using CoachOnline.Model.ApiResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IComment
    {
        Task AddComent(int courseId, int userId, string commentTxt);
        Task ReplyToComment(int commentId, int courseId, int userId, string commentTxt);
        Task<List<CourseCommentResponse>> GetCourseComments(int courseId, int? userId, bool isAdmin);
        Task DeleteComment(int commentId, int courseId, int userId, bool isAdmin);
        Task EditComment(int commentId, int courseId, int userId, string commentTxt);
    }
}

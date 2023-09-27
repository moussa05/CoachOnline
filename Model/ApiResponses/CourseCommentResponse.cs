using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class CourseCommentResponse
    {
        public string Email { get; set; }
        public string Nick { get; set; }
        public string CoachPhotoUrl { get; set; }
        public string FullName { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMyComment { get; set; }
        public bool IsCourseAuthorComment { get; set; }
        public List<CourseCommentResponse> Children { get; set; }
    }
}

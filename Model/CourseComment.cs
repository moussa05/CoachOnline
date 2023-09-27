using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class CourseComment
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public virtual User User { get; set; }
        public string Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEdited { get; set; }
        public List<CourseComment> Responses { get; set; }
        public int CommentLevel { get; set; }
        public int? ParentCommentId { get; set; }
        public int? ReplyCommentId { get; set; }
        public virtual CourseComment ParentComment { get; set; }
        public bool IsCommentatorAuthorOfCourse { get; set; }
    }
}

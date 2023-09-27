using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class CourseEval
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
        public bool IsLiked { get; set; }
    }
}

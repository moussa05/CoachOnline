using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class SuggestedCourse
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public decimal WatchedTime { get; set; }
        public DateTime CreationDay { get; set; }
    }
}

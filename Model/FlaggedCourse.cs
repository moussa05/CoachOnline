using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class FlaggedCourse
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreationDate { get; set; }

        public virtual Course Course { get; set; }
    }
}

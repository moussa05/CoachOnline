using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime FirstOpenedDate { get; set; }
        public DateTime LastOpenedDate { get; set; }
        public virtual User Student { get; set; }
        public virtual Course Course { get; set; }
        public List<StudentEpisode> StudentEpisodes { get; set; }
    }
}

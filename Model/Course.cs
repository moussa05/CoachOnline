using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public CourseState State { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string Description { get; set; }
        public virtual List<Episode> Episodes { get; set; }
        public string PhotoUrl { get; set; }
        public long Created { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<Rejection> RejectionsHistory { get; set; }
        public bool? HasPromo { get; set; }
        public int? PublishedCount { get; set; }
        public string BannerPhotoUrl { get; set; }
        public List<CourseEval> Evaluations { get; set; }
        public List<CourseComment> Comments { get; set; }

        public string PublicTargets { get; set; }
        public string Prerequisite { get; set; }
        public string Objectives { get; set; }
        public string CertificationQCM { get; set; }

    }
    public enum CourseState : byte { PENDING, REJECTED, APPROVED, UNPUBLISHED, BLOCKED }

}




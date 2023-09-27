using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Rejection
    {
        [Key]
        public int Id { get; set; }
        public string Reason { get; set; }
        public long Date { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

    }
}

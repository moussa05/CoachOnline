using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Terms
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime Created { get; set; }
    }
}

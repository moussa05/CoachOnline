using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Student
{
    public class UserEpisodeAttachemntPermission
    {
        public int UserId { get; set; }
        public int MediaId { get; set; }
        public string CurrentToken { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}

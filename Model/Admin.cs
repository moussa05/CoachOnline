using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime LastLogin { get; set; }
        public virtual List<AdminLogin> AdminLogins { get; set; }
    }

    public class AdminLogin
    {
        [Key]
        public int Id { get; set; }
        public string AuthToken { get; set; }
        public DateTime LoggedIn { get; set; }
    }
}

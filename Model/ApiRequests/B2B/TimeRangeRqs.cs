using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class TimeRangeRqs
    {

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class UserTypeWithTimeRangeRqs
    {
        public UserRoleType? Role { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class TimeRangeRqsWithToken: TimeRangeRqs
    {
        [Required]
        public string Token { get; set; }

    }

    public class RegisteredUsersRqs
    {
        public int? ProfessionId { get; set; }
        public string Gender { get; set; }
        public int? AgeGroupStart { get; set; }
        public int? AgeGroupEnd { get; set; }
    }

    public class RegisteredUsersTimeRangeRqs
    {

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? ProfessionId { get; set; }
        public string Gender { get; set; }
        public int? AgeGroupStart { get; set; }
        public int? AgeGroupEnd { get; set; }
    }

    public class RegisteredUsersRqsWithToken:TokenOnlyRequest
    {
        public int? ProfessionId { get; set; }
        public string Gender { get; set; }
        public int? AgeGroupStart { get; set; }
        public int? AgeGroupEnd { get; set; }
    }

    public class RegisteredUsersTimeRangeRqsWithToken:TokenOnlyRequest
    {

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? ProfessionId { get; set; }
        public string Gender { get; set; }
        public int? AgeGroupStart { get; set; }
        public int? AgeGroupEnd { get; set; }
    }
}

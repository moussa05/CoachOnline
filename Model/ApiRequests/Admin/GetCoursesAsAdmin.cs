using CoachOnline.Model.ApiRequests.ApiObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class GetCoursesAsAdminRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
        public int Count { get; set; }
        public int LastId { get; set; }
        public bool FromOldest { get; set; }
        public bool IncludeAll { get; set; }
    }

    public class GetCoursesAsAdminResponse
    {

        public List<GetCoursesAsAdminResponseData> ResponsePairs { get; set; }
        public int TotalCoursesCount { get; set; }
    }
    public class GetCoursesAsAdminResponseData
    {
        public UserShortAPI User { get; set; }
        public CourseAPI Course { get; set; }
        public string LastDeclineReason { get; set; }
        public long LastDeclineDate { get; set; }
    }

    public class UserShortAPI
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public UserAccountStatus Status { get; internal set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string PhotoUrl { get; set; }
        public string PhoneNo { get; set; }
        public string PostalCode { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string Bio { get; set; }
    }
}


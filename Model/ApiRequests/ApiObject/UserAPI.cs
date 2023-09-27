using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.ApiObject
{
    public class UserAPI
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string PhotoUrl { get; set; }
        public string PhoneNo { get; set; }
        public string ProfessionName { get; set; }
        public int? ProfessionId { get; set; }
        public string ProfilePhotoUrl
        {
            get { return $"images/{PhotoUrl}"; }
        }
        public string Bio { get; set; }
        public bool SocialLogin { get; set; }
        public string UserSubscriptionStatus { get; set; }
        public string PostalCode { get; set; }
        public UserRoleType UserRole { get; set; }
        public bool? PaymentEnabled { get; set; }
        public bool? WithdrawalsEnabled { get; set; }
        public UserAccountStatus Status { get; set; }
        public CompanyInfoAPI companyInfo { get; set; }
        public SubscriptionInfoApi subscriptionInfo { get; set; }
        public virtual List<CourseAPI> OwnedCourses { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? RankPosition { get; set; }
        public decimal? TotalMinutes { get; set; }
        public List<CourseResponseWithWatchedStatus> LastOpenedCourses { get; set; }
        public DateTime TrialEndDate { get; set; }
        public bool TrialActive { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }
        public string QuestionnaireResponse { get; set; }
        public string AffiliatorTypeStr
        {
            get
            {
                return AffiliatorType.ToString();
            }
        }
        public string UserRoleStr
        {
            get { 
                
                return UserRole.ToString(); }
        }
        public List<CategoryAPI> UserCategories { get; set; }
        public bool EmailConfirmed { get; set; }

        public CoachInfoDocumentAPI UserDocuments { get; set; }
    }

 

    public class SubscriptionInfoApi
    {

    }

    public class CompanyInfoAPI
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string SiretNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string RegisterAddress { get; set; }
        public string Country { get; set; }
        public string VatNumber { get; set; }
        public string PostalCode { get; set; }
        public string BICNumber { get; set; }
    }

    public class CoachInfoDocumentAPI
    {
        public string UserCV { get; set; }
        public List<string> Returns { get; set;}
        public List<string> Diplomas { get; set;}
    }

}

using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class UserBasicDataRequest
    {
        public string AuthToken { get; set; }
    }
    public class UserBasicDataResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public string Bio { get; set; }
        public string PhotoUrl { get; set; }
        public string Country { get; set; }
        public string PhoneNo { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string ProfilePhotoUrl { get { return $"images/{PhotoUrl}"; } }
        public bool SocialLogin { get; set; }
        public string UserRole { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }
        public string AffiliatorTypeStr
        {
            get { return AffiliatorType.ToString(); }
        }

        public string UserCategory { get; set; }
        [Range(0,3)]
        public int StripeVerificationStatus { get; set; }
        public List<CategoryAPI> categories { get; set; }
        //public bool HasStripeAccount { get; set; }
        //public bool FirstStageVerifed { get; set; }
        //public bool SecondStageVerifed { get; set; }
        /*
         
0 - Nothing
1 - Only stripe account
2 - Only Deposits
3 - Everything 
         */

        public UserBasicDataCompanyInfo CompanyInfo { get; set; }
    }

    public class UserBasicDataCompanyInfo
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string SiretNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string RegisterAddress { get; set; }
        public string Country { get; set; }
        public string VatNumber { get; set; }
        public string PostCode { get;  set; }
        public string BICNumber { get; set; }
    }
     
}

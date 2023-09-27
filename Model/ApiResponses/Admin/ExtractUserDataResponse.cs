using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class ExtractUserDataResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string SubscriptionPlan { get; set; }
        public string IsAffiliate { get; set; }
        public string GodfatherEmail { get; set; }
        public string GodfatherName { get; set; }

        public string HostGodfatherEmail { get; set; }
        public string HostGodfatherName { get; set; }
        public string UserType { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string Origin { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }

        public string LibraryName { get; set; }
        public string AffiliatorTypeStr { get
            {
                return AffiliatorType.ToString();
            } }


    }
}

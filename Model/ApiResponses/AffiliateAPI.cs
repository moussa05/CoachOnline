using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.AffiliateTypesDictionary;

namespace CoachOnline.Model.ApiResponses
{

   


    public class AffiliateAPI
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ChosenPlan { get; set; }
        public DateTime JoinDate { get; set; }
        public decimal EarnedMoney { get; set; }
        public string Currency { get; set; }
        public bool IsDirect { get; set; }
        public HostUserInfoAPI Host { get; set; }
        public string UserRole { get; set; }
        public List<AffiliateAPI> Affiliates { get; set; }
        public DateTime? PotentialNextPaymentDate { get; set; }
        public decimal? PotentialYearlyIncome { get; set; }
        public string Type { get; set; }
        public RetModel TooltipData { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }
        public string AffiliatorTypeStr
        {
            get { return AffiliatorType.ToString(); }
        }

        public string SubCancellationReason { get; set; }
    }

   

    public class HostUserInfoAPI
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRole {get;set;}
    }

    public class AffilationStatisticsResponse
    {
        public decimal TotalEarnings { get; set; }
      //  public decimal TotalEarningsThisMonth { get; set; }
       // public decimal TotalEarningsLast3Months { get; set; }
        public decimal PlatformEarningsFromAffilation { get; set; }
        public int AffiliationUsersTotal { get; set; }
        public string Currency { get; set; }

        public List<AffilationStatisticsHostResponse> Hosts { get; set; }
    }

    public class AffilationStatisticsHostResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string ChosenPlan { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public AffilationStatisticsHostResponse Parent { get; set; }
        public int AffiliatesTotal { get; set; }
        public int AffiliatesFirstLine { get; set; }
        public int AffiliatesSecondLine { get; set; }
        public int AffiliatesCoaches { get; set; }
        public int AffiliatesSubscribers { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalEarningsThisMonth { get; set; }
        public decimal TotalEarningsLast3Months { get; set; }
        public string Currency { get; set; }
        public List<AffiliateAPI> Affiliates { get; set; }
    }

}

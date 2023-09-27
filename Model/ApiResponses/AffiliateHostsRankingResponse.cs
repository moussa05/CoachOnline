using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class AffiliateHostsRankingResponse
    {
        public int HostId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public int RankId { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalSubscribersEarnings { get; set; }
        public decimal TotalCoachesEarnings { get; set; }
        public int AffiliateUsersTotal { get; set; }
        public int AffiliateSubscribersTotal { get; set; }
        public int SecondLineSubscribersTotal { get; set; }
        public int AffiliateCoachesTotal { get; set; }
        public string Currency { get; set; }
        public DateTime? AffiliationStartDate { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class AffiliateHostsRankingPagesResponse
    {
        public int PagesCount { get; set; }
        public int TotalRecordsCount { get; set; }
        public int PageRecordsCount { get; set; }
        public int PageNo { get; set; }
        public List<AffiliateHostsRankingResponse> Data { get; set; }
    }
}

using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.AffiliateService;

namespace CoachOnline.Interfaces
{
    public interface IAffiliate
    {
        Task ChangeAffiliationModelForUser(int userId, AffiliateModelType affType);
        Task<string> GenerateAffiliateLink(int userId);
        Task<string> GenerateAffiliateLinkForCoach(int userId);
        Task<string> ProposeAffiliateLink(int userId, string proposal);
        Task<string> ProposeAffiliateLinkForCoach(int userId, string proposal);
        Task CheckAffiliatePayments();
        Task<List<AffiliateAPI>> GetMyAffiliates(int userId);
        Task<AffiliateLink> GetTokenByAffLink(string link);
        Task<string> GetMyAffiliateLink(int userId);
        Task<string> GetMyAffiliateLinkForCoach(int userId);
        Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesGeneral(int userId);
        Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesForMonth(int userId, int month, int year);
        Task SendAffiliateEmailInvitation(int userId, string email);
        Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesBetweenDates(int userId, DateTime start, DateTime end);
        Task WithdrawPaymentByPaypal(int userId);
        Task CheckPaymentStatuses();
        Task<AffiliateSubscriptionStatus> CheckUserSubscription(int userId);
        Task<AffilationStatisticsResponse> GetAffiliateStats();
        Task<List<AffiliateHostsRankingResponse>> GetAffiliateHostsRanking(HostsRankingType type, bool topTen, int? userId = null);
        Task<AffiliateHostsRankingPagesResponse> GetAffiliateHostsRanking(HostsRankingType type, int page = 1, int? userId = null);
        Task<List<CouponResponse>> GetAvailableCouponsForUser(int userId);
        Task UpdateAffiliateLinkOptions(int userId, string link, LinkUpdateOptionsRqs rqs);
        Task<LinkOptsResponse> GetAffiliateLinkWithOptions(int userId, string link);

    }
}

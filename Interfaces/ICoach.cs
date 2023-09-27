using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Coach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface ICoach
    {
        Task<User> GetCoachByTokenAsync(string token);
        Task<User> GetCoachByIdAsync(int id);
        Task<CurrentPayTransferValueResponse> GetCurrentAmountToWidthraw(User coach);
        Task WithdrawPayment(User coach);
        Task<AccountDataResponse> GetAccountData(User coach);
        Task SuggestNewCategory(int userId, string categoryName, bool hasParent, int? parentId, bool adultOnly);
        Task<List<CategoryAPI>> GetParentCategoriesForSuggestion();
        Task<List<CoachSummarizedRankingReponse>> GetCurrentCoachRanking(int userId, int? month = null, int? year = null);
        Task<List<CoachSummarizedRankingReponse>> GetCurrentCoachesRankingAll();
    }
}

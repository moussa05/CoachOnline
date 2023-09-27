using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.ApiResponses.B2B;
using CoachOnline.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IB2BManagement
    {
        Task<int> CreateB2BAccount(string login, string password, string repeatPassword);
        Task DeleteB2BAccount(int accountId);
        Task DeleteLibraryAccount(int accountId);
        Task UpdateB2BAccountPassword(int accountId, string secret, string repeat);
        Task UpdateB2BAccountInfo(int accountId, UpdateB2BAccountRqs rqs);
        Task AddAccountSalesPerson(int accountId, AddB2BSalesPersonRqs rqs);
        Task RemoveAccountSalesPerson(int salesPersonId);
        Task UpdateAccountSalesPersonInfo(int salesPersonId, AddB2BSalesPersonRqs rqs);
        Task<List<B2BAccountResponse>> GetB2BAccounts();
        List<EnumOpt> GetPricingPeriods();
        List<EnumOpt> GetAccessTypes();
        Task<List<B2BPricing>> GetPricings();
        Task<int> AddPricingPlan(B2BAddPricingRqs rqs);
        Task RemovePricingPlan(int pricingPlanId);
        Task UpdatePricingPlan(int planId, B2BUpdatePricingRqs rqs);
        Task ManageServicesForB2BAccount(int accountId, ManageServicesForB2BAccountRqs rqs);
        Task<string> LoginToB2BAccount(string login, string password);
        Task<int> GetB2BAccountIdByToken(string token);
        Task<B2BAccountResponseWithAccountType> GetB2BAccount(int accountId);
        Task<int> CreateLibraryAccount(int b2bAccountId, CreateLibraryAccountRqs rqs);
        Task UpdateLibraryAccountInfo(int libraryAccId, UpdateLibraryAccountRqs rqs, int? b2bAccount = null);
        Task AddLibraryReferent(int libraryId, AddLibraryReferentRqs rqs, int? b2bAccount = null);
        Task UpdateLibraryReferent(int referentId, AddLibraryReferentRqs rqs, int? b2bAccount = null);
        Task DeleteLibraryReferent(int referentId, int? b2bAccount = null);
        Task<B2BLibraryResponse> GetLibraryAccount(int libraryId, int? b2bAccount = null);
        Task<List<B2BLibraryResponse>> GetB2BAccountClients(int b2bAccountId);
        Task AssignPricingPlanToLibrary(int libraryId, int planId, DateTime start, decimal? negotiatedPrice, bool autoRenew, int? b2bAccount = null);
        Task CancelLibrarySubscription(int subscriptionId, int? b2bId = null);
        Task CancelLibraryPricingPlan(int subId, int? b2bAccount = null);
        Task<string> GenerateInstitutionLink(int libraryId, string proposition, int? b2bAccount = null);
        Task<bool> IsB2BOwnerOfLibrary(int b2bId, int libId);
        Task UpdateLibraryAccountPassword(int libraryId, string secret, string repeat, int? b2bId = null);
        Task<List<B2BLibraryResponse>> GetAllLibraries();
        Task RenewSubscription(int libId, int subscriptionId, int? b2bId = null);
        Task AutoRenewSubscriptions();
        Task CheckLibrariesSubscriptionStates();
        Task UpdateLibrarySub(int libId, int subId, decimal? negotiatedPrice, bool? autoRenew, int? b2bId = null);
    }
}

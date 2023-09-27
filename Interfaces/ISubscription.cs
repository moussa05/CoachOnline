using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Student;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface ISubscription
    {
        Task UpdateSubscriptionPlans();
        Task<ICollection<BillingPlan>> GetSubscriptionPlans(int? userId= null, string affToken= null);
        Task<UserBillingPlan> SelectUserSubscriptionPlan(int subscriptionId, int userId);
        Task UploadStudentCardForSubscription(int subscriptionPlanId, List<PhotoBase64Rqs> photosInBase64);
        Task<UserBillingPlan> GetUserCurrentSubscriptionPlan(int userId);
        Task<UserBillingPlan> GetUserActiveSubscriptionPlan(int userId);
        Task<ICollection<UserBillingPlan>> GetAllUserSubscriptionPlans(int userId);
        Task<PaymentIntent> GetPaymentIntentById(string paymentIntentId);
        Task<Invoice> GetInvoiceById(string invoiceId);
        //Task AddUserStripePaymentMethod(User u, string paymentMethodId);
        Task AdminAcceptStudentCard(int subscriptionId);
        Task AdminRejectStudentCard(int subscriptionId, string rejectReason);
        Task CreateUserStripeCustomerAccount(User u);
        Task AddUserSubscription(User u);
        Task<Subscription> GetCustomerStripeSubscription(string subscriptionId);
        Task<ChangeSubscriptionResponse> ChangeSubscription(User u, int newSubscriptionId);
        Task<Customer> GetStripeCustomer(string stripeCustomerId);
        Task<PaymentMethod> GetCustomerStripePaymentMethod(string paymentSourceId);
        Task<CancelSubscriptionResponse> CancelSubscription(User u, int? cancelSubResp = null);
        Task<PaymentMethodResponse> GetCustomerDefaultPaymentMethod(User u);
        Task<InvoiceHeaderResponse> GetSubscriptionInvoices(User u);
        Task<List<InvoiceResponse>> GetUserInvoices(User u);
        Task<bool> IsCustomerDefaultPaymentACard(User u);
        Task<ICollection<StudentCardsToAcceptResponse>> AdminGetStudentCardsToAccept(int? status = null);
        Task<ChangeSubscriptionResponse> EnableStudentSubscriptionAfterStudentCardAccept(User u, int userSubscriptionPlanId);
        void SetSubscriptionState(ref UserBillingPlan billingPlan, Subscription sub);
        Task ChangeUserActiveSubscriptionState(int userId);
        int? GetUserBillingPlanIdFromSubscription(Subscription s);
        int? GetUserBillingPlanIdFromSubscriptionSchedule(SubscriptionSchedule s);
        Task UpdateSubscriptionStatesFromStripeForUser(User u);
        Task DeleteSubscriptionPlan(int userId, int subscriptionId);
        Task CancelScheduledSubscription(int userId, int subscriptionId);
        Task<ClientSecretResponse> GetPaymentIntentForSub(int userId, int userSubId);
        Task<ClientSecretResponse> CreateSetupIntent(User u);
        Task SetCustomerDefaultSource(User u, string payment_method_id);
        Task<bool> IsUserSubscriptionActive(int userId);
        Task<List<PaymentMethodResponse>> GetUserPaymentMethods(int userId);
        Task DeleteUserPaymentMethod(int userId, string paymentMethodId);
        Task UpdateUserPaymentMethod(int userId, string paymentMethodId, PaymentMethodBillingDetailsOptions billingOpts);

        Task<List<SubCancellationReasonResponse>> GetSubscriptionCancellationReasons();
    }
}

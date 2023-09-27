using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Student
{
    public class EndUserProfileDataResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string Bio { get; set; }
        public string PhotoUrl { get; set; }
        public bool SocialLogin { get; set; }
        public string Nick { get; set; }
       
        public string ProfilePhotoUrl
        {
            get { return $"images/{PhotoUrl}"; }
        }
        public string StripeCustomerId { get; set; }
        public string Gender { get; set; }
        public string PhoneNo { get; set; }
        public SubscriptionResponse Subscription { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime TrialEndDate { get; set; }
        public bool TrialActive { get; set; }
        public AccessType AccessType { get; set; }
        public string AccessTypeStr
        {
            get
            {
                return AccessType.ToString();
            }
        }
        public AffiliateModelType AffiliatorType { get; set; }
        public string AffiliatorTypeStr
        {
            get { return AffiliatorType.ToString(); }
        }
    }

    public enum AccessType : byte
    {
        FULL,
        PROMO,
        NO_ACCESS
    }

    public class SubscriptionResponse
    {
        public int SelectedPlanId {get;set;}
        public string SubscriptionId { get; set; }
        public decimal Price { get; set; }
        public string Period { get; set; }
        public string Currency { get; set; }
        public string SubscriptionName { get; set; }
        public string PaymentMethodId { get; set; }
        public CardResponse Card { get; set; }
        public DateTime? NextBillingTime { get; set; }
        public DateTime? SubscriptionCancelAt { get; set; }
    }

    public class CardResponse
    {
        public string Brand { get; set; }
        public string Last4Digits { get; set; }
        public string ValidTo { get; set; }
        public string Country {get;set;}

    }

    public class BillingDetailsResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class PaymentMethodResponse
    {
        public string StripePaymentMethodId { get; set; }
        public string StripeCustomerId { get; set; }
        public BillingDetailsResponse BillingDetails { get; set; }
        public CardResponse Card { get; set; }
    }

    public class InvoiceHeaderResponse
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string SubscriptionName { get; set; }
        public DateTime? NextBillingTime { get; set; }
        public decimal SubscriptionPrice { get; set; }
        public string SubscriptionPeriod { get; set; }
        public List<InvoiceResponse> Invoices { get; set; }
    }

    public class InvoiceResponse
    {
        public string InvoiceStripeId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Description { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public string CardLast4Digits { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal? Tax { get; set; }
        public string Currency { get; set; }
        public string InvoicePdf { get; set; }
    }
}

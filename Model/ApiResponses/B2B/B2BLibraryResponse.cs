using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.B2B
{
    public class B2BLibraryResponse
    {
        public int Id { get; set; }
        public int B2BAccountId { get; set; }
        public string InstitutionLink { get; set; }
        public string B2BAccountName { get; set; }
        public string Email { get; set; }
        public string LibraryName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string PhoneNo { get; set; }
        public string PhotoUrl { get; set; }
        public string Website { get; set; }
        public List<LibraryReferentResponse> referents { get; set; }
        public int? BooksNo { get; set; }
        public int? ReadersNo { get; set; }
        public int? CdsNo { get; set; }
        public int? VideosNo { get; set; }
        public string SIGBName { get; set; }
        public string Link { get; set; }
        public LibrarySubscriptionResponse ActiveSubscription { get; set; }
        public List<LibrarySubscriptionResponse> AllSubscriptions { get; set; }
    }

    public class LibraryReferentResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class LibrarySubscriptionResponse
    {
        public int Id { get; set; }
        public DateTime SubscriptionStart { get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public bool IsActive { get; set; }
        public int PricePlanId { get; set; }
        public string PricingName { get; set; }
        public int NumberOfActiveUsers { get; set; }
        public B2BPricingPeriod TimePeriod { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public B2BPricingAccessType AccessType { get; set; }
        public LibrarySubscriptionStatus Status { get; set; }
        public decimal? NegotiatedPrice { get; set; }

        public string TimePeriodStr { get { return TimePeriod.ToString(); } }
        public string AccessTypeStr { get { return AccessType.ToString(); } }
        public string StatusStr { get { return Status.ToString(); } }
    }
}

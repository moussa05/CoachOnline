using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.LibraryManagementService;

namespace CoachOnline.Model.ApiResponses.Admin
{

    public class B2BAccountResponseWithAccountType:B2BAccountResponse
    {
        public B2BAccountType AccountType { get; set; }
        public string AccountTypeStr { get { return AccountType.ToString(); } }
    }
    public class B2BAccountResponse
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string PhoneNo { get; set; }
        public string PhotoUrl { get; set; }
        public string Website { get; set; }
        public bool ContractSigned { get; set; }
        public DateTime? ContractSignDate { get; set; }
        public decimal? Comission { get; set; }
        public string ComissionCurrency { get; set; }

        public List<B2BSalesPersonResponse> AccountSalesPersons { get; set; }
        public List<B2BAccountServiceResponse> AvailableServices { get; set; }
    }

    public class B2BSalesPersonResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class B2BAccountServiceResponse
    {
        public int ServiceId { get; set; }
        public string PricingName { get; set; }
        public int NumberOfActiveUsers { get; set; }
        public B2BPricingPeriod TimePeriod { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public B2BPricingAccessType AccessType { get; set; }
        public decimal Comission { get; set; }
        public string ComissionCurrency { get; set; }

        [NotMapped]
        public string TimePeriodStr
        {
            get { return TimePeriod.ToString(); }
        }

        [NotMapped]
        public string AccessTypeStr
        {
            get { return AccessType.ToString(); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateB2BAccountRqs
    {
        public string AccountName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string PhoneNo { get; set; }
        public string PhotoBase64 { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Website { get; set; }
        public bool ContractSigned { get; set; }
        public DateTime? ContractSignDate { get; set; }
        public decimal? Comission { get; set; }
        public string ComissionCurrency { get; set; }
    }

    public class UpdateB2BAccountRqsWithToken: UpdateB2BAccountRqs
    {
        public string Token { get; set; }
    }

    public class AddB2BSalesPersonRqs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string PhotoBase64 { get; set; }
    }
}

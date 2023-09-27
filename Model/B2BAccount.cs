using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class B2BAccount
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AccountName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
        public string PhoneNo { get; set; }
        public bool ContractSigned { get; set; }
        public DateTime? ContractSignDate { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public decimal? Comission { get; set; }
        public string ComissionCurrency { get; set; }
        public List<B2BSalesPerson> AccountSalesPersons { get; set; }
        public List<B2BAccountService> AvailableServices { get; set; }
        public List<B2BAcessToken> AccessTokens { get; set; }
    }

    public enum AccountStatus : byte
    {
        ACTIVE,
        DELETED
    }

    public class B2BSalesPerson
    {
        [Key]
        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string ProfilePicUrl { get; set; }
        public int B2BAccountId { get; set; }
        public virtual B2BAccount B2BAccount { get; set; }
    }

    public class B2BAcessToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public long Created { get; set; }
        public long ValidTo { get; set; }
        public bool Disposed { get; set; }
        public int B2BAccountId { get; set; }
        public virtual B2BAccount B2BAccount { get; set; }
    }
}

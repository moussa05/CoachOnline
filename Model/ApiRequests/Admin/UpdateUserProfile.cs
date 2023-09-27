using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateUserProfileAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public string Bio { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNo { get; set; }
        public string Adress { get; set; }

        //public string UserCategory { get; set; }
    }

    public class UpdateUserBillingInfoAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public string SiretNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string RegisterAddress { get; set; }
        public string Country { get; set; }
        public string VatNumber { get; set; }
        public string BICNumber { get; set; }
        [MaxLength(15)]
        public string PostCode { get; set; }
    }

    public class UpdateCoachPhotoAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }
        public string Base64Photo { get; set; }
        public bool RemoveAvatar { get; set; }
    }

    public class UpdateCoachCVAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string Base64Photo { get; set; }
        public bool RemoveCV { get; set; }
    }

    public class UpdateCoachReturnsAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string Base64Photo { get; set; }
        public bool RemoveReturn { get; set; }
    }

    public class UpdateCoachAttestationAsAdminRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string Base64Photo { get; set; }
        public bool RemoveAttestation { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITSAuth.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string MainEmailAddress { get; set; }
        public string TelephoneNumber { get; set; }
        public string VatNumber { get; set; }
        public long Registered { get; set; }
        public AddressData AddressData { get; set; }
        public List<AuthorizedLogin> Logins { get; set; }
        public List<Claims> Claims { get; set; }
        public List<ResetPassword> ResetPasswords { get; set; }
        public UserType userType { get; set; }
        public enum UserType { Default, Personal, Company }

    }

}

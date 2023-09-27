using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class LibraryAccount
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int B2BAccountId { get; set; }
        public virtual B2BAccount B2BAccount { get; set; }

        public string LibraryName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string Website { get; set; }
        public string PhoneNo { get; set; }
        public string LogoUrl { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public List<LibraryReferent> Referents { get; set; }
        public List<LibraryAcessToken> AccessTokens { get; set; }
        public List<LibrarySubscription> Subscriptions { get; set; }

        public int? BooksNo { get; set; }
        public int? ReadersNo { get; set; }
        public int? CdsNo { get; set; }
        public int? VideosNo { get; set; }
        public string SIGBName { get; set; }
        public string InstitutionUrl { get; set; }
    }

    public class LibraryReferent
    {
        [Key]
        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string ProfilePicUrl { get; set; }
        public int LibraryAccountId { get; set; }
        public virtual LibraryAccount LibraryAccount { get; set; }
    }

    public class LibraryAcessToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public long Created { get; set; }
        public long ValidTo { get; set; }
        public bool Disposed { get; set; }
        public int LibraryAccountId { get; set; }
        public virtual LibraryAccount LibraryAccount { get; set; }
    }
}

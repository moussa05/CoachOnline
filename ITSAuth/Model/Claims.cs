using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITSAuth.Model
{
    public class Claims
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }

        public string SecretToken { get; set; }
        public string RefreshToken { get; set; }
        public string Identificator { get; set; }
        public string EmailAddress { get; set; }

        public long ClaimedDate { get; set; }

        public ClaimSource claimSource { get; set; }


        public enum ClaimSource { Default, Facebook, Google, Apple }
    }
}

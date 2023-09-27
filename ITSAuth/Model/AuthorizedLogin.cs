using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITSAuth.Model
{
    public class AuthorizedLogin
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public int LoggedDate { get; set; }
        public int ValidUntil { get; set; }
        public string DeviceInfo { get; set; }
        public string PlaceInfo { get; set; }
        public string IpAddress { get; set; }
        public bool Disposed { get; set; }

    }
}

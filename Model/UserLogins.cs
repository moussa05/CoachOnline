using System;
using System.ComponentModel.DataAnnotations;

namespace CoachOnline.Model
{
    public class UserLogins
    {
        [Key]
        public int Id { get; set; }
        public string AuthToken { get; set; }
        public long Created { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string PlaceInfo { get; set; }
        public long ValidTo { get; set; }
        public bool Disposed { get; set; }
        public string HubConnectionId { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsAllowedToWatch { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Affiliate
    {
        [Key]
        public int Id { get; set; }
        public int AffiliateUserId { get; set; }
        public int HostUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsAffiliateACoach { get; set; }
        public AffiliateModelType AffiliateModelType { get; set; }
    }

    public enum AffiliateModelType : byte
    {
        Regular,
        Influencer
    }
}

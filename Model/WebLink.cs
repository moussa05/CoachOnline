using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class WebLink
    {
        [Key]
        public int Id { get; set; }
        public string LinkUrl { get; set; }
        public int UserId { get; set; }
        public LinkType LinkType { get; set; }
    }

    public enum LinkType : byte
    {
        WEBSITE,
        LINKEDIN,
        INSTAGRAM,
        TWITTER,
        FACEBOOK
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.FAQ
{
    public class FAQTopicRqs
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string TopicName { get; set; }
        public string TopicBody { get; set; }
        public string Tags { get; set; }
    }
}

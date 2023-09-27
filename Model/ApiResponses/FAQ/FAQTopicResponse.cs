using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.FAQ
{
    public class FAQTopicResponse
    {
        public int TopicId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string TopicName { get; set; }
        public string TopicBody { get; set; }
        public string Tags { get; set; }
    }
}

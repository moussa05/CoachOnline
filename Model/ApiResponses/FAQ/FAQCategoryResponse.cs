using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.FAQ
{
    public class FAQCategoryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class FAQCategoryResponseWithTopics: FAQCategoryResponse
    {
        public List<TopicHeaderResponse> Topics { get; set; }
    }

    public class TopicHeaderResponse
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
    }
}

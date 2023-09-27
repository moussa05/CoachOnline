using CoachOnline.Model.ApiRequests.FAQ;
using CoachOnline.Model.ApiResponses.FAQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IFAQ
    {
        Task DeleteCategory(int catId);
        Task UpdateCategory(int catId, string categoryName);
        Task<int> AddCategory(string categoryName);
        Task<List<FAQCategoryResponse>> GetCategories();
        Task<List<FAQCategoryResponseWithTopics>> GetCategoriesWithTopics();
        Task<int> AddTopic(FAQTopicRqs rqs);
        Task DeleteTopic(int topicId);
        Task UpdateTopic(int topicId, FAQTopicRqs rqs);
        Task<FAQTopicResponse> GetTopic(int topicId);
        Task<List<FAQTopicResponse>> GetTopicsByCategory(int categoryId);
        Task<List<FAQTopicResponse>> SearchTopicsByPhrase(string text);
    }
}

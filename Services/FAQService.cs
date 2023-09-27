using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.FAQ;
using CoachOnline.Model.ApiResponses.FAQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class FAQService: IFAQ
    {
        private readonly ILogger<FAQService> _logger;
        public FAQService(ILogger<FAQService> logger)
        {
            _logger = logger;
        }


        public async Task<int> AddCategory(string categoryName)
        {
            using(var ctx = new DataContext())
            {
                var exists = await ctx.FAQCategories.AnyAsync(x => x.CategoryName.Trim().ToLower() == categoryName.Trim().ToLower());
                if(exists)
                {
                    throw new CoachOnlineException("Category with such name already exist.", CoachOnlineExceptionState.AlreadyExist);
                }

                FAQCategory c = new FAQCategory();
                c.CategoryName = categoryName;
                ctx.FAQCategories.Add(c);
                await ctx.SaveChangesAsync();

                return c.Id;
            }
        }

        public async Task UpdateCategory(int catId, string categoryName)
        {
            using (var ctx = new DataContext())
            {
                var exists = await ctx.FAQCategories.AnyAsync(x => x.CategoryName == categoryName);
                if (exists)
                {
                    throw new CoachOnlineException("Category with such name already exist.", CoachOnlineExceptionState.AlreadyExist);
                }
                var cat = await ctx.FAQCategories.FirstOrDefaultAsync(x => x.Id == catId);

                cat.CheckExist("FAQ Category");

                cat.CategoryName = categoryName;
                await ctx.SaveChangesAsync();
            }
        }

        public async Task DeleteCategory(int catId)
        {
            using (var ctx = new DataContext())
            {

                var cat = await ctx.FAQCategories.Where(x => x.Id == catId).Include(x=>x.Topics).FirstOrDefaultAsync();

                cat.CheckExist("FAQ Category");

                if(cat.Topics != null && cat.Topics.Any())
                {
                    throw new CoachOnlineException("Category contains topics. Cannot delete", CoachOnlineExceptionState.CantChange);
                }

                ctx.FAQCategories.Remove(cat);

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<List<FAQCategoryResponse>> GetCategories()
        {
            using(var ctx = new DataContext())
            {
                var categories = await ctx.FAQCategories.ToListAsync();

                var retData = new List<FAQCategoryResponse>();
                foreach(var cat in categories)
                {
                    retData.Add(new FAQCategoryResponse() { CategoryId = cat.Id, CategoryName = cat.CategoryName });
                }

                return retData;
            }
        }

        public async Task<List<FAQCategoryResponseWithTopics>> GetCategoriesWithTopics()
        {
            using (var ctx = new DataContext())
            {
                var categories = await ctx.FAQCategories.Include(t=>t.Topics).ToListAsync();

                var retData = new List<FAQCategoryResponseWithTopics>();
                foreach (var cat in categories)
                {
                    FAQCategoryResponseWithTopics x = new FAQCategoryResponseWithTopics();
                    x.CategoryId = cat.Id;
                    x.CategoryName = cat.CategoryName;
                    x.Topics = new List<TopicHeaderResponse>();

                    if(cat.Topics != null)
                    {
                        cat.Topics.ToList().ForEach(t => {
                            x.Topics.Add(new TopicHeaderResponse() { TopicId = t.Id, TopicName = t.Topic });
                        });
                    }

                    retData.Add(x);
                }

                return retData;
            }
        }

        public async Task<int> AddTopic(FAQTopicRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var cat = await ctx.FAQCategories.FirstOrDefaultAsync(x => x.Id == rqs.CategoryId);
                cat.CheckExist("FAQ Category");

                if(string.IsNullOrEmpty(rqs.TopicName))
                {
                    throw new CoachOnlineException("Topic header cannot be empty", CoachOnlineExceptionState.DataNotValid);
                }

                FAQTopic t = new FAQTopic();
                t.CategoryId = cat.Id;
                t.Body = rqs.TopicBody;
                t.Topic = rqs.TopicName;
                t.Tags = rqs.Tags;

                ctx.FAQTopics.Add(t);
                await ctx.SaveChangesAsync();

                return t.Id;
            }
        }

        public async Task DeleteTopic(int topicId)
        {
            using(var ctx = new DataContext())
            {
                var topic = await ctx.FAQTopics.FirstOrDefaultAsync(t => t.Id == topicId);

                topic.CheckExist("Topic");

                ctx.FAQTopics.Remove(topic);

                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateTopic(int topicId, FAQTopicRqs rqs)
        {

            using (var ctx = new DataContext())
            {
                var topic = await ctx.FAQTopics.FirstOrDefaultAsync(t => t.Id == topicId);

                topic.CheckExist("Topic");

                if(string.IsNullOrEmpty(rqs.TopicName))
                {
                    throw new CoachOnlineException("Topic cannot be empty", CoachOnlineExceptionState.DataNotValid);
                }

                var cat = await ctx.FAQCategories.FirstOrDefaultAsync(x => x.Id == rqs.CategoryId);
                cat.CheckExist("FAQ Category");

                topic.Body = rqs.TopicBody;
                topic.CategoryId = rqs.CategoryId;
                topic.Tags = rqs.Tags;
                topic.Topic = rqs.TopicName;
               

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<List<FAQTopicResponse>> GetTopicsByCategory(int categoryId)
        {
            using(var ctx = new DataContext())
            {
                var topics = await ctx.FAQTopics.Where(t => t.CategoryId == categoryId).Include(c=>c.Category).ToListAsync();
                var responseData = new List<FAQTopicResponse>();
                foreach(var topic in topics)
                {
                    FAQTopicResponse resp = new FAQTopicResponse();

                    resp.CategoryId = topic.CategoryId;
                    resp.CategoryName = topic.Category?.CategoryName;
                    resp.Tags = topic.Tags;
                    resp.TopicBody = topic.Body;
                    resp.TopicId = topic.Id;
                    resp.TopicName = topic.Topic;
                    responseData.Add(resp);
                }

                return responseData;
            }
        }

        public async Task<FAQTopicResponse> GetTopic(int topicId)
        {
            using(var ctx = new DataContext())
            {
                var topic = await ctx.FAQTopics.Where(x => x.Id == topicId).Include(c => c.Category).FirstOrDefaultAsync();
                if(topic != null)
                {
                    FAQTopicResponse resp = new FAQTopicResponse();

                    resp.CategoryId = topic.CategoryId;
                    resp.CategoryName = topic.Category?.CategoryName;
                    resp.Tags = topic.Tags;
                    resp.TopicBody = topic.Body;
                    resp.TopicId = topic.Id;
                    resp.TopicName = topic.Topic;
                    
                    return resp;
                }

                return null;
            }
        }

        public async Task<List<FAQTopicResponse>> SearchTopicsByPhrase(string text)
        {
            if(string.IsNullOrEmpty(text))
            {
                return new List<FAQTopicResponse>();
            }

            var respData = new List<FAQTopicResponse>();

            text = text.Trim().ToLower();
            text = Helpers.Extensions.RemoveDiacritics(text);

            var phrases = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            using(var ctx = new DataContext())
            {
                var data = await ctx.FAQTopics.Include(c => c.Category).ToListAsync();

                var lookForFullPhrase = data.Where(x =>
                       Helpers.Extensions.RemoveDiacritics(x.Topic.Trim().ToLower()).Contains(text) ||
                       (x.Tags != null && x.Tags.ToLower().Trim().Contains(text)) ||
                       (x.Category != null && x.Category.CategoryName != null && Helpers.Extensions.RemoveDiacritics(x.Category.CategoryName.Trim().ToLower()).Contains(text)) ||
                       (x.Body != null && Helpers.Extensions.RemoveDiacritics(Helpers.Extensions.RemoveHTMLTags(x.Body)).Contains(text)))
                        .ToList();

                lookForFullPhrase.ForEach(x => {
                    respData.Add(new FAQTopicResponse() { CategoryId = x.CategoryId, CategoryName = x.Category?.CategoryName, Tags = x.Tags, TopicBody = x.Body, TopicId = x.Id, TopicName = x.Topic });
                    });

                if (phrases.Length > 1)
                {
                    foreach (var phrase in phrases)
                    {
                        var tempData = data.Where(t=>lookForFullPhrase.Contains(t) == false)
                        .Where(x =>
                            Helpers.Extensions.RemoveDiacritics(x.Topic.Trim().ToLower()).Contains(phrase) ||
                            (x.Tags != null && x.Tags.ToLower().Trim().Contains(phrase)) ||
                            (x.Category != null && x.Category.CategoryName != null && Helpers.Extensions.RemoveDiacritics(x.Category.CategoryName.Trim().ToLower()).Contains(phrase)) ||
                            (x.Body != null && Helpers.Extensions.RemoveDiacritics(Helpers.Extensions.RemoveHTMLTags(x.Body)).Contains(phrase)))
                        .ToList();

                        if(tempData.Any())
                        {
                            tempData.ForEach(x => {
                                respData.Add(new FAQTopicResponse() { CategoryId = x.CategoryId, CategoryName = x.Category?.CategoryName, Tags = x.Tags, TopicBody = x.Body, TopicId = x.Id, TopicName = x.Topic });
                            });
                        }
                    }
                }
            }


            return CoachOnline.Helpers.Extensions.DistinctBy(respData, x => x.TopicId).ToList();

           // return respData.DistinctBy(x => x.TopicId).ToList();
        }
    }
}

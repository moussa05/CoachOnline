using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests.FAQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly ILogger<FAQController> _logger;
        private readonly IFAQ _faqSvc;
        public FAQController(ILogger<FAQController> logger, IFAQ faqSvc)
        {
            _logger = logger;
            _faqSvc = faqSvc;
        }

        [AllowAnonymous]
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories(bool includeTopics)
        {
            try
            {
                if (includeTopics)
                {
                    var resp = await _faqSvc.GetCategoriesWithTopics();

                    return new OkObjectResult(resp);
                }
                else
                {
                    var resp = await _faqSvc.GetCategories();

                    return new OkObjectResult(resp);
                }
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [Authorize(Roles ="ADMIN")]
        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody]FAQCategoryRqs rqs)
        {
            try
            {
                var resp = await _faqSvc.AddCategory(rqs.CategoryName);

                return new OkObjectResult(new { CategoryId = resp});
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("categories/{catId}")]
        public async Task<IActionResult> UpdateCategory(int catId, [FromBody] FAQCategoryRqs rqs)
        {
            try
            {
                await _faqSvc.UpdateCategory(catId, rqs.CategoryName);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("categories/{catId}")]
        public async Task<IActionResult> DeleteCategory(int catId)
        {
            try
            {
                await _faqSvc.DeleteCategory(catId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("topics")]
        public async Task<IActionResult> AddTopic([FromBody]FAQTopicRqs rqs)
        {
            try
            {
                var id = await _faqSvc.AddTopic(rqs);

                return new OkObjectResult(new { TopicId = id });
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("topics/{topicId}")]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            try
            {
                await _faqSvc.DeleteTopic(topicId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("topics/{topicId}")]
        public async Task<IActionResult> UpdateTopic(int topicId, [FromBody]FAQTopicRqs rqs)
        {
            try
            {
                await _faqSvc.UpdateTopic(topicId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [AllowAnonymous]
        [HttpGet("topics/byCat/{categoryId}")]
        public async Task<IActionResult> GetTopicsByCat(int categoryId)
        {
            try
            {
                var topics = await _faqSvc.GetTopicsByCategory(categoryId);

                return new OkObjectResult(topics);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [AllowAnonymous]
        [HttpGet("topics/{topicId}")]
        public async Task<IActionResult> GetTopic(int topicId)
        {
            try
            {
                var topics = await _faqSvc.GetTopic(topicId);

                return new OkObjectResult(topics);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [AllowAnonymous]
        [HttpGet("topics/search/{searchStr}")]
        public async Task<IActionResult> SerachTopicsByPhrase(string searchStr)
        {
            try
            {
                var data = await _faqSvc.SearchTopicsByPhrase(searchStr);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

    }
}

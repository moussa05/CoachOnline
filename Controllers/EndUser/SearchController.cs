using CoachOnline.ElasticSearch.Services;
using CoachOnline.Implementation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rollbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.EndUser
{
  
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ISearch _searchSvc;
        public SearchController(ILogger<SearchController> logger, ISearch searchSvc)
        {
            _logger = logger;
            _searchSvc = searchSvc;
        }

        [HttpPost]
        public async Task<IActionResult> ReindexCourses()
        {
            try
            {
                await _searchSvc.ReindexCourses();
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlatformBasicInfo()
        {
            try
            {
                var resp = await _searchSvc.GetPlatformBasicInfo();
                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReindexCoaches()
        {
            try
            {
                await _searchSvc.ReindexCoaches();
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReindexCategories()
        {
            try
            {
                await _searchSvc.ReindexCategories();
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReindexAll()
        {
            try
            {
                Console.WriteLine("reindexing");
                await _searchSvc.ReindexAll();
                Console.WriteLine("reindexed");
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string search)
        {
            try
            {
                var data = await _searchSvc.Find(search);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchByCat(string search)
        {
            try
            {
                //Console.WriteLine($"Searched phrase for cat: {search}");
                var data = await _searchSvc.SearchByCat(search);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCourses(string search)
        {
            try
            {
                var data = await _searchSvc.FindCourses(search);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCoaches(string search)
        {
            try
            {
                var data = await _searchSvc.FindCoaches(search);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCategories(string search)
        {
            try
            {
                var data = await _searchSvc.FindCategories(search);

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}

using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.ProductManageService;

namespace CoachOnline.Controllers.Admin
{
    [Authorize(Roles ="ADMIN")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductManageController : ControllerBase
    {
        private readonly ILogger<ProductManageController> _logger;
        private readonly IProductManage _productManageSvc;
        public ProductManageController(ILogger<ProductManageController> logger, IProductManage productManageSvc)
        {
            _logger = logger;
            _productManageSvc = productManageSvc;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var data = await _productManageSvc.GetProducts();
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

        [HttpPatch("products/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] PlanUpdateOptsRqs rqs)
        {
            try
            {
                await _productManageSvc.UpdateProduct(productId, rqs);
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

        [HttpPost("coupons")]
        public async Task<IActionResult> CreateCoupon([FromBody]CouponCreateRqs rqs)
        {
            try
            {
                await _productManageSvc.CreateCoupon(rqs.Name, rqs.Duration, rqs.PercentOff, rqs.AmountOff, rqs.Currency, rqs.DurationInMonths, rqs.ForInfluencers);
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

        [HttpPatch("coupons/{couponId}")]
        public async Task<IActionResult> UpdateCoupon(string couponId, [FromBody] CouponUpdateRqs rqs)
        {
            try
            {
                await _productManageSvc.UpdateCoupon(couponId, rqs.Name, rqs.ForInfluencers);
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

        [HttpDelete("coupons/{id}")]
        public async Task<IActionResult> DeleteCoupon(string id)
        {
            try
            {
                await _productManageSvc.DeleteCoupon(id);
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

        [HttpGet("coupons")]
        public async Task<IActionResult> GetCoupons()
        {
            try
            {
                var data = await _productManageSvc.GetCoupons();
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

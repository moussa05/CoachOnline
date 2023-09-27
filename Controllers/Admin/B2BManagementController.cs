using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.Admin
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class B2BManagementController : ControllerBase
    {
        private readonly ILogger<B2BManagementController> _logger;
        private readonly IB2BManagement _b2bSvc;
        private readonly ILibraryManagement _libSvc;
        public B2BManagementController(ILogger<B2BManagementController> logger, IB2BManagement b2bSvc, ILibraryManagement libSvc)
        {
            _logger = logger;
            _b2bSvc = b2bSvc;
            _libSvc = libSvc;
        }


        [HttpGet]
        public async Task<IActionResult> GetB2BAccounts()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = await _b2bSvc.GetB2BAccounts();

                return new OkObjectResult(result);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public IActionResult GetPricingPeriods()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = _b2bSvc.GetPricingPeriods();

                return new OkObjectResult(result);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public IActionResult GetPricingAcessTypes()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = _b2bSvc.GetAccessTypes();

                return new OkObjectResult(result);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPricings()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = await _b2bSvc.GetPricings();

                return new OkObjectResult(result);
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

        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> UpdateLibraryAccountPassword(int libraryId, [FromBody] UpdateB2BPasswordAdminRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateLibraryAccountPassword(libraryId, rqs.Password, rqs.RepeatPassword);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost("{subId}")]
        public async Task<IActionResult> CancelLibrarySubscription(int subId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.CancelLibrarySubscription(subId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPatch("{accountId}")]
        public async Task<IActionResult> UpdateB2BAccountPassword(int accountId, [FromBody] UpdateB2BPasswordAdminRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateB2BAccountPassword(accountId,rqs.Password, rqs.RepeatPassword); 

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpDelete("/api/[controller]/libraries/{accountId}")]
        public async Task<IActionResult> DeleteLibraryAccount(int accountId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.DeleteLibraryAccount(accountId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpDelete("/api/[controller]/b2b/{accountId}")]
        public async Task<IActionResult> DeleteB2BAccount(int accountId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.DeleteB2BAccount(accountId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPricing([FromBody]B2BAddPricingRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var id = await _b2bSvc.AddPricingPlan(rqs);

                return new OkObjectResult(new { Id = id });
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{pricingId}")]
        public async Task<IActionResult> UpdatePricing(int pricingId, [FromBody] B2BUpdatePricingRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdatePricingPlan(pricingId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpDelete("{pricingId}")]
        public async Task<IActionResult> DeletePricing(int pricingId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.RemovePricingPlan(pricingId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateB2BAccount([FromBody] CreateB2BAccountRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = await _b2bSvc.CreateB2BAccount(rqs.Login, rqs.Password, rqs.RepeatPassword);

                return new OkObjectResult(new { AccountId = result });
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

        [HttpPatch("{salesPersonId}")]
        public async Task<IActionResult> UpdateB2BSalesPersonInfo(int salesPersonId, [FromBody] AddB2BSalesPersonRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateAccountSalesPersonInfo(salesPersonId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{accountId}")]
        public async Task<IActionResult> ManageServicesToB2BAccount(int accountId, [FromBody] ManageServicesForB2BAccountRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.ManageServicesForB2BAccount(accountId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{accountId}")]
        public async Task<IActionResult> AddB2BSalesPerson(int accountId, [FromBody] AddB2BSalesPersonRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.AddAccountSalesPerson(accountId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{salesPersonId}")]
        public async Task<IActionResult> RemoveB2BSalesPerson(int salesPersonId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.RemoveAccountSalesPerson(salesPersonId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [HttpPatch("{accountId}")]
        public async Task<IActionResult> UpdateB2BAccount(int accountId, [FromBody] UpdateB2BAccountRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateB2BAccountInfo(accountId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost("/api/[controller]/libraries/{libraryId}")]
        public async Task<IActionResult> GetLibraryAccountInfo(int libraryId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _b2bSvc.GetLibraryAccount(libraryId);

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost("/api/[controller]/libraries/{libId}/sub/{subId}/renew")]
        public async Task<IActionResult> RenewLibrarySubscription(int libId, int subId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.RenewSubscription(libId, subId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("/api/[controller]/libraries/{libId}/sub/{subId}")]
        public async Task<IActionResult> UpdateLibrarySubscription(int libId, int subId,[FromBody] UpdateLibrarySubscriptionRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateLibrarySub(libId, subId, rqs.NegotiatedPrice, rqs.AutoRenew);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost("/api/[controller]/libraries")]
        public async Task<IActionResult> GetLibrariesInfo()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _b2bSvc.GetAllLibraries();
                return new OkObjectResult(resp);
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


        [HttpGet("{b2bId}")]
        public async Task<IActionResult> GetB2BLibraries(int b2bId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _b2bSvc.GetB2BAccountClients(b2bId);

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> AddLibraryReferent(int libraryId, [FromBody] AddLibraryReferentRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.AddLibraryReferent(libraryId, rqs);
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

        [HttpPatch("{referentId}")]
        public async Task<IActionResult> UpdateLibraryReferent(int referentId, [FromBody] AddLibraryReferentRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }
                await _b2bSvc.UpdateLibraryReferent(referentId, rqs);
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

        [HttpDelete("{referentId}")]
        public async Task<IActionResult> DeleteLibraryReferent(int referentId)
        {
            try
            {

                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }
                await _b2bSvc.DeleteLibraryReferent(referentId);
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

        [HttpPost("{b2bId}")]
        public async Task<IActionResult> CreateLibraryAccount(int b2bId, [FromBody] CreateLibraryAccountRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _b2bSvc.CreateLibraryAccount(b2bId, rqs);

                return new OkObjectResult(new { Id = resp });
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> UpdateLibraryAccount(int libraryId, [FromBody] UpdateLibraryAccountRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.UpdateLibraryAccountInfo(libraryId, rqs);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> GenerateInstitutionLink(int libraryId, [FromBody] GenerateInstitutionLinkRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = await _b2bSvc.GenerateInstitutionLink(libraryId, rqs.ProposedName);
                return new OkObjectResult(new { GeneratedName = result });
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


        [HttpPost("{libraryId}")]
        public async Task<IActionResult> AssignSubscriptionForLibrary(int libraryId, [FromBody] AddLibrarySubscriptionRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                await _b2bSvc.AssignPricingPlanToLibrary(libraryId, rqs.PricingPlanId, rqs.SubscriptionStartDate, rqs.NegotiatedPrice, rqs.AutoRenew);
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

        //[HttpDelete("{subId}")]
        //public async Task<IActionResult> CancelLibrarySubscription(int subId)
        //{
        //    try
        //    {
        //        var userId = User.GetUserId();
        //        if (!userId.HasValue)
        //        {
        //            throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
        //        }

        //        var userRole = User.GetUserRole();
        //        if (userRole != UserRoleType.ADMIN.ToString())
        //        {
        //            throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
        //        }

        //        await _b2bSvc.CancelLibraryPricingPlan(subId);
        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetStatsForLibraryFromBeggining(int libraryId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _libSvc.GetTotalConnectionsFromBeggining(libraryId);
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

        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetStatsForLibraryForTimePeriod(int libraryId, [FromBody] TimeRangeRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                if (!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }

                var data = await _libSvc.GetTotalConnectionsWithinTimeRange(libraryId, rqs.Start.Value, rqs.End.Value);
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


        [Authorize(Roles = "ADMIN")]
        [HttpPost("/api/[controller]/libraries/{libraryId}/chart")]
        public async Task<IActionResult> GetLibraryChartData(int libraryId, [FromBody] TimeRangeRqs rqs)
        {
            try
            {
                var data = await _libSvc.GetRegisteredUsersForChart(libraryId, rqs.Start, rqs.End);

                return new OkObjectResult(new { Registered = data });
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
        [HttpPost("/api/[controller]/libraries/{libraryId}/stats")]
        public async Task<IActionResult> GetLibraryStats(int libraryId, [FromBody] LibStatsRqsWithoutToken rqs)
        {
            try
            {
                var data = await _libSvc.GetLibraryStatsByKey(libraryId, rqs.Key, rqs.From, rqs.To);

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

        [Authorize(Roles = "ADMIN")]
        [HttpPost("/api/[controller]/libraries/{libraryId}/stats/excel")]
        public async Task<IActionResult> GetLibraryStatsXlsx(int libraryId, [FromBody] LibStatsRqsWithoutToken rqs)
        {
            try
            {
                var data = await _libSvc.GetLibraryStatsByKey(libraryId, rqs.Key, rqs.From, rqs.To);

                var lib = await _libSvc.GetLibraryInfo(libraryId);

                var excelResp = data.Data.Select(x => new {
                    LibraryId = lib.Id,
                    LibraryName = lib.LibraryName,               
                    data.TotalRegisteredUsers,
                    data.TotalConnections,
                    data.ConnectionsTotalTime,
                    data.ConnectrionsAverageTime,
                    data.CurrentlyConnected,
                    CalculatedBy = rqs.Key,
                    Key = x.Name,
                    LibraryConnectionsTotalTime = x.ConnectionsTotalTime,
                    LibraryConnectionsAverageTime = x.ConnectrionsAverageTime,
                    UsersCurrentlyConnectedToLibrary = x.CurrentlyConnected,
                    LibraryTotalConnections = x.TotalConnections,
                    LibraryTotalRegisteredUsers = x.TotalRegisteredUsers,


                }).ToList();

                string resp = "";
                await Task.Run(() => {
                    resp = Helpers.Extensions.WriteToExcel(excelResp, "LibraryStats");
                });

                string filename = $"export_libstats.xlsx";
                string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if (!System.IO.File.Exists(resp))
                {
                    throw new CoachOnlineException("File does not exists", CoachOnlineExceptionState.NotExist);
                }

                var memory = new System.IO.MemoryStream();

                using (var stream = new System.IO.FileStream(resp, System.IO.FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, mime, filename);
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


        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetRegisteredUsersWithinTimeRange(int libraryId, [FromBody] RegisteredUsersTimeRangeRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                if (!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }
 


                var data = await _libSvc.GetRegisteredUsersFilteredByProfessionGenderAndAgeGroupWithinTimeRange(libraryId, rqs.Start.Value, rqs.End.Value, rqs.ProfessionId, rqs.Gender, rqs.AgeGroupStart, rqs.AgeGroupEnd);

                return new OkObjectResult(new { Registered = data });
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

        [HttpGet("{b2bAccountId}")]
        public async Task<IActionResult> B2BAccount(int b2bAccountId)
        {

            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _b2bSvc.GetB2BAccount(b2bAccountId);

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

        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetRegisteredUsers(int libraryId,[FromBody] RegisteredUsersRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _libSvc.GetRegisteredUsersFilteredByProfessionGenderAndAgeGroup(libraryId, rqs.ProfessionId, rqs.Gender, rqs.AgeGroupStart, rqs.AgeGroupEnd);

                return new OkObjectResult(new { Registered = data });
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

        [HttpPatch("{profId}")]
        public async Task<IActionResult> UpdateProfessionName(int profId, [FromBody] string name)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }


                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                using (var ctx = new DataContext())
                {
                    var prof = await ctx.Professions.Where(t => t.Id == profId).FirstOrDefaultAsync();

                    if (prof != null)
                    {
                        prof.Name = name;

                        await ctx.SaveChangesAsync();
                    }
                }

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

    }
}

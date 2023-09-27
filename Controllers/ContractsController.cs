using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
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
    public class ContractsController : ControllerBase
    {
        private readonly ILogger<ContractsController> _logger;
        private readonly IContract _contractSvc;
        public ContractsController(ILogger<ContractsController> logger, IContract contractSvc)
        {
            _logger = logger;
            _contractSvc = contractSvc;
        }

        [Authorize(Roles ="ADMIN")]
        [HttpPost("contracts")]
        public async Task<IActionResult> AddContract([FromBody]AddContractRqs rqs)
        {
            try
            {
                var id = await _contractSvc.AddContract(rqs);
                return new OkObjectResult(new { ContractId = id });
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
        [HttpDelete("contracts/{contractId}")]
        public async Task<IActionResult> DeleteContract(int contractId)
        {
            try
            {
                await _contractSvc.DeleteContract(contractId);
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
        [HttpPatch("contracts/{contractId}")]
        public async Task<IActionResult> UpdateContract(int contractId,[FromBody]UpdateContractRqs rqs)
        {
            try
            {
                await _contractSvc.UpdateContract(contractId, rqs);
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
        [HttpGet("contracts/byType/{typeId}/latest")]
        public async Task<IActionResult> GetLatestContractByType(ContractType typeId)
        {
            try
            {
                var latest = await _contractSvc.GetLatestContract(typeId);
                return new OkObjectResult(latest);
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
        [HttpGet("contracts/byType/{typeId}")]
        public async Task<IActionResult> GetContractsByType(ContractType typeId)
        {
            try
            {
                var contracts = await _contractSvc.GetContracts(typeId);
                return new OkObjectResult(contracts);
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

using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.LibraryManagementService;

namespace CoachOnline.Controllers.B2B
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class B2BAccountController : ControllerBase
    {
        private readonly ILogger<B2BAccountController> _logger;
        private readonly IB2BManagement _b2bSvc;
        private readonly ILibraryManagement _libSvc;
        public B2BAccountController(ILogger<B2BAccountController> logger, IB2BManagement b2bSvc, ILibraryManagement libSvc)
        {
            _logger = logger;
            _b2bSvc = b2bSvc;
            _libSvc = libSvc;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] B2BLoginRqs rqs)
        {
            try
            {
                var authToken = await _b2bSvc.LoginToB2BAccount(rqs.Login, rqs.Password);
                return new OkObjectResult(new { AuthToken = authToken, AccountType = B2BAccountType.B2B_ACCOUNT, AccountTypeStr = B2BAccountType.B2B_ACCOUNT.ToString() });
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

        [HttpGet]
        public async Task<IActionResult> GetMyAccountInfo(string token)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(token);
                var account = await _b2bSvc.GetB2BAccount(b2bId);
                return new OkObjectResult(account);
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

        [HttpPost]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateB2BPasswordUserRqs rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.UpdateB2BAccountPassword(b2bId, rqs.Password, rqs.RepeatPassword);
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


        [HttpDelete("/api/[controller]/libraries/{accountId}")]
        public async Task<IActionResult> DeleteLibraryAccount(int accountId, [FromBody] TokenOnlyRequest rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                if (await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, accountId))
                {
                    await _b2bSvc.DeleteLibraryAccount(accountId);
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
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost("{libraryId}")]
        public async Task<IActionResult> UpdateLibraryPassword(int libraryId, [FromBody] UpdateB2BPasswordUserRqs rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.UpdateLibraryAccountPassword(libraryId, rqs.Password, rqs.RepeatPassword, b2bId);
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

        [HttpPost("{subId}")]
        public async Task<IActionResult> CancelLibrarySubscription(int subId, [FromBody]TokenOnlyRequest rqs)
        {
            try
            {

                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.CancelLibrarySubscription(subId, b2bId);

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
        public async Task<IActionResult> CreateLibraryAccount([FromBody] CreateLibraryAccountWithTokenRqs rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.CreateLibraryAccount(b2bId, rqs);
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

        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> UpdateLibraryAccountInfo(int libraryId, [FromBody] UpdateLibraryAccountRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.UpdateLibraryAccountInfo(libraryId, rqs, b2bId);
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

        [HttpPatch]
        public async Task<IActionResult> UpdateB2BAccount([FromBody] UpdateB2BAccountRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);

                await _b2bSvc.UpdateB2BAccountInfo(b2bId, rqs);

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
        public async Task<IActionResult> AddLibraryReferent(int libraryId, [FromBody] AddLibraryReferentRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.AddLibraryReferent(libraryId, rqs, b2bId);
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
        public async Task<IActionResult> UpdateLibraryReferent(int referentId, [FromBody] AddLibraryReferentRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.UpdateLibraryReferent(referentId, rqs, b2bId);
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
        public async Task<IActionResult> DeleteLibraryReferent(int referentId, [FromBody]TokenOnlyRequest rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.DeleteLibraryReferent(referentId, b2bId);
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

        [HttpPost("/api/[controller]/libraries/{libraryId}")]
        public async Task<IActionResult> GetLibraryAccountInfo(int libraryId, [FromBody] TokenOnlyRequest rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                var resp = await _b2bSvc.GetLibraryAccount(libraryId, b2bId);
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

        [HttpPost("/api/[controller]/libraries")]
        public async Task<IActionResult> GetLibrariesInfo([FromBody] TokenOnlyRequest rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
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
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{libraryId}")]
        public async Task<IActionResult> GenerateInstitutionLink(int libraryId, [FromBody] GenerateInstitutionLinkRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                var result = await _b2bSvc.GenerateInstitutionLink(libraryId, rqs.ProposedName, b2bId);
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
        public async Task<IActionResult> AssignSubscriptionForLibrary(int libraryId, [FromBody] AddLibrarySubscriptionRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.AssignPricingPlanToLibrary(libraryId, rqs.PricingPlanId, rqs.SubscriptionStartDate, rqs.NegotiatedPrice, rqs.AutoRenew, b2bId);
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

        [HttpPost("/api/[controller]/libraries/{libId}/sub/{subId}/renew")]
        public async Task<IActionResult> RenewLibrarySubscription(int libId, int subId,[FromBody]TokenOnlyRequest rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);

                await _b2bSvc.RenewSubscription(libId, subId, b2bId);

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
        public async Task<IActionResult> UpdateLibrarySubscription(int libId, int subId, [FromBody] UpdateLibrarySubscriptionRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libId);

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

        [HttpPost("/api/[controller]/libraries/{libraryId}/chart")]
        public async Task<IActionResult> GetLibraryChartData(int libraryId, [FromBody] TimeRangeRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);

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


        [HttpPost("/api/[controller]/libraries/{libraryId}/stats")]
        public async Task<IActionResult> GetLibraryStats(int libraryId,[FromBody]LibStatsRqs rqs)
        {
            try
            {


                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);

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

        [HttpPost("/api/[controller]/libraries/{libraryId}/stats/excel")]
        public async Task<IActionResult> GetLibraryStatsXlsx(int libraryId, [FromBody] LibStatsRqs rqs)
        {
            try
            {

                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);

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

        //[HttpPost("{subId}")]
        //public async Task<IActionResult> CancelLibrarySubscription(int subId, [FromBody]TokenOnlyRequest rqs)
        //{
        //    try
        //    {

        //        var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
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
        public async Task<IActionResult> GetStatsForLibraryFromBeggining(int libraryId, [FromBody] TokenOnlyRequest rqs)
        {
            try
            {

                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);
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
        public async Task<IActionResult> GetStatsForLibraryForTimePeriod(int libraryId, [FromBody] TimeRangeRqsWithToken rqs)
        {
            try
            {
                if (!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);
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


        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetRegisteredUsersWithinTimeRange(int libraryId, [FromBody] RegisteredUsersTimeRangeRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);
                if (!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }
             
                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);

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

        [HttpPost("{libraryId}")]
        public async Task<IActionResult> GetRegisteredUsers(int libraryId, [FromBody] RegisteredUsersRqsWithToken rqs)
        {
            try
            {
                var b2bId = await _b2bSvc.GetB2BAccountIdByToken(rqs.Token);

                await _b2bSvc.IsB2BOwnerOfLibrary(b2bId, libraryId);

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
    }
}

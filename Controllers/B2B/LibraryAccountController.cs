using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.B2B;
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
    public class LibraryAccountController : ControllerBase
    {
        private readonly ILogger<LibraryAccountController> _logger;
        private readonly ILibraryManagement _libSvc;
        private readonly IB2BManagement _b2bMgmtSvc;
        public LibraryAccountController(ILogger<LibraryAccountController> logger, ILibraryManagement libSvc, IB2BManagement b2bMgmtSvc)
        {
            _logger = logger;
            _libSvc = libSvc;
            _b2bMgmtSvc = b2bMgmtSvc;
           
        }

     
        [HttpPost]
        public async Task<IActionResult> GetAccountInfo([FromBody]TokenOnlyRequest rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);
                var data = await _b2bMgmtSvc.GetLibraryAccount(libId);
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

        [HttpPatch]
        public async Task<IActionResult> UpdateLibraryAccountInfo([FromBody] UpdateLibraryAccountRqsWithToken rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);

 

                await _b2bMgmtSvc.UpdateLibraryAccountInfo(libId, rqs);
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

        [HttpPost]
        public async Task<IActionResult> UpdateLibraryAccountPassword([FromBody]ChangePasswordRequest rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.AuthToken);
                await _libSvc.UpdateLibraryAccountPassword(libId, rqs.Password, rqs.PasswordRepeat, rqs.OldPassword);
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

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LibraryLoginRqs rqs)
        {
            try
            {
                var authToken = await _libSvc.Login(rqs.Login, rqs.Password);
                return new OkObjectResult(new { AuthToken = authToken, AccountType = B2BAccountType.LIBRARY_ACCOUNT, AccountTypeStr = B2BAccountType.LIBRARY_ACCOUNT.ToString() });
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
        public async Task<IActionResult> GetStatsFromBeggining([FromBody] TokenOnlyRequest rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);
                var data= await _libSvc.GetTotalConnectionsFromBeggining(libId);

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

        [HttpPost("/api/[controller]/libraries/{libraryId}/chart")]
        public async Task<IActionResult> GetLibraryChartData(int libraryId, [FromBody] TimeRangeRqsWithToken rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);
             
                if(libId != libraryId)
                {
                    throw new CoachOnlineException("Permission to access chart data denied", CoachOnlineExceptionState.PermissionDenied);
                }

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
        public async Task<IActionResult> GetLibraryStats(int libraryId, [FromBody] LibStatsRqs rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);

                if (libId != libraryId)
                {
                    throw new CoachOnlineException("Permission to access stats denied", CoachOnlineExceptionState.PermissionDenied);
                }

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
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);

                if (libId != libraryId)
                {
                    throw new CoachOnlineException("Permission to access stats denied", CoachOnlineExceptionState.PermissionDenied);
                }

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

        [HttpPost]
        public async Task<IActionResult> GetStatsWithinTimeRange([FromBody] TimeRangeRqsWithToken rqs)
        {
            try
            {
                if(!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);


                var data = await _libSvc.GetTotalConnectionsWithinTimeRange(libId, rqs.Start.Value, rqs.End.Value);

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



        [HttpPost]
        public async Task<IActionResult> GetRegisteredUsersWithinTimeRange([FromBody] RegisteredUsersTimeRangeRqsWithToken rqs)
        {
            try
            {
                if (!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Please provide start and end date.", CoachOnlineExceptionState.DataNotValid);
                }
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);


                var data = await _libSvc.GetRegisteredUsersFilteredByProfessionGenderAndAgeGroupWithinTimeRange(libId, rqs.Start.Value, rqs.End.Value, rqs.ProfessionId, rqs.Gender, rqs.AgeGroupStart, rqs.AgeGroupEnd);

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

        [HttpPost]
        public async Task<IActionResult> GetRegisteredUsers([FromBody] RegisteredUsersRqsWithToken rqs)
        {
            try
            {
                var libId = await _libSvc.GetLibraryAccountIdByToken(rqs.Token);


                var data = await _libSvc.GetRegisteredUsersFilteredByProfessionGenderAndAgeGroup(libId, rqs.ProfessionId, rqs.Gender, rqs.AgeGroupStart, rqs.AgeGroupEnd);

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

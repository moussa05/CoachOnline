using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests.B2B;
using ITSAuth.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.B2B
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InstitutionController : ControllerBase
    {
        private readonly IInstitution _instSvc;
        private readonly ILogger<InstitutionController> _logger;
        private readonly IAuthAsync _authSvc;

        public InstitutionController(ILogger<InstitutionController> logger, IInstitution instSvc, IAuthAsync authSvc)
        {
            _logger = logger;
            _instSvc = instSvc;
            _authSvc = authSvc;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfessions()
        {
            try
            {
                var professions = await _instSvc.GetProfessions();
                return new OkObjectResult(professions);
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

        [HttpGet("{institutionLink}")]
        public async Task<IActionResult> GetInstitutionInfo(string institutionLink)
        {
            try
            {
                var inst = await _instSvc.GetInstitutionInfo(institutionLink);
                return new OkObjectResult(inst);
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
        public async Task<IActionResult> RegisterStudentAccountForInstitution([FromBody] RegisterStudentInstAccountRqs rqs)
        {
            try
            {
                await _instSvc.RegisterWithInstitution(rqs.LibraryId, rqs.ProfessionId, rqs.Email, rqs.Password, rqs.RepeatPassword, rqs.Gender, rqs.YearOfBirth, rqs.FirstName, rqs.LastName, rqs.PhoneNo, rqs.City, rqs.Country, rqs.Region);
                var response = await _authSvc.GetAuthTokenWithUserDataAsync(rqs.Email, rqs.Password, "", "", "");
                return Ok(response);
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

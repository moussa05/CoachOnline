using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoachOnline.ElasticSearch.Services;
using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Statics;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using ITSAuth.Implementation.Email;
using ITSAuth.Interfaces;
using ITSAuth.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rollbar;

namespace CoachOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationController(IAuthAsync dataImpl, IEmailApiService _emailService, IPaymentService _paymentService, ILogger<AuthenticationController> logger, IUser userSvc, ISearch serachSvc)
        {
            this.dataImpl = dataImpl;
            this.emailService = _emailService;
            this.paymentService = _paymentService;
            this._logger = logger;
            this._userSvc = userSvc;
            this._searchSvc = serachSvc;
           // this._signInManager = signinMgr;
        }
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthAsync dataImpl;
        private readonly IPaymentService paymentService;
        private readonly IEmailApiService emailService;
        private readonly IUser _userSvc;
        private readonly ISearch _searchSvc;
        //private SignInManager<Model.User> _signInManager;
        [HttpPost]
        public async Task<ActionResult> CreateStripeAccount([FromBody] ConnectedAccountCreateRequest request)
        {
            try
            {
                await paymentService.GenerateConnectedAccount(request.AuthToken, request.CountryCode);
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



        //[HttpPost("/[controller]/google/login")]
        //public IActionResult GoogleLogin()
        //{
        //    var siteurl = "http://localhost:16475";//ConfigData.Config.SiteUrl
        //    var properties = new AuthenticationProperties { RedirectUri = $"{siteurl}/authentication/google" };
        //    return new ChallengeResult("Google", properties);
        //}

        //[HttpGet("/[controller]/google")]
        //public async Task<IActionResult> GoogleResponse()
        //{
        //    ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

        //    return new OkObjectResult(null);
        //}


        [HttpPost("/api/[controller]/google/login")]
        public async Task<IActionResult> AuthenticateByGoogle([FromBody] GoogleAuthRqs rqs)
        {
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

                // Change this to your google client ID
                settings.Audience = new List<string>() { ConfigData.Config.GoogleClientId };

                Console.WriteLine("ID TOKEN:");
                Console.WriteLine(rqs.IdToken);

                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(rqs.IdToken, settings);

                var resp = await _userSvc.SocialLogin(payload.Subject, "GOOGLE", rqs.DeviceInfo, rqs.PlaceInfo, rqs.IpAddress);

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


        //[HttpGet("/api/[controller]/signin-google")]
        //public async Task<IActionResult> SignInGoogle()
        //{
        //    try {
        //        var result = await HttpContext.AuthenticateAsync("External");

        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        _logger.LogInformation(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
 
        //}


        [HttpGet("/api/[controller]/signin-google", Name ="SignInGoogle")]
        public async Task<IActionResult> SignInGoogle(string returnUrl, GoogleCallType? callType)
        {
            try
            {
                string webPageReturn = "";
                if(string.IsNullOrEmpty(returnUrl))
                {
                    webPageReturn = $"{ConfigData.Config.WebUrl}";
                }
                else
                {
                    webPageReturn = $"{returnUrl}";
                }
                var result = await HttpContext.AuthenticateAsync("External");

                if(result.Succeeded)
                {
                    var idToken = result.Ticket.Properties.Items.FirstOrDefault(x => x.Key == ".Token.id_token").Value;
                    
                    var name = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName);
                    var surname = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname);
                    var email = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                    var userId = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                    if (callType.HasValue && callType == GoogleCallType.Login)
                    {
                        var resp = await _userSvc.SocialLogin(userId.Value, "GOOGLE", "", "", "");

                        return Redirect($"{webPageReturn}?userToken={resp.AuthToken}&gIdToken={idToken}");
                    }

                    return Redirect($"{webPageReturn}?gIdToken={idToken}");
                }

                //await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);

                return Redirect($"{webPageReturn}");
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return Redirect($"{ConfigData.Config.WebUrl}?errorMsg={e.Message}");
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return Redirect($"{ConfigData.Config.WebUrl}?errorMsg={e.Message}");
            }

        }

        public enum GoogleCallType:int
        {
            Login,
            Register
        }

        [HttpGet("/api/[controller]/google/challange")]
        public async Task<IActionResult> GoogleSignInChallange(string returnUrl, GoogleCallType? callType)
        {
            try
            {
                //foreach(var cookie in HttpContext.Request.Cookies)
                //{
                //    HttpContext.Response.Cookies.Delete(cookie.Key);
                //}
              
                var redirectUrl = $"{ConfigData.Config.SiteUrl}/api/authentication/signin-google?returnUrl={returnUrl}&callType={callType}";
                var properties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

     
                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    
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


        [HttpPost("/api/[controller]/google/register")]
        public async Task<IActionResult> RegisterByGoogle([FromBody] GoogleRegisterRqs rqs)
        {
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

                // Change this to your google client ID
                settings.Audience = new List<string>() { ConfigData.Config.GoogleClientId };


                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(rqs.IdToken, settings);


                var resp = await _userSvc.RegisterSocialLogin(payload.Subject, "GOOGLE", payload.Email, payload.GivenName, payload.FamilyName, payload.Picture, rqs.UserRole, rqs.DeviceInfo, rqs.PlaceInfo, rqs.IpAddress
                    , rqs.Gender, rqs.YearOfBirth, rqs.ProfessionId, rqs.LibraryId, rqs.AffiliateLink);


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

        [HttpPost]
        public async Task<ActionResult<ResponseWithUrl>> FirstStageVerification([FromBody] AuthTokenOnlyRequest request)
        {
            try
            {
                string url = await paymentService.VerificationFirstStage(request.AuthToken);
                return Ok(new ResponseWithUrl { Url = url });
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
        public async Task<ActionResult<ResponseWithUrl>> SecondStageVerification([FromBody] AuthTokenOnlyRequest request)
        {
            try
            {
                string url = await paymentService.VerificationKYCStage(request.AuthToken);
                return Ok(new ResponseWithUrl { Url = url });
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
        public async Task<ActionResult> CreateStripePrivateAccount([FromBody] ConnectedAccountCreateRequest request)
        {
            try
            {
                await paymentService.GenerateConnectedAccountAsPrivateUser(request.AuthToken, request.CountryCode);
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
        public async Task<ActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                await dataImpl.CreateCoachAccountAsync(request.EmailAddress, request.Password, request.RepeatedPassword, request.FirstName, request.LastName, request.PhoneNo, request.AffiliateLink);
                string confirmationToken = await dataImpl.CreateEmailConfirmationToken(request.EmailAddress);

                string body = $"<a href='{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}'>confirm account </a> <br><br> Token: {confirmationToken}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html");
                    body = body.Replace("##CONFIRMATIONURL###", $"{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}");
                }

                await emailService.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = request.EmailAddress,
                    ReceiverName = "",
                    Topic = "Coachs-online confirme votre adresse e-mail"
                });
                //Send email with account confirmation.
                var response = await dataImpl.GetAuthTokenWithUserDataAsync(request.EmailAddress, request.Password, "", "", "");
                return Ok(response);
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

        [HttpGet("{link}")]
        public async Task<IActionResult> GetAffiliateHostInfo(string link)
        {
            try
            {
                var data = await dataImpl.GetInfoAboutAffiliateHost(link);

                return Ok(data);
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
        public async Task<IActionResult> RegisterStudentAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                await dataImpl.CreateStudentAccountAsync(request.EmailAddress, request.Password, request.RepeatedPassword, request.FirstName, request.LastName, request.PhoneNo, request.AffiliateLink);

                string confirmationToken = await dataImpl.CreateEmailConfirmationToken(request.EmailAddress);

                string body = $"<a href='{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}'>confirm account </a> <br><br> Token: {confirmationToken}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html");
                    body = body.Replace("##CONFIRMATIONURL###", $"{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}");

                }

                await emailService.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = request.EmailAddress,
                    ReceiverName = "",
                    Topic = "Coachs-online confirme votre adresse e-mail"
                });

                var response = await dataImpl.GetAuthTokenWithUserDataAsync(request.EmailAddress, request.Password, "", "", "");
                return Ok(response);
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
        public async Task<ActionResult> ResendEmailVerification([FromBody] ResendEmailTokenRequest request)
        {
            try
            {
                string confirmationToken = await dataImpl.ResendEmailConfirmation(request.EmailAddress);
                string body = $"<a href='{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}'>confirm account </a> <br><br> Token: {confirmationToken}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html");
                    body = body.Replace("##CONFIRMATIONURL###", $"{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}");

                }
                await emailService.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = request.EmailAddress,
                    ReceiverName = "",
                    Topic = "Coachs-online confirme votre adresse e-mail"
                });

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
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                string confirmationToken = await dataImpl.ResetPasswordAsync(request.emailAddress);


                string body = $"<a href='{Statics.ConfigData.Config.WebUrl}/auth/resetPassword?Token={confirmationToken}'>Reset Password</a> <br><br> Token: {confirmationToken}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/ResetPassword.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/ResetPassword.html");
                    body = body.Replace("##CONFIRMATIONURL###", $"{Statics.ConfigData.Config.WebUrl}/auth/resetPassword?Token={confirmationToken}");

                }
                await emailService.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = request.emailAddress,
                    ReceiverName = "",
                    Topic = "Coachs-online réinitialiser votre mot de passe"
                });


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
        public async Task<ActionResult> ConfirmNewPassword([FromBody] ConfirmNewPasswordRequest request)
        {
            try
            {
                await dataImpl.ResetPasswordConfirmationAsync(request.emailAddress, request.Password, request.RepeatedPassword, request.ResetPasswordToken);

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
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await dataImpl.ChangePasswordAsync(request.AuthToken, request.Password, request.PasswordRepeat, request.OldPassword);
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
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GetAuthTokenResponse>> GetAuthToken([FromBody] GetAuthTokenRequest request)
        {
            try
            {
                var response = await dataImpl.GetAuthTokenWithUserDataAsync(request.Email, request.Password, request.DeviceInfo, request.IpAddress, request.PlaceInfo);
                return Ok(response);
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
        public async Task<ActionResult> ConfirmEmailToken([FromQuery] string Token)
        {
            try
            {
                await dataImpl.ConfirmEmailRegistrationAsync(Token);

                return Redirect($"{Statics.ConfigData.Config.WebUrl}/auth/login?email_confirmed=true");
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}/auth/login?email_confirmed=false&reason={e.Message}");
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}/?email_confirmed=false&reason=unknown");
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUserData([FromBody] UpdateUserDataRequest request)
        {
            try
            {
                await dataImpl.UpdateUserData(request.AuthToken, request.Name, request.Surname, request.YearOfBirth, request.City, request.Gender, request.Bio, 0, request.PhoneNo, request.Country, request.PostalCode, request.Address);
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
        public async Task<ActionResult<UpdateProfileAvatarResponse>> UpdateProfileAvatar([FromBody] UpdateProfileAvatarRequest request)
        {
            try
            {
                UpdateProfileAvatarResponse response = new UpdateProfileAvatarResponse { FileName = "" };
                if (request.RemoveAvatar)
                {
                    await dataImpl.RemoveAvatar(request.AuthToken);
                    return Ok(response);
                }
                else
                {
                    string file = await dataImpl.UpdateProfileAvatar(request.AuthToken, request.PhotoBase64);
                    response.FileName = file;
                    return Ok(response);
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

        [HttpPost]
        public async Task<ActionResult> UpdateCompanyInfo([FromBody] UpdateCompanyInfoRequest request)
        {
            try
            {
                await dataImpl.UpdateCompanyData(request.AuthToken, request.Name, request.City, request.SiretNumber, request.BankAccountNumber,
                    request.RegisterAddress, request.Country, request.VatNumber, request.PostCode, request.BICNumber);
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

        [HttpDelete]
        public async Task<ActionResult> DeleteAccount([FromBody] AuthTokenOnlyRequest request)
        {
            try
            {
                var user = await _userSvc.GetUserByTokenAsync(request.AuthToken);
                user.CheckExist("User");
                await _userSvc.DeleteAccount(user.Id);

                if (user.UserRole == UserRoleType.COACH)
                {
                    await _searchSvc.ReindexAll();
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

        [HttpPost]
        public async Task<ActionResult<UserBasicDataResponse>> GetUserBasicData([FromBody] UserBasicDataRequest request)
        {
            try
            {
                var response = await dataImpl.GetUserBasicData(request.AuthToken);
                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                // //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpGet]
        public async Task<ActionResult<GetCoursesCategoriesResponse>> GetUserCategories()
        {
            try
            {
                var response = await dataImpl.GetCategoriesForUsers();
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

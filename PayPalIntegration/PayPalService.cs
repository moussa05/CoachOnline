using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.PayPalIntegration.Model;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration
{
    public interface IPayPal
    {
        Task<PayPalUserResponse> GetUserPayPalAccountInfo(int coachId);
        Task VerifyAccount(int userId, string access_token);
        Task WithdrawPaymentByPaypal(int coachId);
        Task<PayPalPayoutResponse> Payout(string recipientEmail, decimal amount, string currency, string note, string email_subject, string email_message, string itemId, bool isRecipientEmail);
        Task CheckPaymentStatuses();
        Task<PayPalPayoutResponse> GetPaymentStatus(string payment_id);
    }
    public class PayPalService:IPayPal
    {
        private readonly ICoach _coachSvc;
        private readonly ILogger<PayPalService> _logger;
        public PayPalService(ILogger<PayPalService> logger, ICoach coachSvc)
        {
            _logger = logger;
            _coachSvc = coachSvc;
        }

        public async Task CheckPaymentStatuses()
        {
            try
            {
                using (var ctx = new DataContext())
                {
                    var checkTransactionStatus = await ctx.CoachDailyBalance.Where(t => !t.Transferred && t.Calculated && t.PayoutViaPaypal.HasValue && t.PayoutViaPaypal.Value && t.PayPalPayoutId != null).ToListAsync();

                    foreach (var tr in checkTransactionStatus)
                    {
                        var paymentstatus = await GetPaymentStatus(tr.PayPalPayoutId);
                        if (paymentstatus != null)
                        {
                            if (paymentstatus.batch_header.batch_status == "SUCCESS")
                            {
                                tr.Transferred = true;
                                await ctx.SaveChangesAsync();
                                Console.WriteLine("payout successfull after check");
                            }
                            else if (paymentstatus.batch_header.batch_status == "DENIED")
                            {
                                tr.Transferred = false;
                                tr.TransferDate = null;
                                tr.PayPalPayoutId = null;
                                await ctx.SaveChangesAsync();
                                Console.WriteLine("payout denied after check");
                            }
                            else if (tr.TransferDate.HasValue)
                            {
                                var dtnow = DateTime.Today.AddDays(-30);
                                if (tr.TransferDate < dtnow)
                                {
                                    tr.PayPalPayoutId = null;
                                    await ctx.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                Console.WriteLine("payment status in else");
                                Console.WriteLine(paymentstatus.batch_header.batch_status);
                            }
                        }
                        else
                        {
                            tr.PayPalPayoutId = null;
                            tr.TransferDate = null;
                            tr.PayoutViaPaypal = false;
                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
            catch(CoachOnlineException ex)
            {
                _logger.LogInformation(ex.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

        }

        public async Task VerifyAccount(int userId, string access_token)
        {
            try
            {
                PayPalConfiguration.client.DefaultRequestHeaders.Accept.Clear();
                PayPalConfiguration.client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                PayPalConfiguration.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{access_token}");

                var httpResponse = await PayPalConfiguration.client.GetAsync($"{ConfigData.Config.PayPalBaseUrl}/v1/identity/oauth2/userinfo?schema=paypalv1.1");

                if (httpResponse.IsSuccessStatusCode)
                {
                    if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                    {
                        var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                        using var streamReader = new StreamReader(contentStream);
                        using var jsonReader = new JsonTextReader(streamReader);

                        JsonSerializer serializer = new JsonSerializer();

                        try
                        {
                            var userInfo = serializer.Deserialize<PayPalUserInfo>(jsonReader);

                            Console.WriteLine("user id "+userInfo.user_id);
                            if (userInfo.emails != null)
                            {
                                var primary = userInfo.emails.FirstOrDefault(t => t.primary);
                                if(primary!= null)
                                {
                                    using (var ctx = new DataContext())
                                    {
                                        var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                                        usr.CheckExist("User");
                                        usr.PayPalPayerId = userInfo.payer_id;
                                        usr.PayPalPayerEmail = primary.value;
                                        await ctx.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    var emailData = userInfo.emails.FirstOrDefault();
                                    if(emailData!= null)
                                    {
                                        using (var ctx = new DataContext())
                                        {
                                            var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                                            usr.CheckExist("User");
                                            usr.PayPalPayerEmail = emailData.value;
                                            usr.PayPalPayerId = userInfo.payer_id;
                                            await ctx.SaveChangesAsync();
                                        }
                                    }
                                    else
                                    {
                                        throw new CoachOnlineException("No paypal email provided", CoachOnlineExceptionState.DataNotValid);
                                    }
                                }
                            }
                            Console.WriteLine("payer id "+userInfo.payer_id);
                            Console.WriteLine("name "+userInfo.name);

                         

                        }
                        catch (JsonReaderException)
                        {
                            throw new CoachOnlineException("Invalid JSON rsponse.", CoachOnlineExceptionState.DataNotValid);
                        }
                    }
                    else
                    {
                        Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
                    }
                }
                else
                {
                    throw new CoachOnlineException("Inavlid paypal token", CoachOnlineExceptionState.WrongToken);
                }
            }
            catch(Exception ex)
            {
                throw new CoachOnlineException(ex.ToString(), CoachOnlineExceptionState.UNKNOWN);
            }
        }

        private async Task<PayPalAccessTokenresponse> GetPayPalAccessToken()
        {
            PayPalAccessTokenresponse response = null;
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            var content = new FormUrlEncodedContent(values);

            var authenticationString = $"{ConfigData.Config.PayPalClientID}:{ConfigData.Config.PayPalSecret}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

            PayPalConfiguration.client.DefaultRequestHeaders.Accept.Clear();
            PayPalConfiguration.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            PayPalConfiguration.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{ConfigData.Config.PayPalBaseUrl}/v1/oauth2/token");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = content;

            //make the request
            var httpResponse = await PayPalConfiguration.client.SendAsync(requestMessage);

            if (httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    using var streamReader = new StreamReader(contentStream);
                    using var jsonReader = new JsonTextReader(streamReader);

                    JsonSerializer serializer = new JsonSerializer();

                    try
                    {
                        response = serializer.Deserialize<PayPalAccessTokenresponse>(jsonReader);
                    }
                    catch (Exception ex)
                    {
                        throw new CoachOnlineException("Invalid JSON rsponse.", CoachOnlineExceptionState.DataNotValid);
                    }
                }
            }

            return response;
        }

        public async Task<PayPalUserResponse> GetUserPayPalAccountInfo(int coachId)
        {
            PayPalUserResponse resp = new PayPalUserResponse();
            using (var ctx = new DataContext())
            {
                var coach = await ctx.users.FirstOrDefaultAsync(t => t.Id == coachId);
                coach.CheckExist("User");
                if (!(coach.UserRole == CoachOnline.Model.UserRoleType.COACH || coach.UserRole == CoachOnline.Model.UserRoleType.STUDENT))
                {
                    throw new CoachOnlineException("User does not have rights to view this information.", CoachOnlineExceptionState.NotAuthorized);
                }
                if (string.IsNullOrEmpty(coach.PayPalPayerId) && string.IsNullOrEmpty(coach.PayPalPayerEmail))
                {
                    throw new CoachOnlineException("User did not sign in via paypal to allow payouts.", CoachOnlineExceptionState.NotExist);
                }

                resp.PayPalEmail = coach.PayPalPayerEmail;
                resp.PayPalPayerId = coach.PayPalPayerId;
                resp.UserId = coachId;

                return resp;
            }
            return null;
        }


        public async Task<PayPalPayoutResponse> GetPaymentStatus(string payment_id)
        {
            var accessToken = await GetPayPalAccessToken();
            PayPalConfiguration.client.DefaultRequestHeaders.Accept.Clear();
            PayPalConfiguration.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Console.WriteLine("Token:");
            Console.WriteLine(accessToken.access_token);
            PayPalConfiguration.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{accessToken.access_token}");

            var httpResponse = await PayPalConfiguration.client.GetAsync($"{ConfigData.Config.PayPalBaseUrl}/v1/payments/payouts/{payment_id}");

            if (httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    using var streamReader = new StreamReader(contentStream);
                    using var jsonReader = new JsonTextReader(streamReader);

                    JsonSerializer serializer = new JsonSerializer();

                    try
                    {
                        var response = serializer.Deserialize<PayPalPayoutResponse>(jsonReader);

                        //Console.WriteLine("payout response valid");
                        //Console.WriteLine("status: " + response.batch_header.batch_status);
                        //Console.WriteLine("payout batch id: " + response.batch_header?.payout_batch_id);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Invalid JSON rsponse."+ex.ToString());
                        //throw new CoachOnlineException("Invalid JSON rsponse.", CoachOnlineExceptionState.DataNotValid);
                        return null;
                    }
                }
            }
            else
            {
                throw new CoachOnlineException("Payout invalid. Status code: " + httpResponse.StatusCode, CoachOnlineExceptionState.IncorrectFormat);
            }

            return null;

        }


        public async Task WithdrawPaymentByPaypal(int coachId)
        {
            using (var ctx = new DataContext())
            {

                var coach = await ctx.users.FirstOrDefaultAsync(t => t.Id == coachId);
                coach.CheckExist("Coach");
                if (coach.UserRole != CoachOnline.Model.UserRoleType.COACH)
                {
                    throw new CoachOnlineException("User is not a coach", CoachOnlineExceptionState.NotAuthorized);
                }
                if (string.IsNullOrEmpty(coach.PayPalPayerId) && string.IsNullOrEmpty(coach.PayPalPayerEmail))
                {
                    throw new CoachOnlineException("Coach did not sign in via paypal to allow payouts.", CoachOnlineExceptionState.NotExist);
                }


                var data = await _coachSvc.GetCurrentAmountToWidthraw(coach);
                string payment_id = "";
                

                if(data.ToWidthraw<=0)
                {
                    throw new CoachOnlineException("No funds to perform payout", CoachOnlineExceptionState.CantChange);
                }

                RequestedPayment reqPay = new RequestedPayment();
                reqPay.Currency = "";
                reqPay.PaymentValue = 0;
                reqPay.PayPalEmail = coach.PayPalPayerEmail;
                reqPay.PayPalPayerId = coach.PayPalPayerId;
                reqPay.RequestDate = DateTime.Now;
                reqPay.Status = RequestedPaymentStatus.Prepared;
                reqPay.UserId = coach.Id;
                reqPay.PaymentType = RequestedPaymentType.CoachPayout;
                ctx.RequestedPayments.Add(reqPay);
                await ctx.SaveChangesAsync();
                decimal amount = 0;
                string currency = "";
                foreach (var d in data.Balances)
                {
                    currency = d.Currency;
                    foreach (var x in d.DailyBalances)
                    {
                        var dB = await ctx.CoachDailyBalance.FirstOrDefaultAsync(e => e.Id == x.Id);
                        if (dB != null && dB.Calculated == true && dB.Transferred == false && dB.PayPalPayoutId == null && dB.RequestedPaymentId == null)
                        {                           
                    
                            if (dB.BalanceValue > 0)
                            {
                                dB.RequestedPaymentId = reqPay.Id;
                                dB.PayPalPayoutId = $"coach_payout_{reqPay.Id}";
                                dB.TransferDate = DateTime.Now;
                                dB.Transferred = true;
                                amount += Math.Round(dB.BalanceValue / 100, 2);
                                
                            }
                            dB.PayoutViaPaypal = true;
                            await ctx.SaveChangesAsync();

                        }

                    }
  
                }

                if (amount > 0)
                {
                    reqPay.PaymentValue = amount;
                    reqPay.Currency = currency;
                    reqPay.Status = RequestedPaymentStatus.Requested;
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    var dailyBalances = await ctx.CoachDailyBalance.Where(x => x.RequestedPaymentId == reqPay.Id && x.Calculated).ToListAsync();

                    foreach (var b in dailyBalances)
                    {
                        b.Transferred = false;
                        b.TransferDate = null;
                        b.PayPalPayoutId = null;
                        b.RequestedPaymentId = null;
                    }

                    ctx.RequestedPayments.Remove(reqPay);

                    await ctx.SaveChangesAsync();
                }

            }
        }
        //public async Task WithdrawPaymentByPaypal(int coachId)
        //{
        //    using (var ctx = new DataContext())
        //    {

        //        var coach = await ctx.users.FirstOrDefaultAsync(t => t.Id == coachId);
        //        coach.CheckExist("Coach");
        //        if(coach.UserRole != CoachOnline.Model.UserRoleType.COACH)
        //        {
        //            throw new CoachOnlineException("User is not a coach", CoachOnlineExceptionState.NotAuthorized);
        //        }
        //        if (string.IsNullOrEmpty(coach.PayPalPayerId) && string.IsNullOrEmpty(coach.PayPalPayerEmail))
        //        {
        //            throw new CoachOnlineException("Coach did not sign in via paypal to allow payouts.", CoachOnlineExceptionState.NotExist);
        //        }


        //        var data = await _coachSvc.GetCurrentAmountToWidthraw(coach);
        //        string payment_id = "";
        //        decimal amount = 0;
        //        string currency = "";
        //        foreach (var d in data.Balances)
        //        {      
        //                foreach (var x in d.DailyBalances)
        //                {
        //                    var dB = await ctx.CoachDailyBalance.FirstOrDefaultAsync(e => e.Id == x.Id);
        //                if (dB != null && !dB.Transferred && dB.Calculated == true && dB.PayPalPayoutId == null)
        //                {
        //                    payment_id = dB.Id.ToString();
        //                    dB.PayoutViaPaypal = true;

        //                    currency = d.Currency.ToUpper();
        //                    amount = dB.BalanceValue;
        //                    amount = Math.Round(amount / 100, 2);

        //                    if (amount > 0)
        //                    {
        //                        string payer_id = coach.PayPalPayerId != null ? coach.PayPalPayerId : coach.PayPalPayerEmail;
        //                        Console.WriteLine($"Payout amount: {amount}{currency}");
        //                        payment_id += $"_{DateTime.Now.ToString("HHmmss")}";

        //                        var payout = await Payout(payer_id, amount, currency, "CoachsOnline payout", "CoachsOnline - Payout for your PayPal account", $"Payout for  {dB.BalanceDay.ToString("dd-MM-yyyy")} has been sent.", payment_id, coach.PayPalPayerId == null);
        //                        if (payout != null)
        //                        {
        //                            dB.PayPalPayoutId = payout.batch_header.payout_batch_id;
        //                            dB.TransferDate = DateTime.Now;
        //                            await ctx.SaveChangesAsync();
        //                            Console.WriteLine("payout is not null");

        //                            var paymentstatus = await GetPaymentStatus(dB.PayPalPayoutId);
        //                            if (paymentstatus.batch_header.batch_status == "SUCCESS")
        //                            {
        //                                dB.Transferred = true;
        //                                await ctx.SaveChangesAsync();
        //                                Console.WriteLine("payout successfull");
        //                            }
        //                        }
        //                    }
        //                }
        //                else if (dB != null && !dB.Transferred && dB.Calculated == true && dB.PayoutViaPaypal.HasValue && dB.PayoutViaPaypal.Value && dB.PayPalPayoutId != null)
        //                {
        //                    var paymentstatus = await GetPaymentStatus(dB.PayPalPayoutId);
        //                    if (paymentstatus.batch_header.batch_status == "SUCCESS")
        //                    {
        //                        dB.Transferred = true;
        //                        await ctx.SaveChangesAsync();
        //                        Console.WriteLine("payout successfull after check");
        //                    }
        //                    else if (paymentstatus.batch_header.batch_status == "DENIED")
        //                    {
        //                        dB.Transferred = false;
        //                        dB.TransferDate = null;
        //                        dB.PayPalPayoutId = null;
        //                        await ctx.SaveChangesAsync();
        //                        Console.WriteLine("payout denied after check");
        //                    }
        //                    else if (dB.TransferDate.HasValue)
        //                    {
        //                        var dtnow = DateTime.Today.AddDays(-30);
        //                        if (dB.TransferDate < dtnow)
        //                        {
        //                            dB.PayPalPayoutId = null;
        //                            await ctx.SaveChangesAsync();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("payment status in else");
        //                        Console.WriteLine(paymentstatus.batch_header.batch_status);
        //                    }
        //                }

        //                }








        //            }

        //    }
        //}

        public async Task<PayPalPayoutResponse> Payout(string recipient, decimal amount, string currency, string note, string email_subject, string email_message, string itemId, bool isRecipientEmail)
        {
            try
            {
                using (var ctx = new DataContext())
                {

                    var payout = new PayPalPayoutRqs();
                    payout.items = new List<PayPalPayoutItem>();
                    payout.sender_batch_header = new PayPalSenderBatch() { email_message = email_message, email_subject = email_subject, sender_batch_id = "batch_"+itemId };
                    payout.items.Add(new PayPalPayoutItem()
                    {
                        receiver = recipient,
                        amount = new PayPalPayoutAmonut()
                        {
                            currency = currency,
                            value = amount.ToString().Replace(",", ".")
                        },
                        note = note,
                        recipient_type = isRecipientEmail ? "EMAIL": "PAYPAL_ID",
                        sender_item_id = "co_" + itemId

                    }); ;
                    var accessToken = await GetPayPalAccessToken();
                    if (accessToken == null)
                    {
                        throw new CoachOnlineException("Unable to get paypal token", CoachOnlineExceptionState.WrongToken);
                    }

                    PayPalConfiguration.client.DefaultRequestHeaders.Accept.Clear();
                    PayPalConfiguration.client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    Console.WriteLine("Token:");
                    Console.WriteLine(accessToken.access_token);
                    PayPalConfiguration.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{accessToken.access_token}");
                    var serializedJson = JsonConvert.SerializeObject(payout);
                    Console.WriteLine("JSON:");
                    Console.WriteLine(serializedJson);
                    HttpContent content = new StringContent(serializedJson, Encoding.ASCII, "application/json");
   
                    var httpResponse = await PayPalConfiguration.client.PostAsync($"{ConfigData.Config.PayPalBaseUrl}/v1/payments/payouts", content);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                        {
                            var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                            using var streamReader = new StreamReader(contentStream);
                            using var jsonReader = new JsonTextReader(streamReader);

                            JsonSerializer serializer = new JsonSerializer();

                            try
                            {
                                Console.WriteLine("Im inside");
                                var response = serializer.Deserialize<PayPalPayoutResponse>(jsonReader);

                                Console.WriteLine("payout response valid");
                                Console.WriteLine("status: "+ response.batch_header.batch_status);
                                Console.WriteLine("payout batch id: "+response.batch_header?.payout_batch_id);
                                return response;
                            }
                            catch (Exception ex)
                            {
                                throw new CoachOnlineException("Invalid JSON rsponse.", CoachOnlineExceptionState.DataNotValid);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Status code: " + httpResponse.StatusCode);
                        throw new CoachOnlineException("Payout invalid", CoachOnlineExceptionState.UNKNOWN);
                    }

                    return null;
                }
            }
            catch(CoachOnlineException ex)
            {
                throw (ex);
            }
            catch(Exception ex)
            {
                throw new CoachOnlineException(ex.ToString(), CoachOnlineExceptionState.UNKNOWN);
            }
        }
    }
}

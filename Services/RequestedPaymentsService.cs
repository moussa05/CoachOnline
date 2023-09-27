using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class RequestedPaymentsService: IRequestedPayments
    {
        private readonly ILogger<RequestedPaymentsService> _logger;
        public RequestedPaymentsService(ILogger<RequestedPaymentsService> logger)
        {
            _logger = logger;
        }

        public async Task<List<RequestedPaymentResponse>> GetPaypalWithdrawalRequestedPaymentsForUser(int userId)
        {
            var data = new List<RequestedPaymentResponse>();
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId);
                user.CheckExist("User");
                var requestedPayments = await ctx.RequestedPayments
                    .Where(u => u.UserId == userId && u.Status != RequestedPaymentStatus.Prepared)
                    .Include(a => a.Payments)
                    .Include(c => c.CoachPayments)
                    .ThenInclude(x => x.CoachBalanceMonth).ThenInclude(m => m.MonthlyBalance)
                    .ToListAsync();

                foreach (var p in requestedPayments)
                {
                    var resp = new RequestedPaymentResponse();
                    resp.Id = p.Id;
                    resp.UserId = user.Id;
                    resp.Email = user.EmailAddress;
                    resp.FirstName = user.FirstName;
                    resp.LastName = user.Surname;
                    resp.Value = p.PaymentValue;
                    resp.Currency = p.Currency;
                    resp.PayPalEmail = p.PayPalEmail;
                    resp.PayPalPayerId = p.PayPalPayerId;
                    resp.PaymentType = p.PaymentType;
                    resp.Status = p.Status;
                    resp.RejectReason = p.RejectReason;
                    resp.RequestDate = p.RequestDate;
                    resp.UpdateDate = p.StatusChangeDate;
                    resp.AffiliatePaymentsTotal = p.Payments != null ? p.Payments.Count : 0;
                    resp.PaymentsRequests = new List<AffiliaterequestedPaymentsResponse>();
                    resp.CoachRequestedPayments = new List<CoachRequestedPaymentResponse>();
                    resp.PayoutType = PayoutType.Paypal;
                    if (p.PaymentType == RequestedPaymentType.Affiliation)
                    {
                        foreach (var a in p.Payments)
                        {
                            resp.PaymentsRequests.Add(new AffiliaterequestedPaymentsResponse { Id = a.Id, AffiliateId = a.AffiliateId, Currency = a.PaymentCurrency, Value = a.PaymentValue, PaymentCreationDate = a.PaymentCreationDate });
                        }
                    }
                    else if (p.PaymentType == RequestedPaymentType.CoachPayout)
                    {
                        foreach (var c in p.CoachPayments)
                        {
                            resp.CoachRequestedPayments.Add(new CoachRequestedPaymentResponse { Id = c.Id, ForDay = c.BalanceDay, Value = Math.Round(c.BalanceValue / 100, 2), Currency = c?.CoachBalanceMonth?.MonthlyBalance?.Currency });
                        }
                    }
                    data.Add(resp);
                }

                if (user.UserRole == UserRoleType.COACH)
                {
                    var stripeData = await GetStripeWithdrawalRequestedPaymentsForCoach(userId);
                    data.AddRange(stripeData);
                }
            }

            data = data.OrderByDescending(x => x.RequestDate).ToList();

            return data;
        }

        public async Task<List<RequestedPaymentResponse>> GetPaypalWithdrawalRequestedPayments(RequestedPaymentStatus? status)
        {
            var data = new List<RequestedPaymentResponse>();
            using (var ctx = new DataContext())
            {
                List<RequestedPayment> requestedPayments = null;
                if (!status.HasValue)
                {
                    requestedPayments = await ctx.RequestedPayments.Where(u => u.Status != RequestedPaymentStatus.Prepared)
                     .Include(a => a.Payments)
                     .Include(c => c.CoachPayments)
                      .ThenInclude(x => x.CoachBalanceMonth).ThenInclude(m => m.MonthlyBalance)
                      .OrderByDescending(x => x.RequestDate).ToListAsync();
                }
                else
                {
                    requestedPayments = await ctx.RequestedPayments
                       .Where(u => u.Status == status.Value)
                       .Include(a => a.Payments)
                       .Include(c => c.CoachPayments)
                       .ThenInclude(x => x.CoachBalanceMonth).ThenInclude(m => m.MonthlyBalance)
                       .OrderByDescending(x => x.RequestDate).ToListAsync();
                }


                foreach (var p in requestedPayments)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == p.UserId);
                    var resp = new RequestedPaymentResponse();
                    resp.Id = p.Id;
                    resp.UserId = p.UserId;
                    resp.Email = user?.EmailAddress;
                    resp.FirstName = user?.FirstName;
                    resp.LastName = user?.Surname;
                    resp.Value = p.PaymentValue;
                    resp.Currency = p.Currency;
                    resp.PayPalEmail = p.PayPalEmail;
                    resp.PayPalPayerId = p.PayPalPayerId;
                    resp.PaymentType = p.PaymentType;
                    resp.Status = p.Status;
                    resp.RejectReason = p.RejectReason;
                    resp.RequestDate = p.RequestDate;
                    resp.UpdateDate = p.StatusChangeDate;
                    resp.AffiliatePaymentsTotal = p.Payments != null ? p.Payments.Count : 0;
                    resp.PaymentsRequests = new List<AffiliaterequestedPaymentsResponse>();
                    resp.CoachRequestedPayments = new List<CoachRequestedPaymentResponse>();
                    resp.PayoutType = PayoutType.Paypal;
                    if (p.PaymentType == RequestedPaymentType.Affiliation)
                    {
                        foreach (var a in p.Payments)
                        {
                            resp.PaymentsRequests.Add(new AffiliaterequestedPaymentsResponse { Id = a.Id, AffiliateId = a.AffiliateId, Currency = a.PaymentCurrency, Value = a.PaymentValue, PaymentCreationDate = a.PaymentCreationDate });
                        }
                    }
                    else if (p.PaymentType == RequestedPaymentType.CoachPayout)
                    {
                        foreach (var c in p.CoachPayments)
                        {
                            resp.CoachRequestedPayments.Add(new CoachRequestedPaymentResponse { Id = c.Id, ForDay = c.BalanceDay, Value = Math.Round(c.BalanceValue / 100, 2), Currency = c?.CoachBalanceMonth?.MonthlyBalance?.Currency });
                        }
                    }
                    data.Add(resp);
                }

                if(!status.HasValue || status.Value == RequestedPaymentStatus.Withdrawn)
                {
                    var stripeData = await GetStripeWithdrawalRequestedPaymentsForCoaches();

                    data.AddRange(stripeData);
                }

            }

            data = data.OrderByDescending(x => x.RequestDate).ToList();

            return data;
        }

        public async Task<List<RequestedPaymentResponse>> GetStripeWithdrawalRequestedPaymentsForCoach(int userId)
        {
            var data = new List<RequestedPaymentResponse>();
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(u => u.Id == userId && u.UserRole == UserRoleType.COACH);

                if (usr != null)
                {
                    var dailyBalances = await ctx.CoachDailyBalance.Include(cb => cb.CoachBalanceMonth).ThenInclude(m => m.MonthlyBalance).Where(x => x.Calculated && x.Transferred
                        && (!x.PayoutViaPaypal.HasValue || !x.PayoutViaPaypal.Value) && x.BalanceValue > 0 &&
                        x.CoachBalanceMonth.CoachId == usr.Id).ToListAsync();

                    var groupped = dailyBalances.GroupBy(x => new { x.CoachBalanceMonth.CoachId, x.TransferDate.Value.Date });

                    foreach (var item in groupped)
                    {
                        var payment = new RequestedPaymentResponse();
                        payment.CoachRequestedPayments = new List<CoachRequestedPaymentResponse>();

                        payment.Email = usr.EmailAddress;
                        payment.FirstName = usr.FirstName;
                        payment.LastName = usr.Surname;
                        payment.PaymentType = RequestedPaymentType.CoachPayout;
                        payment.PayoutType = PayoutType.Stripe;
                        payment.RequestDate = item.Key.Date;
                        payment.Status = RequestedPaymentStatus.Withdrawn;
                        payment.UserId = usr.Id;
                        payment.Value = 0;

                        item.ToList().ForEach(x => {
                            payment.Currency = x.CoachBalanceMonth.MonthlyBalance.Currency;
                            payment.Value += Math.Round(x.BalanceValue / 100, 2);
                            payment.CoachRequestedPayments.Add(new CoachRequestedPaymentResponse
                            { Currency = x.CoachBalanceMonth.MonthlyBalance.Currency, ForDay = x.BalanceDay, Id = x.Id, Value = Math.Round(x.BalanceValue / 100, 2) });
                        });

                        data.Add(payment);
                    }
                }
            }

            return data;
        }


        public async Task<List<RequestedPaymentResponse>> GetStripeWithdrawalRequestedPaymentsForCoaches()
        {
            var data = new List<RequestedPaymentResponse>();
            using (var ctx = new DataContext())
            {

                var dailyBalances = await ctx.CoachDailyBalance.Include(cb => cb.CoachBalanceMonth).ThenInclude(m => m.MonthlyBalance).Where(x => x.Calculated && x.Transferred
                    && (!x.PayoutViaPaypal.HasValue || !x.PayoutViaPaypal.Value) && x.BalanceValue > 0).ToListAsync();

                var groupped = dailyBalances.GroupBy(x => new { x.CoachBalanceMonth.CoachId, x.TransferDate.Value.Date });

                foreach (var item in groupped)
                {
                    var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == item.First().CoachBalanceMonth.CoachId);
                    if (usr != null)
                    {
                        var payment = new RequestedPaymentResponse();
                        payment.CoachRequestedPayments = new List<CoachRequestedPaymentResponse>();

                        payment.Email = usr.EmailAddress;
                        payment.FirstName = usr.FirstName;
                        payment.LastName = usr.Surname;
                        payment.PaymentType = RequestedPaymentType.CoachPayout;
                        payment.PayoutType = PayoutType.Stripe;
                        payment.RequestDate = item.Key.Date;
                        payment.Status = RequestedPaymentStatus.Withdrawn;
                        payment.UserId = usr.Id;
                        payment.Value = 0;

                        item.ToList().ForEach(x =>
                        {
                            payment.Currency = x.CoachBalanceMonth.MonthlyBalance.Currency;
                            payment.Value += Math.Round(x.BalanceValue / 100, 2);
                            payment.CoachRequestedPayments.Add(new CoachRequestedPaymentResponse
                            { Currency = x.CoachBalanceMonth.MonthlyBalance.Currency, ForDay = x.BalanceDay, Id = x.Id, Value = Math.Round(x.BalanceValue / 100, 2) });
                        });

                        data.Add(payment);
                    }
                }
            }


            return data;
        }

        public async Task AcceptPayPalWithdrawal(int withdrawalId)
        {
            using (var ctx = new DataContext())
            {
                var withdrawal = await ctx.RequestedPayments.Where(x => x.Id == withdrawalId).FirstOrDefaultAsync();
                withdrawal.CheckExist("Withdrawal");

                if (withdrawal.Status != RequestedPaymentStatus.Requested)
                {
                    throw new CoachOnlineException("Wrong withdrawal status", CoachOnlineExceptionState.AlreadyChanged);
                }

                withdrawal.Status = RequestedPaymentStatus.Withdrawn;
                withdrawal.StatusChangeDate = DateTime.Now;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task RejectPaypalWithdrawal(int withdrawalId, string reason)
        {
            using (var ctx = new DataContext())
            {
                var withdrawal = await ctx.RequestedPayments.Where(x => x.Id == withdrawalId)
                    .Include(p => p.Payments)
                    .Include(c => c.CoachPayments)
                    .FirstOrDefaultAsync();

                withdrawal.CheckExist("Withdrawal");

                if (withdrawal.Status != RequestedPaymentStatus.Requested)
                {
                    throw new CoachOnlineException("Wrong withdrawal status", CoachOnlineExceptionState.AlreadyChanged);
                }

                withdrawal.Status = RequestedPaymentStatus.Rejected;
                withdrawal.StatusChangeDate = DateTime.Now;
                withdrawal.RejectReason = reason;

                if (withdrawal.PaymentType == RequestedPaymentType.Affiliation)
                {

                    foreach (var p in withdrawal.Payments)
                    {
                        p.Transferred = false;
                        p.TransferDate = null;
                        p.RequestedPaymentId = null;
                        p.PayPalPayoutId = null;
                    }
                }
                else if (withdrawal.PaymentType == RequestedPaymentType.CoachPayout)
                {
                    foreach (var x in withdrawal.CoachPayments)
                    {
                        x.RequestedPaymentId = null;
                        x.TransferDate = null;
                        x.Transferred = false;
                        x.PayPalPayoutId = null;
                    }
                }

                await ctx.SaveChangesAsync();

            }
        }
    }
}

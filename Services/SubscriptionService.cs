using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Student;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class SubscriptionService:ISubscription
    {
        public static readonly string UserBillingPlanIdKey = "UserBillingPlanId";
        IUser _userSvc;
        ILibraryManagement _libSvc;
        public SubscriptionService(IServiceProvider svcProvider, IUser userSvc)
        {
            _userSvc = userSvc;
            _libSvc = (ILibraryManagement)svcProvider.GetService(typeof(ILibraryManagement)); ;
            StripeConfiguration.ApiKey = ConfigData.Config.StripeRk;
        }

        #region admin
        public async Task AdminAcceptStudentCard(int subscriptionId)
        {
            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == subscriptionId);
                plan.CheckExist("Plan");
                Console.WriteLine("UserBillingPlanId: "+plan.Id +" Status: " +plan.StatusStr);
                if (plan.StudentCardVerificationStatus != StudentCardStatus.IN_VERIFICATION)
                {
                    throw new CoachOnlineException($"Cannot accept student card. Wrong object state. Current state: {plan.StudentCardVerificationStatus}", CoachOnlineExceptionState.AlreadyChanged);
                }
                plan.StudentCardVerificationStatus = StudentCardStatus.ACCEPTED;
                if (plan.PlannedActivationDate != null)
                {
                    plan.Status = BillingPlanStatus.AWAITING_ACTIVATION;
                }
                else
                {
                    plan.Status = BillingPlanStatus.PENDING;
                }

                await ctx.SaveChangesAsync();
                var user = await _userSvc.GetUserById(plan.UserId);
                await EnableStudentSubscriptionAfterStudentCardAccept(user, plan.Id);
            }
        }

        public async Task AdminRejectStudentCard(int subscriptionId, string rejectReason)
        {
            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == subscriptionId);
                plan.CheckExist("Plan");
                if (plan.StudentCardVerificationStatus != StudentCardStatus.IN_VERIFICATION)
                {
                    throw new CoachOnlineException($"Cannot reject student card. Wrong object state. Current state: {plan.StatusStr}", CoachOnlineExceptionState.AlreadyChanged);
                }

                plan.Status = BillingPlanStatus.CANCELLED;
                plan.ExpiryDate = DateTime.Now;
                plan.StudentCardVerificationStatus = StudentCardStatus.REJECTED;

                plan.StudentCardRejection = new StudentCardRejection() { SubscriptionId = plan.Id, Reason = rejectReason };

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<ICollection<StudentCardsToAcceptResponse>> AdminGetStudentCardsToAccept(int? status = null)
        {
            using (var ctx = new DataContext())
            {
                List<StudentCardsToAcceptResponse> resp = new List<StudentCardsToAcceptResponse>();
                List<UserBillingPlan> subscriptions = new List<UserBillingPlan>();
                if (!status.HasValue)
                {
                    subscriptions = await ctx.UserBillingPlans
                        .Where(t => t.IsStudent &&
                        (t.StudentCardVerificationStatus == StudentCardStatus.ACCEPTED ||t.StudentCardVerificationStatus == StudentCardStatus.REJECTED || t.StudentCardVerificationStatus == StudentCardStatus.IN_VERIFICATION) &&
                        (t.Status != BillingPlanStatus.DELETED)).Include(u=>u.User).Include(sc => sc.StudentCardData)
                        .Where(u => u.User.Status != UserAccountStatus.DELETED)
                        .Include(b => b.BillingPlanType)
                        .ToListAsync();
                }
                else if(status.Value == (int)StudentCardStatus.IN_VERIFICATION)
                {
                    subscriptions = await ctx.UserBillingPlans
                      .Where(t => t.IsStudent && (t.StudentCardVerificationStatus == StudentCardStatus.IN_VERIFICATION) 
                      && (t.Status == BillingPlanStatus.PENDING || t.Status == BillingPlanStatus.AWAITING_ACTIVATION)).Include(sc => sc.StudentCardData).Include(u => u.User)
                      .Where(u => u.User.Status != UserAccountStatus.DELETED)
                      .Include(b => b.BillingPlanType)
                      .ToListAsync();
                }
                else if (status.Value == (int)StudentCardStatus.REJECTED)
                {
                    subscriptions = await ctx.UserBillingPlans
                      .Where(t => t.IsStudent && (t.StudentCardVerificationStatus == StudentCardStatus.REJECTED) && 
                      (t.Status != BillingPlanStatus.DELETED)).Include(sc => sc.StudentCardData).Include(u => u.User)
                      .Where(u => u.User.Status != UserAccountStatus.DELETED)
                      .Include(b => b.BillingPlanType)
                      .Include(r=>r.StudentCardRejection)
                      .ToListAsync();
                }
                else if (status.Value == (int)StudentCardStatus.ACCEPTED)
                {
                    subscriptions = await ctx.UserBillingPlans
                      .Where(t => t.IsStudent && (t.StudentCardVerificationStatus == StudentCardStatus.ACCEPTED) 
                      && (t.Status != BillingPlanStatus.DELETED)).Include(sc => sc.StudentCardData).Include(u => u.User)
                      .Where(u => u.User.Status != UserAccountStatus.DELETED)
                      .Include(b=>b.BillingPlanType)
                      .ToListAsync();
                }

                subscriptions.ForEach(el =>
                {
                    var itm = new StudentCardsToAcceptResponse();
                    itm.FirstName = el.User.FirstName;
                    itm.LastName = el.User.Surname;
                    itm.Email = el.User.EmailAddress;
                    itm.StudentCardData = el.StudentCardData;
                    itm.StudentCardRejection = el.StudentCardRejection;
                    itm.UserId = el.UserId;
                    itm.StudentCardVerificationStatus = el.StudentCardVerificationStatus;
                    itm.StripeSubscriptionId = el.StripeSubscriptionId;
                    itm.Status = el.Status;
                    itm.IsStudent = el.IsStudent;
                    itm.Id = el.Id;
                    itm.BillingPlanTypeId = el.BillingPlanTypeId;
                    itm.ActivationDate = el.ActivationDate;
                    itm.PlannedActivationDate = el.PlannedActivationDate;
                    itm.BillingPlanType = el.BillingPlanType;
                    itm.CreationDate = el.CreationDate;
                    itm.ExpiryDate = el.ExpiryDate;
                    itm.StripePriceId = el.StripePriceId;
                    itm.StripeSubscriptionId = el.StripeSubscriptionId;

                    resp.Add(itm);
                });

                return resp;
            }
        }
        #endregion


        public async Task DeleteUserPaymentMethod(int userId, string paymentMethodId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    throw new CoachOnlineException("User has no active stripe account", CoachOnlineExceptionState.NotExist);
                }
 
                var service = new PaymentMethodService();
                var paymentMethod= await service.GetAsync(paymentMethodId);

                if(paymentMethod.CustomerId != user.StripeCustomerId)
                {
                    throw new CoachOnlineException("Cannot delete payment method of another user", CoachOnlineExceptionState.PermissionDenied);
                }

                await service.DetachAsync(paymentMethodId);

            }
        }

        public async Task UpdateUserPaymentMethod(int userId, string paymentMethodId, PaymentMethodBillingDetailsOptions billingOpts)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    throw new CoachOnlineException("User has no active stripe account", CoachOnlineExceptionState.NotExist);
                }

                var service = new PaymentMethodService();
                var paymentMethod = await service.GetAsync(paymentMethodId);

                if (paymentMethod.CustomerId != user.StripeCustomerId)
                {
                    throw new CoachOnlineException("Cannot delete payment method of another user", CoachOnlineExceptionState.PermissionDenied);
                }

                var options = new PaymentMethodUpdateOptions
                {
                    BillingDetails = billingOpts
                };

                await service.UpdateAsync(paymentMethodId, options);

            }
        }

        public async Task<List<PaymentMethodResponse>> GetUserPaymentMethods(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    throw new CoachOnlineException("User has no active stripe account", CoachOnlineExceptionState.NotExist);
                }
                var options = new PaymentMethodListOptions
                {
                    Type = "card",
                    Customer = user.StripeCustomerId
                };
                var service = new PaymentMethodService();
                var paymentMethods = await service.ListAsync(options);

                List<PaymentMethodResponse> data = new List<PaymentMethodResponse>();
                if(paymentMethods != null && paymentMethods.Data.Count > 0)
                {
                    foreach(var pm in paymentMethods.Data)
                    {

                        PaymentMethodResponse resp = new PaymentMethodResponse();
                        resp.StripePaymentMethodId = pm.Id;
                        resp.StripeCustomerId = user.StripeCustomerId;
                        if (pm.BillingDetails != null)
                        {
                            BillingDetailsResponse b = new BillingDetailsResponse();
                            b.Email = pm.BillingDetails.Email;
                            b.Name = pm.BillingDetails.Name;
                            b.Street = pm.BillingDetails.Address?.Line1;
                            b.Street2 = pm.BillingDetails.Address?.Line2;
                            b.City = pm.BillingDetails.Address.City;
                            b.Country = pm.BillingDetails.Address.Country;
                            b.PostalCode = pm.BillingDetails.Address.PostalCode;

                            resp.BillingDetails = b;
                        }

                        if (pm.Card != null)
                        {
                            CardResponse c = new CardResponse();
                            c.Brand = pm.Card.Brand;
                            c.Last4Digits = pm.Card.Last4;
                            c.ValidTo = $"{pm.Card.ExpMonth}/{pm.Card.ExpYear}";
                            c.Country = pm.Card.Country;
                            resp.Card = c;
                        }

                        data.Add(resp);
                    }
                }

                return data;
            }
        }

        public async Task<ICollection<BillingPlan>> GetSubscriptionPlans(int? userId = null, string affToken = null)
        {
            await UpdateSubscriptionPlans();
            using (var ctx = new DataContext())
            {
                if (!userId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(affToken))
                    {
                        return await ctx.BillingPlans.Where(t => t.IsActive && t.IsPublic).Include(p => p.Price).ToListAsync();
                    }
                    else
                    {

                        var plans = await ctx.BillingPlans.Where(t => t.IsActive && t.IsPublic).Include(p => p.Price).ToListAsync();

                        var aff = await ctx.AffiliateLinks.FirstOrDefaultAsync(x => x.GeneratedToken == affToken);
                        if (aff != null)
                        {
                            if (aff.WithTrialPlans)
                            {
                                var hiddenPlans = await ctx.BillingPlans.Where(t => t.IsActive && !t.IsPublic).Include(p => p.Price).ToListAsync();

                                plans = plans.Union(hiddenPlans).ToList();
                            }

                            if (!string.IsNullOrWhiteSpace(aff.CouponCode))
                            {
                                var cpExists = await CheckCouponExists(aff.CouponCode);
                                if (cpExists)
                                {
                                    var promoC = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == aff.CouponCode);
                                    if (promoC != null)
                                    {
                                        var cResp = new CouponResponse();
                                        cResp.Id = promoC.Id;
                                        cResp.Name = promoC.Name;
                                        cResp.PercentOff = promoC.PercentOff;
                                        cResp.AmountOff = promoC.AmountOff;
                                        cResp.Currency = promoC.Currency;
                                        cResp.Duration = promoC.Duration;
                                        cResp.DurationInMonths = promoC.DurationInMonths;

                                        foreach (var p in plans)
                                        {
                                            if (p.Price.Amount.HasValue)
                                            {
                                                p.PromotionalPrice = p.Price.Amount;
                                                if (cResp.AmountOff.HasValue && cResp.Currency == p.Price.Currency)
                                                {
                                                    p.PromotionalPrice = Math.Round(p.Price.Amount.Value - cResp.AmountOff.Value, 2);
                                                    if (p.PromotionalPrice < 0)
                                                    {
                                                        p.PromotionalPrice = 0;
                                                    }

                                                    p.PromotionalAmountPerMonth = p.Price.PeriodType == "year" ? Math.Round(p.PromotionalPrice.Value / 12m, 2) : p.PromotionalPrice.Value;
                                                }
                                                else if (cResp.PercentOff.HasValue)
                                                {
                                                    p.PromotionalPrice = Math.Round(p.Price.Amount.Value - (p.Price.Amount.Value * (cResp.PercentOff.Value / 100m)), 2);

                                                    p.PromotionalAmountPerMonth = p.Price.PeriodType == "year" ? Math.Round(p.PromotionalPrice.Value / 12m, 2) : p.PromotionalPrice.Value;
                                                }
                                            }
                                            p.Coupon = cResp;
                                        }
                                    }
                                }
                            }

                           
                        }

                        return plans;
                    }
                }
                else
                {
                    var bPlans = await ctx.BillingPlans.Where(t => t.IsActive && t.IsPublic).Include(p => p.Price).ToListAsync();
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId.Value && x.Status != UserAccountStatus.DELETED);
                    if (user != null && !string.IsNullOrEmpty(user.CouponId) && (!user.CouponValidDate.HasValue || (user.CouponValidDate.HasValue && user.CouponValidDate.Value> DateTime.Now)))
                    {
                        var promoC= await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == user.CouponId);
                        if(promoC != null)
                        {
                            var cResp = new CouponResponse();
                            cResp.Id = promoC.Id;
                            cResp.Name = promoC.Name;
                            cResp.PercentOff = promoC.PercentOff;
                            cResp.AmountOff = promoC.AmountOff;
                            cResp.Currency = promoC.Currency;
                            cResp.Duration = promoC.Duration;
                            cResp.DurationInMonths = promoC.DurationInMonths;
                            

                            foreach(var p in bPlans)
                            {
                                if (p.Price.Amount.HasValue)
                                {
                                    p.PromotionalPrice = p.Price.Amount;
                                    if (cResp.AmountOff.HasValue && cResp.Currency == p.Price.Currency)
                                    {
                                        p.PromotionalPrice = Math.Round(p.Price.Amount.Value - cResp.AmountOff.Value, 2);
                                        if(p.PromotionalPrice <0)
                                        {
                                            p.PromotionalPrice = 0;
                                        }

                                        p.PromotionalAmountPerMonth = p.Price.PeriodType == "year" ? Math.Round(p.PromotionalPrice.Value / 12m, 2) : p.PromotionalPrice.Value;
                                    }
                                    else if(cResp.PercentOff.HasValue)
                                    {
                                        p.PromotionalPrice = Math.Round(p.Price.Amount.Value - (p.Price.Amount.Value * (cResp.PercentOff.Value / 100m)),2);

                                        p.PromotionalAmountPerMonth = p.Price.PeriodType == "year" ? Math.Round(p.PromotionalPrice.Value / 12m, 2) : p.PromotionalPrice.Value;
                                    }
                                }
                                p.Coupon = cResp;
                            }
                        }
                    }

                    if(user.AllowHiddenProducts)
                    {
                        var hiddenPlans = await ctx.BillingPlans.Where(t => t.IsActive && !t.IsPublic).Include(p => p.Price).ToListAsync();
                        if (hiddenPlans.Any())
                        {
                            bPlans = bPlans.Union(hiddenPlans).ToList();
                        }
                    }
                    

                    return bPlans;
                }
            } 
        }

        public async Task<UserBillingPlan> SelectUserSubscriptionPlan(int subscriptionId, int userId)
        {
            using (var ctx = new DataContext())
            {
                if(await ctx.UserBillingPlans.AnyAsync(t => t.UserId == userId && (t.Status != BillingPlanStatus.CANCELLED || t.Status!=BillingPlanStatus.DELETED)))
                {
                    throw new CoachOnlineException("User subscription already exist.", CoachOnlineExceptionState.AlreadyExist);
                }

                var subscription = await ctx.BillingPlans.FirstOrDefaultAsync(t => t.Id == subscriptionId);
                if (subscription == null)
                {
                    throw new CoachOnlineException("Selected subscription does not exist.", CoachOnlineExceptionState.NotExist);
                }

                UserBillingPlan subscriptionPlan = new UserBillingPlan() { BillingPlanTypeId = subscriptionId, UserId = userId, CreationDate = DateTime.Now, Status = BillingPlanStatus.PENDING };

                subscriptionPlan.BillingPlanType = subscription;
               

                if (subscription.BillingOption == BillingPlanOption.STUDENT)
                {
                    subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.AWAITING_STUDENT_CARD;
                    subscriptionPlan.IsStudent = true;
                    Console.WriteLine("Is student option");
                }
                else
                {
                    subscriptionPlan.IsStudent = false;
                    subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.CANCELLED;
                    Console.WriteLine("Not a student option");

                }

                subscriptionPlan.StripePriceId = subscription.StripePriceId;
                subscriptionPlan.StripeProductId = subscription.StripeProductId;

                ctx.UserBillingPlans.Add(subscriptionPlan);
                await ctx.SaveChangesAsync();

                return subscriptionPlan;
            }
        }

        public async Task DeleteSubscriptionPlan(int userId, int subscriptionId)
        {
            using(var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == subscriptionId);
                plan.CheckExist("Plan");
                if (!(plan.Status == BillingPlanStatus.PENDING || plan.Status == BillingPlanStatus.AWAITING_ACTIVATION))
                {
                    throw new CoachOnlineException($"User subscription plan cannot be deleted because is in wrong state. Current state is:{plan.StatusStr}", CoachOnlineExceptionState.CantChange);
                }

                if(plan.Status == BillingPlanStatus.PENDING && !string.IsNullOrEmpty(plan.StripeSubscriptionId))
                {
                    
                    var svc = new Stripe.SubscriptionService();
                    var sub = await svc.GetAsync(plan.StripeSubscriptionId);

                    SetSubscriptionState(ref plan, sub);

                    await ctx.SaveChangesAsync();

                    if(plan.Status != BillingPlanStatus.PAYMENT_REJECTED)
                    {
                        throw new CoachOnlineException("Subscription must be cancelled", CoachOnlineExceptionState.CantChange);
                    }


                }
               

                if(!string.IsNullOrEmpty(plan.StripeSubscriptionScheduleId))
                {
                    throw new CoachOnlineException("Subscription schedule must be cancelled", CoachOnlineExceptionState.CantChange);
                }

                plan.Status = BillingPlanStatus.DELETED;

                await ctx.SaveChangesAsync();
                
            }
        }

        public async Task CancelNotPaidSubscription(int userId, int subscriptionId)
        {
            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == subscriptionId);
                plan.CheckExist("Plan");
                if (plan.Status != BillingPlanStatus.AWAITING_PAYMENT)
                {
                    throw new CoachOnlineException($"User subscription plan cannot be cancelled because is in wrong state. Current state is:{plan.StatusStr}", CoachOnlineExceptionState.CantChange);
                }

                if(string.IsNullOrEmpty(plan.StripeSubscriptionId))
                {
                    await DeleteSubscriptionPlan(userId, subscriptionId);
                }

                var svc = new Stripe.SubscriptionService();

                await svc.CancelAsync(plan.StripeSubscriptionId);

                plan.Status = BillingPlanStatus.CANCELLED;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task CancelScheduledSubscription(int userId, int subscriptionId)
        {
            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == subscriptionId);
                plan.CheckExist("Plan");
                if (plan.Status != BillingPlanStatus.AWAITING_ACTIVATION)
                {
                    throw new CoachOnlineException($"User subscription plan cannot be deleted because is in wrong state. Current state is:{plan.StatusStr}", CoachOnlineExceptionState.CantChange);
                }

                if (!string.IsNullOrEmpty(plan.StripeSubscriptionId))
                {
                    throw new CoachOnlineException("Subscription must be cancelled", CoachOnlineExceptionState.CantChange);
                }


                if (string.IsNullOrEmpty(plan.StripeSubscriptionScheduleId) && string.IsNullOrEmpty(plan.StripeSubscriptionId))
                {

                    await DeleteSubscriptionPlan(plan.UserId, plan.Id);

                    return;
                    //throw new CoachOnlineException("Subscription schedule does not exist", CoachOnlineExceptionState.NotExist);
                }

                var scheduleSubSvc = new SubscriptionScheduleService();
                var schedule = await scheduleSubSvc.GetAsync(plan.StripeSubscriptionScheduleId);
                
                if(schedule.Status == "not_started")
                {
                    await scheduleSubSvc.CancelAsync(schedule.Id);
                }


                
                plan.Status = BillingPlanStatus.CANCELLED;

                await ctx.SaveChangesAsync();

            }
        }



        public async Task CreateUserStripeCustomerAccount(User u)
        {           

            if(!string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User already has a Stripe customer account.", CoachOnlineExceptionState.AlreadyExist); 
            }

            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == u.Id);
                user.CheckExist("User");

                CustomerService cs = new CustomerService();
                var allCustomers = await cs.ListAsync();

                var existingCustomer = allCustomers.FirstOrDefault(t => t.Email == u.EmailAddress);
                if(existingCustomer != null)
                {
                    user.StripeCustomerId = existingCustomer.Id;
                    await ctx.SaveChangesAsync();
                    return;
                }

                CustomerCreateOptions customer = new CustomerCreateOptions();
                customer.Email = u.EmailAddress;
                if (u.FirstName != null || u.Surname != null)
                {
                    customer.Name = $"{u.FirstName?.ToString()} {u.Surname?.ToString()}".Trim();
                }
                
                var result = await cs.CreateAsync(customer);
                user.StripeCustomerId = result.Id;
                await ctx.SaveChangesAsync();
            }
            
        }

        public async Task AddUserSubscription(User u)
        {
            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User does not have a Stripe customer account.", CoachOnlineExceptionState.NotExist);
            }
            if(u.FirstName == null || u.Surname == null)
            {
                throw new CoachOnlineException("Please enter user name and surname", CoachOnlineExceptionState.DataNotValid);
            }
            var userBillingPlan = await GetUserCurrentSubscriptionPlan(u.Id);
            if(userBillingPlan == null)
            {
                throw new CoachOnlineException("Select subscription type first", CoachOnlineExceptionState.NotExist);
            }
            if(userBillingPlan.Status != BillingPlanStatus.PENDING)
            {
                throw new CoachOnlineException("The subscription is in a wrong state. Cannot proceed.", CoachOnlineExceptionState.AlreadyExist);
            }

            if (userBillingPlan.IsStudent == true && userBillingPlan.StudentCardVerificationStatus != StudentCardStatus.ACCEPTED)
            {
                throw new CoachOnlineException("The subscription is in a wrong state. Student card is not accepted. Cannot proceed.", CoachOnlineExceptionState.DataNotValid);
            }
            if (userBillingPlan.BillingPlanType.BillingOption == BillingPlanOption.STUDENT && userBillingPlan.StudentCardVerificationStatus != StudentCardStatus.ACCEPTED)
            {
                throw new CoachOnlineException("The subscription is in a wrong state. Student card is not accepted. Cannot proceed.", CoachOnlineExceptionState.DataNotValid);
            }
            if (!string.IsNullOrEmpty(userBillingPlan.StripeSubscriptionId))
            {
                throw new CoachOnlineException("User subscription for seletec plan already exists. Cannot proceed.", CoachOnlineExceptionState.AlreadyExist);
            }

            if (!(await IsCustomerDefaultPaymentACard(u)))
            {
                throw new CoachOnlineException("Customer does not have default card payment method created.", CoachOnlineExceptionState.DataNotValid);
            }

            var options = new SubscriptionCreateOptions
            {
                Customer = u.StripeCustomerId,
                OffSession = true,
                CollectionMethod = "charge_automatically",
                Metadata = new Dictionary<string, string>
                        {
                            {UserBillingPlanIdKey,userBillingPlan.Id.ToString()}
                        },
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = userBillingPlan.StripePriceId
                    },
                },
            };
            var service = new Stripe.SubscriptionService();
            var subscription = await service.CreateAsync(options);


            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == userBillingPlan.Id);
                plan.CheckExist("Plan");
                plan.StripeSubscriptionId = subscription.Id;

                SetSubscriptionState(ref plan, subscription);
               
                await ctx.SaveChangesAsync();

                

            }

        }

        public async Task SetCustomerDefaultSource(User u ,string payment_method_id)
        {
            try
            {
                u.CheckExist("User");
                if (string.IsNullOrEmpty(u.StripeCustomerId))
                {
                    throw new CoachOnlineException("User does not have a customer account", CoachOnlineExceptionState.NotExist);
                }
                // CustomerUpdateOptions opts = new CustomerUpdateOptions();
                // opts.DefaultSource = payment_method_id;

                var options = new PaymentMethodListOptions
                {
                    Customer = u.StripeCustomerId,
                    Type = "card",
                };

                if (string.IsNullOrEmpty(payment_method_id))
                {
                    var payService = new PaymentMethodService();
                    var paymentMethods = await payService.ListAsync(options);

                    if (paymentMethods == null || paymentMethods.Count() ==0)
                    {
                        throw new CoachOnlineException("User does not have any debit/credit card added. Cannot set default source.", CoachOnlineExceptionState.NotExist);
                    }
                    var pmLastCreated = paymentMethods.OrderByDescending(t => t.Created).FirstOrDefault();
                    
                    if(pmLastCreated != null)
                    {
                        payment_method_id = pmLastCreated.Id;
                    }
                    else
                    {
                        throw new CoachOnlineException("User does not have any debit/credit card added. Cannot set default source.", CoachOnlineExceptionState.NotExist);
                    }
                }
                

                CustomerInvoiceSettingsOptions sett = new CustomerInvoiceSettingsOptions();
                sett.DefaultPaymentMethod = payment_method_id;
                CustomerUpdateOptions opts = new CustomerUpdateOptions();
                opts.InvoiceSettings = sett;


             
               // await svc.UpdateAsync(u.StripeCustomerId, opts);

               CustomerService svc = new CustomerService();
                
               var result = await svc.UpdateAsync(u.StripeCustomerId, opts);
            }
            catch(Exception ex)
            {
                throw new CoachOnlineException(ex.Message + " " + ex.ToString(), CoachOnlineExceptionState.DataNotValid);
            }
        }

        public async Task<ClientSecretResponse> CreateSetupIntent(User u)
        {
            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User doesnt have stripe customer account", CoachOnlineExceptionState.NotExist);
            }

            var options = new SetupIntentCreateOptions
            {
                Customer = u.StripeCustomerId,
            };
            var service = new SetupIntentService();
            var intent = await service.CreateAsync(options);

            return new ClientSecretResponse() { ClientSecret = intent.ClientSecret, Type = "SetupIntent" };
        }

        public async Task<ClientSecretResponse> GetPaymentIntentForSub(int userId, int userSubId)
        {
            var user = await _userSvc.GetUserById(userId);
            using (var ctx = new DataContext())
            {
              
                var billingPlan = await ctx.UserBillingPlans.Where(x => x.UserId == userId && x.Id == userSubId).FirstOrDefaultAsync();
                billingPlan.CheckExist("User subscription");

                if(string.IsNullOrEmpty(billingPlan.StripeSubscriptionId))
                {
                    return await CreateSetupIntent(user);
                }

                var subSvc = new Stripe.SubscriptionService();
                var sub = await subSvc.GetAsync(billingPlan.StripeSubscriptionId);

                if(sub.LatestInvoiceId == null)
                {
                    Console.WriteLine("Latest invoice id is null");
                }

                var invoice = await GetInvoiceById(sub.LatestInvoiceId);

                if (invoice != null)
                {
                    if (invoice.Status == "paid")
                    {
                        Console.WriteLine($"Invoice {sub.LatestInvoiceId} is paid");
                        return null;
                    }

                    if(invoice.Status == "draft")
                    {
                        var invSvc = new Stripe.InvoiceService();
                        invoice = await invSvc.FinalizeInvoiceAsync(invoice.Id);
                    }

                    Console.WriteLine($"Invoice id: {sub.LatestInvoiceId}");
                    Console.WriteLine($"Invoice status: {invoice.Status}");



                    Console.WriteLine($"Invoice payment intent is: {invoice.PaymentIntentId}");


                    var payIntent = await GetPaymentIntentById(invoice.PaymentIntentId);
                    if(payIntent == null)
                    {
                        Console.WriteLine("PaymentIntent is null");
                        return await CreateSetupIntent(user);
                    }
                    //var opts = new PaymentIntentCreateOptions
                    //{
                    //    Customer = u.StripeCustomerId,
                    //    Amount = 0,
                    //    Currency = "pln",
                    //    SetupFutureUsage = ""
                    //};
                    ////var options = new SetupIntentCreateOptions
                    ////{
                    ////    Customer = u.StripeCustomerId,
                    ////};

                    //var service = new PaymentIntentService();

                    //var intent = await service.CreateAsync(opts);
                    return new ClientSecretResponse() { ClientSecret = payIntent.ClientSecret, Type="PaymentIntent" };
                }
                else
                {
                    return await CreateSetupIntent(user);
                }
            }
        }

        public async Task<List<InvoiceResponse>> GetUserInvoices(User u)
        {
            InvoiceService invSvc = new InvoiceService();
            var invoices = await invSvc.ListAsync(new InvoiceListOptions() { Customer = u.StripeCustomerId });
            var userInvoices = invoices.Where(t => t.CustomerId == u.StripeCustomerId).ToList();

            var response = new List<InvoiceResponse>();
            userInvoices = userInvoices.OrderByDescending(t => t.Created).ToList();

            foreach (var el in userInvoices)
            {
                InvoiceLineItem lineItm = null;
                if (el.Lines.Data.Count > 0)
                {
                    lineItm = el.Lines.Data[0];
                }
                var paymentIntent = await GetPaymentIntentById(el.PaymentIntentId);
                string last4Digits = "";
                if (paymentIntent != null && paymentIntent.Charges.Data.Count > 0 && paymentIntent.Charges.Data[0].PaymentMethodDetails?.Card != null)
                {
                    last4Digits = paymentIntent.Charges.Data[0].PaymentMethodDetails.Card.Last4;
                }

                var inv = new InvoiceResponse()
                {

                    InvoiceDate = el.Created,
                    InvoiceStripeId = el.Id,
                    PeriodStart = lineItm!= null ? ConvertTime.FromUnixTimestamp((int)lineItm.Period.Start): el.Created,
                    PeriodEnd = lineItm != null ? (DateTime?)ConvertTime.FromUnixTimestamp((int)lineItm.Period.End) : null,
                    //lineItm == null ? null : CalculateBillingPlanExpirationDate(el.PeriodStart, 1, lineItm.Plan.Interval),
                    Total = ((decimal)el.Total) / 100,
                    Subtotal = ((decimal)el.Subtotal) / 100,
                    Description = lineItm == null ? null : lineItm.Description,
                    Currency = el.Currency,
                    CardLast4Digits = last4Digits,
                    InvoicePdf = el.InvoicePdf,
                    Tax = el.Tax ?? 0
                };
                response.Add(inv);
            }

            return response;
        }

        public async Task<InvoiceHeaderResponse> GetSubscriptionInvoices(User u)
        {

            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User does not have a Stripe customer account.", CoachOnlineExceptionState.NotExist);
            }
            //var userBillingPlan = await GetUserCurrentSubscriptionPlan(u.Id);
            var userBillingPlan = await GetUserActiveSubscriptionPlan(u.Id);

            var currentBillingBlan = await GetUserCurrentSubscriptionPlan(u.Id);
            if (userBillingPlan == null && currentBillingBlan == null)
            {


                var invoices = await GetUserInvoices(u);
                return new InvoiceHeaderResponse()
                {
                    Name = u.FirstName,
                    Surname = u.Surname,
                    Invoices = invoices
                };
            }

      

            if(string.IsNullOrEmpty(userBillingPlan.StripeSubscriptionId) && string.IsNullOrEmpty(userBillingPlan.StripeSubscriptionScheduleId))
            {
                throw new CoachOnlineException("User has not subscribed to a plan yet", CoachOnlineExceptionState.NotExist);
            }
            InvoiceHeaderResponse header = new InvoiceHeaderResponse();
            header.Name = u.FirstName;
            header.Surname = u.Surname;

            //InvoiceService invSvc = new InvoiceService();
            //var invoices = await invSvc.ListAsync(new InvoiceListOptions() { Customer = u.StripeCustomerId });
            //var userInvoices = invoices.Where(t => t.CustomerId == u.StripeCustomerId).ToList();

            if (currentBillingBlan != null && currentBillingBlan.StripeSubscriptionScheduleId != null && currentBillingBlan.Status == BillingPlanStatus.AWAITING_ACTIVATION)
            {
                var schedule = await GetCustomerStripeSubscriptionSchedule(currentBillingBlan.StripeSubscriptionScheduleId);
                header.SubscriptionName = currentBillingBlan.BillingPlanType.Name;
                header.SubscriptionPrice = currentBillingBlan.BillingPlanType.Price.Amount.GetValueOrDefault();
                header.SubscriptionPeriod = currentBillingBlan.BillingPlanType.Price.Period.HasValue && currentBillingBlan.BillingPlanType.Price.Period.Value > 1 ? currentBillingBlan.BillingPlanType.Price.Period.Value.ToString() +" "+ currentBillingBlan.BillingPlanType.Price.PeriodType +"s": currentBillingBlan.BillingPlanType.Price.PeriodType;
               var phase = schedule.Phases[0];
                header.NextBillingTime = phase.StartDate.Date;
            }
            else
            {
                var stripeSubscription = await GetCustomerStripeSubscription(userBillingPlan.StripeSubscriptionId);


                header.Name = u.FirstName;
                header.Surname = u.Surname;
                header.SubscriptionName = currentBillingBlan.BillingPlanType.Name;

                if (stripeSubscription.Items.Data.Count > 0)
                {
                    var plan = stripeSubscription.Items.Data[0].Plan;
                    header.SubscriptionPeriod = plan.Interval;
                    header.SubscriptionPrice = plan.AmountDecimal.HasValue ? plan.AmountDecimal.Value / 100 : 0;
                    header.NextBillingTime = CalculateBillingPlanExpirationDate(stripeSubscription.CurrentPeriodStart, 1, plan.Interval);
                }
            }

            header.Invoices = await GetUserInvoices(u);

            return header;
        }

        public async Task<bool> IsCustomerDefaultPaymentACard(User u)
        {

            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User doesn't have Stripe customer account.", CoachOnlineExceptionState.NotExist);
            }
            var customer = await GetStripeCustomer(u.StripeCustomerId);

            if(string.IsNullOrEmpty(customer.InvoiceSettings.DefaultPaymentMethodId))
            {
                throw new CoachOnlineException("User doesn't have default payment method set.", CoachOnlineExceptionState.NotExist);
            }

            var paymentMethod = await GetCustomerStripePaymentMethod(customer.InvoiceSettings.DefaultPaymentMethodId);

            if(paymentMethod == null)
            {
                throw new CoachOnlineException("Customer does not have default payment method", CoachOnlineExceptionState.NotExist);
            }

            if (paymentMethod.Type == "card")
                return true;

            return false;

        }

        public async Task<PaymentMethodResponse> GetCustomerDefaultPaymentMethod(User u)
        {

            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User doesn't have Stripe customer account.", CoachOnlineExceptionState.NotExist);
            }
            var customer = await GetStripeCustomer(u.StripeCustomerId);
            var cardInfo = await GetCustomerStripePaymentMethod(customer.InvoiceSettings.DefaultPaymentMethodId);

            PaymentMethodResponse resp = new PaymentMethodResponse();
            resp.StripePaymentMethodId = cardInfo.Id;
            resp.StripeCustomerId = u.StripeCustomerId;
            if(cardInfo.BillingDetails != null)
            {
                BillingDetailsResponse b = new BillingDetailsResponse();
                b.Email = cardInfo.BillingDetails.Email;
                b.Name = cardInfo.BillingDetails.Name;
                b.Street = cardInfo.BillingDetails.Address?.Line1;
                b.Street2 = cardInfo.BillingDetails.Address?.Line2;
                b.City = cardInfo.BillingDetails.Address.City;
                b.Country = cardInfo.BillingDetails.Address.Country;
                b.PostalCode = cardInfo.BillingDetails.Address.PostalCode;

                resp.BillingDetails = b;
            }

            if (cardInfo.Card != null)
            {
                CardResponse c = new CardResponse();
                c.Brand = cardInfo.Card.Brand;
                c.Last4Digits = cardInfo.Card.Last4;
                c.ValidTo = $"{cardInfo.Card.ExpMonth}/{cardInfo.Card.ExpYear}";
                c.Country = cardInfo.Card.Country;
                resp.Card = c;
            }
            return resp;
        }

        private async Task AddCancellationReason(int userId, int subscriptionId, int? cancellationResponse)
        {
            using (var ctx = new DataContext())
            {
                var questionaaire = await ctx.Questionnaires.FirstOrDefaultAsync(x => x.QuestionnaireType == QuestionnaireType.CancelSub);
                if (questionaaire != null && cancellationResponse.HasValue)
                {
                    var answer = await ctx.QuestionnaireAnswers.Where(x => x.QuestionnaireId == questionaaire.Id)
                        .Where(x => x.UserId == userId && x.Id == cancellationResponse.Value).FirstOrDefaultAsync();

                    if (answer != null)
                    {
                        var sub = await ctx.UserBillingPlans.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == subscriptionId);

                        if (sub != null)
                        {
                            sub.QuestionaaireCancelReason = cancellationResponse.Value;

                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        public async Task<CancelSubscriptionResponse> CancelSubscription(User u, int? cancelSubResp = null)
        {
            CancelSubscriptionResponse resp = new CancelSubscriptionResponse();
            resp.ActiveSubscriptionNotExist = false;

            if(string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("Cannot cancel because user is not a stripe customer.", CoachOnlineExceptionState.NotExist);
            }
            var currentBillingPlan = await GetUserCurrentSubscriptionPlan(u.Id);
            if(currentBillingPlan == null)
            {
                Console.WriteLine("No current billing plan");
                resp.IsOkState = true;
                resp.ActiveSubscriptionNotExist = true;
                resp.CancellationDate = DateTime.Now;
                return resp;
            }
            if(currentBillingPlan.Status == BillingPlanStatus.PENDING)
            {
                Console.WriteLine("Billing plan pending");
                await DeleteSubscriptionPlan(u.Id, currentBillingPlan.Id);
                resp.IsOkState = true;
                resp.ActiveSubscriptionNotExist = true;
                resp.CancellationDate = DateTime.Now;
                await AddCancellationReason(u.Id, currentBillingPlan.Id, cancelSubResp);
                return resp;
            }
            if(currentBillingPlan.Status == BillingPlanStatus.AWAITING_PAYMENT)
            {
                Console.WriteLine("Billing plan awaiting payment");
                await CancelNotPaidSubscription(u.Id, currentBillingPlan.Id);
                resp.IsOkState = true;
                resp.ActiveSubscriptionNotExist = true;
                resp.CancellationDate = DateTime.Now;
                await AddCancellationReason(u.Id, currentBillingPlan.Id, cancelSubResp);
                return resp;
            }
            if(currentBillingPlan.Status == BillingPlanStatus.AWAITING_ACTIVATION)
            {
                Console.WriteLine("Billing plan awaiting activation");
                await CancelScheduledSubscription(u.Id, currentBillingPlan.Id);
                resp.IsOkState = true;
                resp.ActiveSubscriptionNotExist = false;
                resp.CancellationDate = currentBillingPlan.PlannedActivationDate.HasValue? currentBillingPlan.PlannedActivationDate.Value:DateTime.Now;
                Console.WriteLine($"Activation date is set on {resp.CancellationDate.ToString()}");
                await AddCancellationReason(u.Id, currentBillingPlan.Id, cancelSubResp);
                return resp;
            }
            else if (currentBillingPlan.Status == BillingPlanStatus.ACTIVE)
            {
                Console.WriteLine("Billing plan active");
                var service = new Stripe.SubscriptionService();
                var checkSubscription = await service.GetAsync(currentBillingPlan.StripeSubscriptionId);
                
                if (checkSubscription.Status == "canceled")
                {
                    resp.IsOkState = true;
                    
                    resp.ActiveSubscriptionNotExist = checkSubscription.CancelAtPeriodEnd && checkSubscription.CurrentPeriodEnd <= DateTime.Now ? false : true; 
                    resp.CancellationDate = checkSubscription.CancelAtPeriodEnd ? checkSubscription.CurrentPeriodEnd : DateTime.Now;
                    await AddCancellationReason(u.Id, currentBillingPlan.Id, cancelSubResp);
                    Console.WriteLine($"Billing plan is cancelled and cancellation date is on {resp.CancellationDate.ToString()}");
                    return resp;
                }

                if(checkSubscription.ScheduleId != null)
                {
                    var scheduleSvc = new SubscriptionScheduleService();

                    await scheduleSvc.ReleaseAsync(checkSubscription.ScheduleId);
       
                    checkSubscription = await service.GetAsync(currentBillingPlan.StripeSubscriptionId);
                    Console.WriteLine("Status after schedule release"+ checkSubscription.Status);


                }

                if (!checkSubscription.CancelAt.HasValue)
                {

                   
                    var opts = new SubscriptionUpdateOptions()
                    {
                        CancelAtPeriodEnd = true,

                    };

                    var subscription = await service.UpdateAsync(currentBillingPlan.StripeSubscriptionId, opts);
                    resp.CancellationDate = subscription.CurrentPeriodEnd;
                    resp.IsOkState = true;
                    resp.ActiveSubscriptionNotExist = DateTime.Now >= subscription.CurrentPeriodEnd ? true : false;

                    Console.WriteLine($"Billing plan is about to be cancelled on {resp.CancellationDate.ToString()} and active sub status not exist is {resp.ActiveSubscriptionNotExist.ToString()}");

                    using (var ctx = new DataContext())
                    {
                        var userBillingPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == currentBillingPlan.Id && t.UserId == u.Id);
                        if (userBillingPlan != null)
                        {
                           
                            SetSubscriptionState(ref userBillingPlan, subscription);
                            await ctx.SaveChangesAsync();

                            await AddCancellationReason(u.Id, userBillingPlan.Id, cancelSubResp);
                        }
                    }

                    return resp;
                }
                else if (checkSubscription.CancelAt.HasValue)
                {
                    resp.CancellationDate = checkSubscription.CancelAt.Value;
                    resp.IsOkState = true;

                    Console.WriteLine($"Billing plan has cancel value {resp.CancellationDate.ToString()} and active sub status not exist is {resp.ActiveSubscriptionNotExist.ToString()}");

                    using (var ctx = new DataContext())
                    {
                        var userBillingPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == currentBillingPlan.Id && t.UserId == u.Id);
                        if (userBillingPlan != null)
                        {
                           
                            SetSubscriptionState(ref userBillingPlan, checkSubscription);
                            await ctx.SaveChangesAsync();
                            await AddCancellationReason(u.Id, userBillingPlan.Id, cancelSubResp);
                        }
                    }

                    return resp;
                }
            }

            return resp;

        }

        public async Task<List<SubCancellationReasonResponse>> GetSubscriptionCancellationReasons()
        {
            List<SubCancellationReasonResponse> data = new List<SubCancellationReasonResponse>();
            using(var ctx = new DataContext())
            {
                var cancelledSubs = await ctx.UserBillingPlans.Where(x => x.QuestionaaireCancelReason.HasValue).Include(b=>b.BillingPlanType).Include(u=>u.User).ToListAsync();

                foreach(var sub in cancelledSubs)
                {
                    var reason = await ctx.QuestionnaireAnswers.Where(x => x.Id == sub.QuestionaaireCancelReason.Value).Include(a=>a.Response).FirstOrDefaultAsync();
                    if(reason!= null)
                    {
                        SubCancellationReasonResponse resp = new SubCancellationReasonResponse();
                        resp.UserId = sub.UserId;
                        resp.FirstName = sub.User.FirstName;
                        resp.LastName = sub.User.Surname;
                        resp.Email = sub.User.EmailAddress;
                        resp.SubscriptionId = sub.Id;
                        resp.Reason = reason.Response.IsOtherOption ? reason.OtherResponse : reason.Response.Option;
                        resp.PlanName = sub.BillingPlanType.Name;
                        resp.ExpiryDate = sub.ExpiryDate;
                        resp.CurrentStatus = sub.StatusStr;

                        data.Add(resp);
                    }
                }

                return data;

            }
        }

        public async Task<ChangeSubscriptionResponse> EnableStudentSubscriptionAfterStudentCardAccept(User u, int userSubscriptionPlanId)
        {
            if (string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User does not have a stripe customer account.", CoachOnlineExceptionState.NotExist);
            }

            await UpdateSubscriptionStatesFromStripeForUser(u);

            var isCardPayment = await IsCustomerDefaultPaymentACard(u);

            if (!isCardPayment)
            {
                throw new CoachOnlineException("Card payment method for user is not set.", CoachOnlineExceptionState.NotExist);
            }

            ChangeSubscriptionResponse changeResp = new ChangeSubscriptionResponse();
            //cancel current at period end
            var resp = await CancelSubscription(u);

            if (!resp.IsOkState)
            {
                throw new CoachOnlineException("Cancellation of current active subscription went wrong", CoachOnlineExceptionState.UNKNOWN);
            }

            //start new subscription plan
            using (var ctx = new DataContext())
            {

                UserBillingPlan subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(t => t.Id == userSubscriptionPlanId && t.IsStudent && t.StudentCardVerificationStatus == StudentCardStatus.ACCEPTED);
                subscriptionPlan.CheckExist("Plan");

                changeResp.UserSubscriptionId = subscriptionPlan.Id;
                changeResp.ValidFrom = resp.CancellationDate;
                //add_subscription to stripe

                if (resp.ActiveSubscriptionNotExist)
                {
                    Console.WriteLine("No active sub, Activating payment");
                    subscriptionPlan.Status = BillingPlanStatus.PENDING;
                    //create subscription
                    var options = new SubscriptionCreateOptions
                    {
                        Customer = u.StripeCustomerId,
                        OffSession = true,
                        CollectionMethod = "charge_automatically",
                        //Coupon = subscriptionPlan.CouponId,
                        Metadata = new Dictionary<string, string>
                        {
                            {UserBillingPlanIdKey,subscriptionPlan.Id.ToString()}
                        },
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Price = subscriptionPlan.StripePriceId
                            },
                        },
                    };
                    var service = new Stripe.SubscriptionService();
                    var newSubscription = await service.CreateAsync(options);
                    subscriptionPlan.StripeSubscriptionId = newSubscription.Id;
                    SetSubscriptionState(ref subscriptionPlan, newSubscription);

                    await ctx.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"Scheduling payment on {resp.CancellationDate.ToString()}");
                    //create schedule
                    var options = new SubscriptionScheduleCreateOptions
                    {
                        Customer = u.StripeCustomerId,
                        StartDate = resp.CancellationDate,
                        EndBehavior = "release",
                        
                        Metadata = new Dictionary<string, string>
                        {
                            {UserBillingPlanIdKey,subscriptionPlan.Id.ToString()}
                        },
                        Phases = new List<SubscriptionSchedulePhaseOptions>
                    {
                        new SubscriptionSchedulePhaseOptions
                        {
                            Items = new List<SubscriptionSchedulePhaseItemOptions>
                            {
                                new SubscriptionSchedulePhaseItemOptions
                                {
                                    Price = subscriptionPlan.StripePriceId
                                },
                                
                            },
                            //Coupon = subscriptionPlan.CouponId

                        },
                    },
                    };
                    var service = new SubscriptionScheduleService();
                    var subSchedule = await service.CreateAsync(options);
                    subscriptionPlan.StripeSubscriptionScheduleId = subSchedule.Id;
                    subscriptionPlan.StripeSubscriptionId = subSchedule.SubscriptionId;
                    subscriptionPlan.Status = BillingPlanStatus.AWAITING_ACTIVATION;
                    await ctx.SaveChangesAsync();
                }
            }


            return changeResp;
        }

        public async Task<ChangeSubscriptionResponse> ChangeSubscription(User u, int newSubscriptionId)
        {
            if(string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User does not have a stripe customer account.", CoachOnlineExceptionState.NotExist);
            }

            await UpdateSubscriptionStatesFromStripeForUser(u);

            var isCardPayment = await IsCustomerDefaultPaymentACard(u);

            if (!isCardPayment)
            {
                throw new CoachOnlineException("Card payment method for user is not set.", CoachOnlineExceptionState.NotExist);
            }

            ChangeSubscriptionResponse changeResp = new ChangeSubscriptionResponse();
            //cancel current at period end
            var resp = await CancelSubscription(u);

            if(!resp.IsOkState)
            {
                throw new CoachOnlineException("Cancellation of current active subscription went wrong", CoachOnlineExceptionState.UNKNOWN);
            }

            //start new subscription plan
            using(var ctx = new DataContext())
            {
                var subscription = await ctx.BillingPlans.Where(t => t.Id == newSubscriptionId).Include(p=>p.Price).FirstOrDefaultAsync();
                if (subscription == null)
                {
                    throw new CoachOnlineException("Selected subscription does not exist.", CoachOnlineExceptionState.NotExist);
                }

    
                //add_subscription to stripe

                if (resp.ActiveSubscriptionNotExist)
                {
                    UserBillingPlan subscriptionPlan = new UserBillingPlan() { BillingPlanTypeId = newSubscriptionId, UserId = u.Id, CreationDate = DateTime.Now };
                    subscriptionPlan.Status = BillingPlanStatus.PENDING;
                    subscriptionPlan.PlannedActivationDate = resp.CancellationDate;
                    if (subscription.BillingOption == BillingPlanOption.STUDENT)
                    {

                        subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.AWAITING_STUDENT_CARD;
                        subscriptionPlan.IsStudent = true;
                    }
                    else
                    {
                        subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.CANCELLED;
                        subscriptionPlan.IsStudent = false;
                    }

                    subscriptionPlan.StripePriceId = subscription.StripePriceId;
                    subscriptionPlan.StripeProductId = subscription.StripeProductId;

                    ctx.UserBillingPlans.Add(subscriptionPlan);
                    await ctx.SaveChangesAsync();
                    changeResp.UserSubscriptionId = subscriptionPlan.Id;
                    changeResp.ValidFrom = resp.CancellationDate;

                    if (subscriptionPlan.IsStudent && subscriptionPlan.StudentCardVerificationStatus != StudentCardStatus.ACCEPTED)
                    {
                        return changeResp;
                    }

                    var isAnotherSub = await ctx.UserBillingPlans.Where(x => x.UserId == u.Id && x.StripeSubscriptionId != null).AnyAsync();
                    //create subscription
                    var options = new SubscriptionCreateOptions
                    {
                        Customer = u.StripeCustomerId,
                        OffSession = true,
                        
                        PaymentBehavior =  isCardPayment? "allow_incomplete" : "default_incomplete",
                        CollectionMethod = "charge_automatically",
                       
                        TrialPeriodDays = isAnotherSub == false && subscription.Price.TrialDays> 0 ? (int?)subscription.Price.TrialDays:null,
                        Metadata = new Dictionary<string, string>
                        {
                            {UserBillingPlanIdKey,subscriptionPlan.Id.ToString()}
                        },
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Price = subscriptionPlan.StripePriceId
                            },
                        },
                    };

                    if(!string.IsNullOrEmpty(u.CouponId) && (!u.CouponValidDate.HasValue || (u.CouponValidDate.HasValue && u.CouponValidDate.Value> DateTime.Now) ))
                    {
                        if(await CheckCouponExists(u.CouponId))
                        {
                            options.Coupon = u.CouponId;
                            subscriptionPlan.CouponId = u.CouponId;
                        }
                    }

                    var service = new Stripe.SubscriptionService();
                    var newSubscription = await service.CreateAsync(options);
                    
                    subscriptionPlan.StripeSubscriptionId = newSubscription.Id;
                    SetSubscriptionState(ref subscriptionPlan, newSubscription);
                    
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    UserBillingPlan subscriptionPlan = new UserBillingPlan() { BillingPlanTypeId = newSubscriptionId, UserId = u.Id, CreationDate = DateTime.Now };
                    subscriptionPlan.Status = BillingPlanStatus.AWAITING_ACTIVATION;
                    subscriptionPlan.PlannedActivationDate = resp.CancellationDate;
                    
                    if (subscription.BillingOption == BillingPlanOption.STUDENT)
                    {

                        subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.AWAITING_STUDENT_CARD;
                        subscriptionPlan.IsStudent = true;
                    }
                    else
                    {
                        subscriptionPlan.StudentCardVerificationStatus = StudentCardStatus.CANCELLED;
                        subscriptionPlan.IsStudent = false;
                    }

                    if (!string.IsNullOrEmpty(u.CouponId) && (!u.CouponValidDate.HasValue || (u.CouponValidDate.HasValue && u.CouponValidDate.Value > DateTime.Now)))
                    {
                        if (await CheckCouponExists(u.CouponId))
                        {
                            subscriptionPlan.CouponId = u.CouponId;
                        }
                    }

                    subscriptionPlan.StripePriceId = subscription.StripePriceId;
                    subscriptionPlan.StripeProductId = subscription.StripeProductId;

                    ctx.UserBillingPlans.Add(subscriptionPlan);
                    await ctx.SaveChangesAsync();
                    changeResp.UserSubscriptionId = subscriptionPlan.Id;
                    changeResp.ValidFrom = resp.CancellationDate;

                    if (subscriptionPlan.IsStudent && subscriptionPlan.StudentCardVerificationStatus != StudentCardStatus.ACCEPTED)
                    {
                        return changeResp;
                    }


                    var options = new SubscriptionScheduleCreateOptions
                    {
                        Customer = u.StripeCustomerId,
                       
                        StartDate = resp.CancellationDate,
                        EndBehavior = "release",
                        
                        Metadata = new Dictionary<string, string>
                        {
                            {UserBillingPlanIdKey,subscriptionPlan.Id.ToString()}
                        },
                        Phases = new List<SubscriptionSchedulePhaseOptions>
                        {
                        new SubscriptionSchedulePhaseOptions
                        {
                            Items = new List<SubscriptionSchedulePhaseItemOptions>
                            {
                                new SubscriptionSchedulePhaseItemOptions
                                {
                                    Price = subscriptionPlan.StripePriceId
                                },
                                      
                            },
                            Coupon = subscriptionPlan.CouponId
                        },
                    },
                    };
                    var service = new SubscriptionScheduleService();
                    var subSchedule = await service.CreateAsync(options);
                    subscriptionPlan.StripeSubscriptionScheduleId = subSchedule.Id;
                    subscriptionPlan.StripeSubscriptionId = subSchedule.SubscriptionId;
                    subscriptionPlan.Status = BillingPlanStatus.AWAITING_ACTIVATION;
                    await ctx.SaveChangesAsync();
                }
            }


            return changeResp;
        }

        private async Task<bool> CheckCouponExists(string couponId)
        {
            try
            {
                CouponService svc = new CouponService();
                var coupon = await svc.GetAsync(couponId);
                if (coupon != null)
                {
                    return true;
                }
                return false;
            }
            catch(StripeException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<UserBillingPlan> GetUserCurrentSubscriptionPlan(int userId)
        {
            await UpdateSubscriptionStatesFromStripeForUser(await _userSvc.GetUserById(userId));
            using (var ctx = new DataContext())
            {
                var today = DateTime.Today;
                var plan = await ctx.UserBillingPlans.Where(t => t.UserId == userId).OrderByDescending(t=>t.CreationDate).Include(b => b.BillingPlanType).ThenInclude(p => p.Price).FirstOrDefaultAsync(t =>
                  t.Status == BillingPlanStatus.ACTIVE ||
                t.Status == BillingPlanStatus.AWAITING_ACTIVATION ||               
                        t.Status == BillingPlanStatus.AWAITING_PAYMENT ||
                        t.Status == BillingPlanStatus.PENDING
                   );

                if (plan != null && plan.Status == BillingPlanStatus.ACTIVE && plan.ExpiryDate.HasValue)
                {
                    var betweenDates = CheckBetweenDates(plan.ActivationDate ?? today, plan.ExpiryDate, today);
                    if (!betweenDates)
                        return null;
                }

                return plan;
            }
        }
        public async Task<ICollection<UserBillingPlan>> GetAllUserSubscriptionPlans(int userId)
        {
            await UpdateSubscriptionStatesFromStripeForUser(await _userSvc.GetUserById(userId));
            using (var ctx = new DataContext())
            {
       
                var plans = await ctx.UserBillingPlans.Where(t => t.UserId == userId).OrderBy(t => t.CreationDate).Include(b => b.BillingPlanType).ThenInclude(p => p.Price).ToListAsync();

                return plans;
            }
        }

        public async Task<UserBillingPlan> GetUserActiveSubscriptionPlan(int userId)
        {
            await UpdateSubscriptionStatesFromStripeForUser(await _userSvc.GetUserById(userId));
            using (var ctx = new DataContext())
            {
                var today = DateTime.Today;
                var plan = await ctx.UserBillingPlans.Where(t => t.UserId == userId).Include(b => b.BillingPlanType).ThenInclude(p => p.Price).OrderByDescending(t=>t.CreationDate).FirstOrDefaultAsync(t =>
                          (t.Status == BillingPlanStatus.ACTIVE && (!t.ExpiryDate.HasValue ||t.ExpiryDate.Value.Date>= DateTime.Today))
                   );

                if(plan == null)
                {
                    return null;
                }

                return plan;
            }
        }

        public async Task UpdateSubscriptionStatesFromStripeForUser(User u)
        {
            if(string.IsNullOrEmpty(u.StripeCustomerId))
            {
                throw new CoachOnlineException("User does not have Stripe customer id.", CoachOnlineExceptionState.NotExist);
            }
            var opts = new SubscriptionListOptions()
            {
                Customer = u.StripeCustomerId,
                Status = "canceled"
                
            };
            var opts2 = new SubscriptionListOptions()
            {
                Customer = u.StripeCustomerId

            };
            var subscriptionSvc = new Stripe.SubscriptionService();
            var subscriptionsCancelled = await subscriptionSvc.ListAsync(opts);
            var subscriptionsActive = await subscriptionSvc.ListAsync(opts2);

            var subscriptions = subscriptionsActive.Union(subscriptionsCancelled).ToList();

            var schedOptions = new SubscriptionScheduleListOptions()
            {
                Customer = u.StripeCustomerId
            };
            var schedSubscriptionSvc = new Stripe.SubscriptionScheduleService();
            var schedules = await schedSubscriptionSvc.ListAsync(schedOptions);

            using (var ctx = new DataContext())
            {

                foreach(var schedule in schedules)
                {

                    if(schedule.Status != "not_started")
                    {
                        var billingPlan = await ctx.UserBillingPlans.Where(t => t.UserId == u.Id && t.StripeSubscriptionScheduleId == schedule.Id && t.StripeSubscriptionId == null && t.Status == BillingPlanStatus.AWAITING_ACTIVATION).FirstOrDefaultAsync();
                        if(billingPlan!= null)
                        {
                            billingPlan.Status = BillingPlanStatus.DELETED;
                            await ctx.SaveChangesAsync();
                        }
                    }
                    else if(schedule.Status == "not_started")
                    {
                        if(schedule.Phases.Any())
                        {
                            var phase = schedule.Phases.First();
                            var startDate = phase.StartDate;
                            var priceId = phase.Items.First().PriceId;
                            //int? scheduleId = GetUserBillingPlanIdFromSubscriptionSchedule(schedule);
                            var billingPlan = await ctx.UserBillingPlans.Where(t => t.UserId == u.Id && t.StripePriceId == priceId  && t.StripeSubscriptionScheduleId == schedule.Id).FirstOrDefaultAsync();
                            var billingPlanType = await ctx.BillingPlans.Where(t => t.StripePriceId == priceId).Include(p => p.Price).FirstOrDefaultAsync();
                            if (billingPlan == null && billingPlanType != null)
                            {
                                var subPlan = new UserBillingPlan() { UserId = u.Id, CreationDate = DateTime.Now, BillingPlanTypeId = billingPlanType.Id, Status = BillingPlanStatus.AWAITING_ACTIVATION, PlannedActivationDate=startDate };
                                subPlan.StripeSubscriptionId = null;
                                subPlan.StripeSubscriptionScheduleId = schedule.Id;
                                subPlan.StripePriceId = priceId;
      

                                if (billingPlanType.BillingOption == BillingPlanOption.STUDENT)
                                {
                                    subPlan.IsStudent = true;
                                }
                                else
                                {
                                    subPlan.IsStudent = false;
                                }

                                ctx.UserBillingPlans.Add(subPlan);
                                await ctx.SaveChangesAsync();
                            }
                        }
                    }
                }

                if(subscriptions.Count()==0)
                {
                    var billingPlan = ctx.UserBillingPlans.Where(b => b.UserId == u.Id && b.StripeSubscriptionId != null);
                    foreach(var bp in billingPlan)
                    {
                        bp.Status = BillingPlanStatus.CANCELLED;
                    }

                    await ctx.SaveChangesAsync();

                }
                else {
                    foreach (var sub in subscriptions)
                    {
                        var billingPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == u.Id && b.StripeSubscriptionId == sub.Id);
                        if (billingPlan == null)
                        {
                            //check by metadata
                            var userBillingPlanId = GetUserBillingPlanIdFromSubscription(sub);
                            if (userBillingPlanId.HasValue)
                            {
                                billingPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == u.Id && b.Id == userBillingPlanId);
                            }
                        }
                        var priceId = sub.Items.Data[0].Price.Id;
                        var productId = sub.Items.Data[0].Price.ProductId;

                        if (billingPlan != null)
                        {
                            billingPlan.StripeSubscriptionId = sub.Id;
                            billingPlan.StripePriceId = priceId;
                            billingPlan.StripeProductId = productId;
                            
                            SetSubscriptionState(ref billingPlan, sub);
                            await ctx.SaveChangesAsync();
                        }
                        else
                        {
                            //need to create new billing plan

                            var billingPlanType = await ctx.BillingPlans.Where(t => t.StripePriceId == priceId && t.StripeProductId == productId).Include(p => p.Price).FirstOrDefaultAsync();
                            if (billingPlanType != null)
                            {
                                //create such billing plan for user
                                var subPlan = new UserBillingPlan() { UserId = u.Id, CreationDate = DateTime.Now, BillingPlanTypeId = billingPlanType.Id, Status = BillingPlanStatus.PENDING };
                                subPlan.StripeSubscriptionId = sub.Id;
                                subPlan.StripePriceId = priceId;
                                subPlan.StripeProductId = productId;

                                if (billingPlanType.BillingOption == BillingPlanOption.STUDENT)
                                {
                                    subPlan.IsStudent = true;
                                }
                                else
                                {
                                    subPlan.IsStudent = false;
                                }

                                SetSubscriptionState(ref subPlan, sub);

                                ctx.UserBillingPlans.Add(subPlan);
                                await ctx.SaveChangesAsync();
                            }
                        }
                    }
                }
            }

            await ChangeUserActiveSubscriptionState(u.Id);
        }

        public async Task UploadStudentCardForSubscription(int subscriptionPlanId, List<PhotoBase64Rqs> photosInBase64)
        {
            Console.WriteLine("Uploading student cards. Subscription id is: " + subscriptionPlanId);
            if(!photosInBase64.Any() || photosInBase64.Any(t=>string.IsNullOrEmpty(t.ImgBase64)))
            {
                throw new CoachOnlineException("No image data provided.", CoachOnlineExceptionState.DataNotValid);
            }
            using (var ctx = new DataContext())
            {
                var plan = await ctx.UserBillingPlans.Where(t => t.Id == subscriptionPlanId).Include(b=>b.BillingPlanType).FirstOrDefaultAsync();
                plan.CheckExist("Plan");

                if(!plan.IsStudent)
                {
                    throw new CoachOnlineException("User didn't select student option. Cannot upload student card data.", CoachOnlineExceptionState.WrongDataSent);
                }
                if((plan.StudentCardVerificationStatus == StudentCardStatus.ACCEPTED || plan.StudentCardVerificationStatus == StudentCardStatus.REJECTED))
                {
                    throw new CoachOnlineException("User subscription has incorrect student card status.", CoachOnlineExceptionState.DataNotValid);
                }
                if(!(plan.Status == BillingPlanStatus.AWAITING_ACTIVATION || plan.Status == BillingPlanStatus.PENDING))
                {
                    throw new CoachOnlineException("User subscription has incorrect status.", CoachOnlineExceptionState.DataNotValid);
                }


                plan.StudentCardData = new List<UserStudentCard>();

   
                    foreach (var imgDataBase64 in photosInBase64)
                    {
                        var hashName = Statics.LetsHash.RandomHash(DateTime.Now.ToString());
                       SaveImage(imgDataBase64.ImgBase64, hashName);
                    Console.WriteLine("Student card data saved");
                       plan.StudentCardData.Add(new UserStudentCard() { StudentsCardPhotoName = hashName, UserBillingPlanId = plan.Id });
                    }
   

                plan.StudentCardVerificationStatus = StudentCardStatus.IN_VERIFICATION;
                Console.WriteLine($"Plan status is {plan.StatusStr}");
                Console.WriteLine($"Student card status is {plan.StudentCardVerificationStatusStr}");
                await ctx.SaveChangesAsync();
                Console.WriteLine("Images saved");
            }
        }

        public async Task<PaymentMethod> GetCustomerStripePaymentMethod(string paymentSourceId)
        {
            try
            {
                if(paymentSourceId==null)
                {
                    return null;
                }
                PaymentMethodService pSvc = new PaymentMethodService();
                var payment = await pSvc.GetAsync(paymentSourceId);

                return payment;
            }
            catch(StripeException ex)
            {
                return null;
            }

        }

        public async Task<Customer> GetStripeCustomer(string stripeCustomerId)
        {
            CustomerService svc = new CustomerService();
            var customer = await svc.GetAsync(stripeCustomerId);
            return customer;
        }

        public async Task<Subscription> GetCustomerStripeSubscription(string subscriptionId)
        {
            Stripe.SubscriptionService svc = new Stripe.SubscriptionService();
            var subscription = await svc.GetAsync(subscriptionId);
            return subscription;
        }

        public async Task<SubscriptionSchedule> GetCustomerStripeSubscriptionSchedule(string subScheduleId)
        {
            Stripe.SubscriptionScheduleService svc = new Stripe.SubscriptionScheduleService();
            var subscription = await svc.GetAsync(subScheduleId);
            return subscription;
        }

        public int? GetUserBillingPlanIdFromSubscription(Subscription s)
        {
            if(s.Metadata.ContainsKey(UserBillingPlanIdKey))
            {
                return s.Metadata[UserBillingPlanIdKey].ToInteger();
            }
            else
            {
                return null;
            }
        }

        public int? GetUserBillingPlanIdFromSubscriptionSchedule(SubscriptionSchedule s)
        {
            if (s.Metadata.ContainsKey(UserBillingPlanIdKey))
            {
                return s.Metadata[UserBillingPlanIdKey].ToInteger();
            }
            else
            {
                return null;
            }
        }

        public async Task<PaymentIntent> GetPaymentIntentById(string paymentIntentId)
        {
            try
            {
                if (paymentIntentId == null)
                    return null;
                PaymentIntentService paySvc = new PaymentIntentService();
                var paymentIntent = await paySvc.GetAsync(paymentIntentId);

                return paymentIntent;
            }
            catch(StripeException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<Invoice> GetInvoiceById(string invoiceId)
        {
            try
            {
                InvoiceService invSvc = new InvoiceService();
                var invoice = await invSvc.GetAsync(invoiceId);

                return invoice;
            }
            catch (StripeException ex)
            {
                return null;
            }
        }

        public async Task ChangeUserActiveSubscriptionState(int userId)
        {

            using (var ctx = new DataContext())
            {
               
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                if (user != null)
                {
                    if (user.UserRole == UserRoleType.INSTITUTION_STUDENT)
                    {
                        if (!user.InstitutionId.HasValue)
                        {
                            user.SubscriptionActive = false;
                        }
                        else
                        {
                            var inst = await ctx.LibraryAccounts.Where(t => t.Id == user.InstitutionId.Value).Include(s=>s.Subscriptions).FirstOrDefaultAsync();
                            if(inst != null && inst.Subscriptions != null && inst.Subscriptions.Any(t=>t.Status == LibrarySubscriptionStatus.ACTIVE))
                            {
                                user.SubscriptionActive = true;
                            }
                            else
                            {
                                user.SubscriptionActive = false;
                            }
                        }
                    }
                    else
                    {
                        var anyActive = ctx.UserBillingPlans.Any(b => b.UserId == userId && b.Status == BillingPlanStatus.ACTIVE && (!b.ExpiryDate.HasValue || b.ExpiryDate.Value >= DateTime.Today));
                        if (!anyActive)
                        {
                            user.SubscriptionActive = false;
                        }
                        else
                        {
                            user.SubscriptionActive = true;
                        }
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }


        public async Task<bool> IsUserSubscriptionActive(int userId)
        {
            await ChangeUserActiveSubscriptionState(userId);
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                if(user != null && user.UserRole == UserRoleType.STUDENT && user.SubscriptionActive)
                {
                    return true;
                }
                else if(user != null && user.UserRole == UserRoleType.INSTITUTION_STUDENT)
                {
                    if(user.SubscriptionActive && user.InstitutionId.HasValue)
                    {
                        var isUserConnected = await _libSvc.IsUserCurrentlyConnectedAndAllowed(user.InstitutionId.Value, user.Id);

                        if(isUserConnected > 0)
                        {
                            Console.WriteLine($"User already connected: Sub active");
                            return true;
                        }


                        var limit = await _libSvc.GetConnectionsLimitForLibrary(user.InstitutionId.Value);
                        var current = await _libSvc.GetCurrentConnections(user.InstitutionId.Value);
                        
                        Console.WriteLine($"Checking user sub. Current:{current}, limit {limit}");
                        if(current <= limit)
                        {
                            Console.WriteLine($"Sub active");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Sub not active");
                            return false;
                        }
                    }
                    return false;
                }
                else if(user != null && user.UserRole == UserRoleType.COACH)
                {
                    //if coach has active course then let him watch
                    var anyCourseThatsActive = await ctx.courses.AnyAsync(t => t.UserId == user.Id && t.State == CourseState.APPROVED);

                    return anyCourseThatsActive;
                }

                return false;
            }
        }

        public void SetSubscriptionState(ref UserBillingPlan billingPlan, Subscription sub)
        {
            if (sub.Status == "active")
            {
                billingPlan.Status = BillingPlanStatus.ACTIVE;
                billingPlan.ActivationDate = sub.StartDate;
                billingPlan.ExpiryDate = sub.CancelAtPeriodEnd ? (DateTime?)sub.CurrentPeriodEnd : null;
                if(sub.CancelAt.HasValue)
                {
                    billingPlan.ExpiryDate = sub.CancelAt;
                }
                else if(!sub.CancelAt.HasValue && !sub.CancelAtPeriodEnd)
                {
                    billingPlan.ExpiryDate = null;
                }
            }
            else if (sub.Status == "incomplete")
            {
                billingPlan.Status = BillingPlanStatus.AWAITING_PAYMENT;
            }
            else if (sub.Status == "trialing")
            {
                billingPlan.Status = BillingPlanStatus.ACTIVE;
                billingPlan.ActivationDate = sub.StartDate;
                billingPlan.ExpiryDate = sub.CancelAtPeriodEnd ? (DateTime?)sub.CurrentPeriodEnd : null;
            }
            else if (sub.Status == "canceled")
            {
                billingPlan.Status = BillingPlanStatus.CANCELLED;
                billingPlan.ExpiryDate = sub.EndedAt;
            }
            else if (sub.Status == "incomplete_expired")
            {
                billingPlan.Status = BillingPlanStatus.PAYMENT_REJECTED;
            }
            else if (sub.Status == "unpaid")
            {
                billingPlan.Status = BillingPlanStatus.CANCELLED;
            }
            else if (sub.Status == "past_due")
            {
                billingPlan.Status = BillingPlanStatus.CANCELLED;
            }
        }

        #region private members

        public async Task UpdateSubscriptionPlans()
        {
            var pSvc = new Stripe.ProductService();
            var stripeProducts = await pSvc.ListAsync();
            var priceSvc = new PriceService();
            var prices = await priceSvc.ListAsync();
            using (var ctx = new DataContext())
            {
                foreach (var p in stripeProducts)
                {
                    bool isNew = false;
                    var product = await ctx.BillingPlans.Where(t => t.StripeProductId == p.Id).Include(p => p.Price).FirstOrDefaultAsync();
                    if (product != null)
                    {
                        product.IsActive = p.Active;
                        product.Name = p.Name;
                        product.Description = p.Description;
                    }
                    else
                    {
                        isNew = true;
                        product = new BillingPlan() { StripeProductId = p.Id, IsActive = p.Active, Name = p.Name, Description = p.Description };
                    }

                    if (p.Metadata.ContainsKey("StudentOption") && p.Metadata["StudentOption"] == "true")
                    {
                        product.BillingOption = BillingPlanOption.STUDENT;
                    }
                    else
                    {
                        product.BillingOption = BillingPlanOption.NORMAL;
                    }

                    if (p.Metadata.ContainsKey("IsPublic") && p.Metadata["IsPublic"] == "false")
                    {
                        product.IsPublic = false;
                    }
                    else
                    {
                        product.IsPublic = true;
                    }
                    var stripePrice = prices.Where(t => t.ProductId == p.Id && t.Active).FirstOrDefault();

                    if (stripePrice == null)
                    {
                        if (product.Price != null)
                        {
                            ctx.Remove(product.Price);
                        }

                        product.Currency = null;
                        product.StripePriceId = null;
                        product.AmountPerMonth = null;
                    }
                    else
                    {
                        if (product.Price == null)
                        {
                            product.Price = new SubscriptionPrice();
                        }
                        product.Price.StripePriceId = stripePrice.Id;
                        product.Price.Reccuring = stripePrice.Recurring != null;
                        product.Price.TrialDays = stripePrice.Recurring.TrialPeriodDays.HasValue ? (int)stripePrice.Recurring.TrialPeriodDays.Value : 0;
                        if (product.Price.Reccuring)
                        {
                            product.Price.PeriodType = stripePrice.Recurring.Interval;
                            product.Price.Period = (int)stripePrice.Recurring.IntervalCount;
                        }
                        else
                        {
                            product.Price.Period = null;
                            product.Price.PeriodType = null;
                        }
                        product.Price.Currency = stripePrice.Currency;
                        product.Price.Amount = stripePrice.UnitAmountDecimal.HasValue ? stripePrice.UnitAmountDecimal.Value / 100 : 0;
                        product.StripePriceId = stripePrice.Id;
                        product.Currency = stripePrice.Currency;
                        product.AmountPerMonth = product.Price.Reccuring ? GetProductAmountPerMonth(product.Price.Period, product.Price.PeriodType, product.Price.Amount) : null;

                    }

                    if (isNew)
                    {
                        ctx.BillingPlans.Add(product);
                    }

                    await ctx.SaveChangesAsync();

                }
            }
        }

        private decimal? GetProductAmountPerMonth(int? period, string periodType, decimal? amount)
        {
            if (!period.HasValue || !amount.HasValue || period.Value == 0) return null;
            decimal result = 0;
            if(periodType == "month")
            {
                result = amount.Value / period.Value;
            }
            else if(periodType == "year")
            {
                result = amount.Value / (period.Value*12);

            }
            else if(periodType == "day")
            {
                //we are assuming 30 days in month
                result = (amount.Value / period.Value) * 30;
            }

            result = Math.Round(result, 2);

            return result;

        }

        private bool CheckBetweenDates(DateTime? start, DateTime? end, DateTime date)
        {
            if (!start.HasValue || !end.HasValue) return false;

            if (start.Value.Date <= date && end.Value.Date >= date.Date) return true;

            return false;
        }

        private static void SaveImage(string base64, string name)
        {
            try
            {
                base64 = base64.Split(',').Last().Trim();
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    using (Bitmap bm2 = new Bitmap(ms))
                    {
                        bm2.Save($"{ConfigData.Config.EnviromentPath}/wwwroot/student_cards/" + $"{name}.jpg");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private DateTime? CalculateBillingPlanExpirationDate(DateTime startDate, int period, string periodType)
        {
            if (periodType == null) return null;
            periodType = periodType.ToUpper();
            if (periodType == "DAY")
            {
                return startDate.AddDays(period);
            }
            else if (periodType == "MONTH")
            {
                return startDate.AddMonths(period);
            }
            else if (periodType == "YEAR")
            {
                return startDate.AddYears(period);
            }
            else if (periodType == "WEEK")
            {
                return startDate.AddDays(7*period);
            }

            return null;
        }

       

        #endregion
    }
}

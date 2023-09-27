using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class ProductManageService:IProductManage
    {
        private readonly ILogger<ProductManageService> _logger;
        private readonly ISubscription _subSvc;
        public ProductManageService(ILogger<ProductManageService> logger, ISubscription subSvc)
        {
            _logger = logger;
            _subSvc = subSvc;
        }

        public class PlanUpdateOptsRqs
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public bool IsPublic { get; set; }
            public int? TrialDays { get; set; }   
            public bool IsStudentCardRequired { get; set; }
        }

        public async Task UpdateProduct(int planId, PlanUpdateOptsRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var plan = await ctx.BillingPlans.FirstOrDefaultAsync(x => x.Id == planId);
                plan.CheckExist("Product");

                if(string.IsNullOrEmpty(rqs.Name))
                {
                    rqs.Name = plan.Name;
                }

                var productSvc = new ProductService();
                var product = await productSvc.GetAsync(plan.StripeProductId);

                var priceSvc = new PriceService();
                var price = await priceSvc.GetAsync(plan.StripePriceId);

                var metaData = new Dictionary<string, string>();
                metaData.Add("IsPublic", rqs.IsPublic?"true":"false");
                metaData.Add("StudentOption", rqs.IsStudentCardRequired ? "true" : "false");

                await productSvc.UpdateAsync(product.Id, new ProductUpdateOptions { Metadata = metaData, Active = rqs.IsActive, Name = rqs.Name, Description = rqs.Description });

                await priceSvc.UpdateAsync(price.Id, new PriceUpdateOptions {Recurring = new PriceRecurringOptions { TrialPeriodDays = rqs.TrialDays} });

                await _subSvc.UpdateSubscriptionPlans();
            }
        }

        public async Task<List<BillingPlan>> GetProducts()
        {
        

            using(var ctx = new DataContext())
            {
                var data = await ctx.BillingPlans.Include(x=>x.Price).ToListAsync();

                return data;
            }
        }

        public async Task CreateCoupon(string name, CouponDuration duration, int? percentOff, decimal? amountOff, string currency, int? durationInMonths, bool forInfluencers)
        {
            if(!amountOff.HasValue && !percentOff.HasValue)
            {
                throw new CoachOnlineException("Either amount or percentage of a coupon must be specified.", CoachOnlineExceptionState.DataNotValid);
            }
            if(amountOff.HasValue && percentOff.HasValue)
            {
                amountOff = null;
                currency = null;
            }
            if(duration == CouponDuration.repeating && (!durationInMonths.HasValue || durationInMonths.Value <1))
            {
                throw new CoachOnlineException("Duration in months must be specified when in repeating duration period mode.", CoachOnlineExceptionState.DataNotValid);
            }

            if(percentOff.HasValue && !(percentOff.Value >0 && percentOff.Value <=100))
            {
                throw new CoachOnlineException("When providing coupon with percentage off the given number needs to be between 1-100", CoachOnlineExceptionState.DataNotValid);
            }

            if(amountOff.HasValue && (amountOff.Value <=0 || string.IsNullOrEmpty(currency)))
            {
                throw new CoachOnlineException("When providing coupon with amount off the given number needs to be greater than 0 and it has to contain currency code.", CoachOnlineExceptionState.DataNotValid);
            }

            int? calcAmountOff = null;

            if(amountOff.HasValue && amountOff.Value>0)
            {
                calcAmountOff = (int)(Math.Round(amountOff.Value, 2) * 100);
            }

            var options = new CouponCreateOptions
            {
                Name = name,
                Duration = duration.ToString(),
                PercentOff = percentOff,
                DurationInMonths = durationInMonths,
                AmountOff = calcAmountOff.HasValue ? calcAmountOff:null,
                Currency = currency
            };

            var service = new CouponService();
            var c = await service.CreateAsync(options);

            using(var ctx = new DataContext())
            {
                var promoCoupon = new PromoCoupon()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Duration = duration,
                    PercentOff = (int?)c.PercentOff,
                    DurationInMonths = durationInMonths,
                    AmountOff = c.AmountOff.HasValue? (decimal?)c.AmountOff/100m: null,
                    Currency = c.Currency,
                    AvailableForInfluencers = forInfluencers
                };

                ctx.PromoCoupons.Add(promoCoupon);
                await ctx.SaveChangesAsync();
                
            }
        }

        public async Task DeleteCoupon(string couponId)
        {
            var service = new CouponService();
            await service.DeleteAsync(couponId);

            using(var ctx = new DataContext())
            {
                var coupon = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == couponId);
                if(coupon != null)
                {
                    ctx.PromoCoupons.Remove(coupon);

                    await ctx.SaveChangesAsync();
                }

            }
        }

        public async Task UpdateCoupon(string couponId, string name, bool availableForInfluencers)
        {
            using(var ctx = new DataContext())
            {
                var coupon = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == couponId);
                coupon.CheckExist("Coupon");
                coupon.AvailableForInfluencers = availableForInfluencers;
                coupon.Name = name;

                var service = new CouponService();
                await service.UpdateAsync(couponId, new CouponUpdateOptions() { Name = name });

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<List<CouponResponse>> GetCouponsForInfluencers()
        {
            var data = new List<CouponResponse>();

            using (var ctx = new DataContext())
            {
                var coupons = await ctx.PromoCoupons.Where(x=>x.AvailableForInfluencers).ToListAsync();

                foreach (var c in coupons)
                {
                    data.Add(new CouponResponse()
                    {
                        AmountOff = c.AmountOff,
                        Currency = c.Currency,
                        PercentOff = c.PercentOff,
                        Duration = c.Duration,
                        Id = c.Id,
                        Name = c.Name,
                        DurationInMonths = c.DurationInMonths,
                        AvailableForInfluencers = c.AvailableForInfluencers
                    });
                }
            }
            return data;
        }

        public async Task<List<CouponResponse>> GetCoupons()
        {
            //var service = new CouponService();

            //var existingCoupons = await service.ListAsync();

            var data = new List<CouponResponse>();
            //foreach(var c in existingCoupons.Data)
            //{
            //    data.Add(new CouponResponse()
            //    {
            //        AmountOff = c.AmountOff,
            //        Currency = c.Currency,
            //        PercentOff = c.PercentOff,
            //        Duration = c.Duration,
            //        Id = c.Id,
            //        Name = c.Name,
            //        DurationInMonths = (int?)c.DurationInMonths
            //    });

            //}

            using(var ctx = new DataContext())
            {
                var coupons = await ctx.PromoCoupons.ToListAsync();

                foreach(var c in coupons)
                {
                    data.Add(new CouponResponse()
                    {
                        AmountOff = c.AmountOff,
                        Currency = c.Currency,
                        PercentOff = c.PercentOff,
                        Duration = c.Duration,
                        Id = c.Id,
                        Name = c.Name,
                        DurationInMonths = c.DurationInMonths,
                        AvailableForInfluencers = c.AvailableForInfluencers
                    });
                }
            }
            return data;
        }

    }
}

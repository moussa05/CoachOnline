using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.ProductManageService;

namespace CoachOnline.Interfaces
{
    public interface IProductManage
    {
        Task CreateCoupon(string name, CouponDuration duration, int? percentOff, decimal? amountOff, string currency, int? durationInMonths, bool forInfluencers);
        Task DeleteCoupon(string couponId);
        Task<List<CouponResponse>> GetCoupons();
        Task UpdateCoupon(string couponId, string name, bool availableForInfluencers);
        Task<List<CouponResponse>> GetCouponsForInfluencers();
        Task<List<BillingPlan>> GetProducts();
        Task UpdateProduct(int planId, PlanUpdateOptsRqs rqs);
    }
}

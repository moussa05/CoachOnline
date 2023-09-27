using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.HelperModels;
using CoachOnline.PayPalIntegration;
using CoachOnline.PayPalIntegration.Model;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
using ITSAuth.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CoachOnline.Services.AffiliateTypesDictionary;

namespace CoachOnline.Services
{
    public class AffiliateTypesDictionary
    {
        public class RetModel
        {
            public string Value { get; set; }
            public string Tooltip { get; set; }
        }

        public AffiliateTypesDictionary()
        {
            Dictionary.Add(AffiliateNotifType.FirstLineCoach, new RetModel { Tooltip = "A la fin de chaque journée, nous créditerons votre solde de 20% du revenu de ce coach.", Value = "20%" });
            Dictionary.Add(AffiliateNotifType.YearlyFirstLineStudentInfluencer, new RetModel { Tooltip = "Si un utilisateur s'est inscrit par votre intermédiaire avec un abonnement annuel, vous recevrez immédiatement un paiement de 70% pour le premier mois et de 20% pour chaque mois suivant.", Value = "20%" });
            Dictionary.Add(AffiliateNotifType.YearlyFirstLineStudentRegular, new RetModel { Tooltip = "Si un utilisateur s'est inscrit à partir de votre recommandation avec un abonnement annuel, vous recevrez immédiatement le paiement d'un mois complet, à partir du deuxième mois, vous recevrez 20% de chacun des paiements mensuels de cet utilisateur.", Value = "20%" });
            Dictionary.Add(AffiliateNotifType.MonthlyFirstLineStudentInfluencer, new RetModel { Tooltip = "Si un utilisateur s'est inscrit sur votre recommandation avec un abonnement mensuel, vous recevrez immédiatement un paiement le deuxième mois de son abonnement égal à 70% du premier mois et 20% du deuxième mois et 20% de chacun de ses paiements ultérieurs.", Value = "20%" });
            Dictionary.Add(AffiliateNotifType.MonthlyFirstLineStudentRegular, new RetModel { Tooltip = "Pour tout utilisateur inscrit à partir de votre affiliation pour un abonnement annuel, vous percevez immédiatement 70% du 1er mois que cet utilisateur a réglé, et ensuite vous percevrez 20% de chacun de ses paiements mensuels.", Value = "20%" });
            Dictionary.Add(AffiliateNotifType.SecondLineStudentInfluencer, new RetModel { Tooltip = "Si un utilisateur s'inscrit à la suite de votre recommandation, vous recevrez 5 % de son revenu d'affiliation total s'il recommande d'autres utilisateurs à la plate-forme.", Value = "5%" });
            Dictionary.Add(AffiliateNotifType.SecondLineStudentRegular, new RetModel { Tooltip = "Si un utilisateur qui s'est inscrit sur votre recommandation a recommandé d'autres utilisateurs à la plateforme, vous recevrez 5 % mensuellement après troisième mois.", Value = "5%" });
            Dictionary.Add(AffiliateNotifType.NoSub, new RetModel { Tooltip = "Le plan d'abonnement n'est pas choisi", Value = "0%" });
        }

        public enum AffiliateNotifType
        {
            NoSub,
            YearlyFirstLineStudentRegular,
            MonthlyFirstLineStudentRegular,
            SecondLineStudentRegular,
            FirstLineCoach,
            YearlyFirstLineStudentInfluencer,
            MonthlyFirstLineStudentInfluencer,
            SecondLineStudentInfluencer
        }
        public Dictionary<AffiliateNotifType, RetModel> Dictionary = new Dictionary<AffiliateNotifType, RetModel>();
    }

    public class AffiliateService : IAffiliate
    {
        private readonly ILogger<AffiliateService> _logger;
        private readonly IEmailApiService _emailSvc;
        private readonly IPayPal _payPal;
        private readonly ISubscription _subSvc;
        private readonly IProductManage _prodManageSvc;

        Random rn = new Random();
        public AffiliateService(ILogger<AffiliateService> logger, IEmailApiService emailSvc, IStream streamSvc, IPayPal payPal, ISubscription subSvc, IProductManage prodManageSvc)
        {
            _logger = logger;
            _emailSvc = emailSvc;
            _payPal = payPal;
            _subSvc = subSvc;
            _prodManageSvc = prodManageSvc;
        }

        public async Task<AffiliateLink> GetTokenByAffLink(string link)
        {
            using (var ctx = new DataContext())
            {

                var x = await ctx.AffiliateLinks.FirstOrDefaultAsync(t => t.GeneratedLink == link);
                x.CheckExist("Link");

                //var response = new Tuple<string, string>(x.GeneratedToken, x.ForCoach ? "COACH" : "STUDENT");

                return x;
            }
        }

        private async Task<string> GetNonExistingHash(string x)
        {
            var hash = LetsHash.RandomHash(x);
            var exists = true;
            using (var ctx = new DataContext())
            {
                while (exists)
                {
                    var tokenExists = await ctx.AffiliateLinks.AnyAsync(x => x.GeneratedToken == hash);
                    if (tokenExists)
                    {
                        hash = LetsHash.RandomHash(x);
                    }
                    else
                    {
                        exists = false;
                    }
                }
            }

            return hash;
        }

        public async Task<string> GenerateAffiliateLink(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.Surname))
                {
                    _logger.LogInformation("To generate affiliate link please fill in your firstname and lastname");
                    throw new CoachOnlineException("Pour générer votre lien d'affiliation, remplissez d'abord vos coordonnées dans la section Paramètres du compte.", CoachOnlineExceptionState.DataNotValid);
                }

                var hash = await GetNonExistingHash(user.EmailAddress);

                var link = user.FirstName.Replace(" ", "_").Replace("/", "_").Replace(@"\", "_").Replace(".", "").Replace(",", "").Replace("+", "") + user.Surname.First() + "_" + hash.Substring(0, 10);

                var exists = await ctx.AffiliateLinks.AnyAsync(t => t.GeneratedLink == link);

                if (exists)
                {
                    throw new CoachOnlineException("Such link alredy exists", CoachOnlineExceptionState.AlreadyExist);
                }

                ctx.AffiliateLinks.Add(new Model.AffiliateLink() { CreateDate = DateTime.Now, GeneratedLink = link, UserId = userId, GeneratedToken = hash, ForCoach = false });

                await ctx.SaveChangesAsync();

                return $"{ConfigData.Config.SiteUrl}/ref/{link}";
            }
        }

        public async Task<string> GenerateAffiliateLinkForCoach(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.Surname))
                {
                    _logger.LogInformation("To generate affiliate link please fill in your firstname and lastname");
                    throw new CoachOnlineException("Pour générer votre lien d'affiliation, remplissez d'abord vos coordonnées dans la section Paramètres du compte.", CoachOnlineExceptionState.DataNotValid);
                }

                var hash = await GetNonExistingHash(user.EmailAddress);

                var link = user.FirstName.Replace(" ", "_").Replace("/", "_").Replace(@"\", "_").Replace(".", "").Replace(",", "").Replace("+", "") + user.Surname.First() + "_" + hash.Substring(0, 10);

                var exists = await ctx.AffiliateLinks.AnyAsync(t => t.GeneratedLink == link);

                if (exists)
                {
                    throw new CoachOnlineException("Such link alredy exists", CoachOnlineExceptionState.AlreadyExist);
                }

                ctx.AffiliateLinks.Add(new Model.AffiliateLink() { CreateDate = DateTime.Now, GeneratedLink = link, UserId = userId, GeneratedToken = hash, ForCoach = true });

                await ctx.SaveChangesAsync();

                return $"{ConfigData.Config.SiteUrl}/ref/{link}";
            }
        }

        public async Task<string> ProposeAffiliateLink(int userId, string proposal)
        {
            using (var ctx = new DataContext())
            {
                proposal = proposal.Trim();
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (proposal.Length < 5 && proposal.Length > 25)
                {
                    throw new CoachOnlineException("Affiliate link must contain at leat 5 characters and maximum 25 characters length", CoachOnlineExceptionState.DataNotValid);
                }

                var matchCharactersPattern = @"^\w+$";

                if (!Regex.IsMatch(proposal, matchCharactersPattern))
                {
                    throw new CoachOnlineException("Affiliate link must contain only from letters, digits and underscore '_'", CoachOnlineExceptionState.DataNotValid);
                }

                var alreadyExists = await ctx.AffiliateLinks.AnyAsync(x => x.GeneratedLink.ToLower() == proposal.ToLower());

                if (alreadyExists)
                {
                    throw new CoachOnlineException("Such affiliate link already exists", CoachOnlineExceptionState.AlreadyExist);
                }

                var hash = await GetNonExistingHash(user.EmailAddress);

                var link = proposal;

                ctx.AffiliateLinks.Add(new Model.AffiliateLink() { CreateDate = DateTime.Now, GeneratedLink = link, UserId = userId, GeneratedToken = hash, ForCoach = false });

                await ctx.SaveChangesAsync();

                return $"{ConfigData.Config.SiteUrl}/ref/{link}";
            }
        }


        public async Task<string> ProposeAffiliateLinkForCoach(int userId, string proposal)
        {
            using (var ctx = new DataContext())
            {
                proposal = proposal.Trim();
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (proposal.Length < 5 && proposal.Length > 25)
                {
                    throw new CoachOnlineException("Affiliate link must contain at leat 5 characters and maximum 25 characters length", CoachOnlineExceptionState.DataNotValid);
                }

                var matchCharactersPattern = @"^\w+$";

                if (!Regex.IsMatch(proposal, matchCharactersPattern))
                {
                    throw new CoachOnlineException("Affiliate link must contain only from letters, digits and underscore '_'", CoachOnlineExceptionState.DataNotValid);
                }

                var alreadyExists = await ctx.AffiliateLinks.AnyAsync(x => x.GeneratedLink.ToLower() == proposal.ToLower());

                if (alreadyExists)
                {
                    throw new CoachOnlineException("Such affiliate link already exists", CoachOnlineExceptionState.AlreadyExist);
                }

                var hash = await GetNonExistingHash(user.EmailAddress);

                var link = proposal;

                ctx.AffiliateLinks.Add(new Model.AffiliateLink() { CreateDate = DateTime.Now, GeneratedLink = link, UserId = userId, GeneratedToken = hash, ForCoach = true });

                await ctx.SaveChangesAsync();

                return $"{ConfigData.Config.SiteUrl}/ref/{link}";
            }
        }

        public async Task UpdateAffiliateLinkOptions(int userId, string link, LinkUpdateOptionsRqs rqs)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserAccountStatus.DELETED);

                var linkDb = await ctx.AffiliateLinks.FirstOrDefaultAsync(t => t.GeneratedLink == link && t.UserId == userId);
                linkDb.CheckExist("Link");
                if (linkDb.ForCoach)
                {
                    rqs.CouponId = null;
                    rqs.WithTrialPlans = false;
                }

                linkDb.LimitedPageView = rqs.LimitedPageView;
                linkDb.ReturnUrl = rqs.ReturnUrl;


                if (user != null && user.AffiliatorType == AffiliateModelType.Regular)
                {
                    linkDb.WithTrialPlans = rqs.WithTrialPlans;
                    linkDb.CouponCode = null;
                }
                else if (user != null && user.AffiliatorType == AffiliateModelType.Influencer && rqs.WithTrialPlans)
                {
                    throw new CoachOnlineException("This affiliation mode does not allow using subscription plans with trial period.", CoachOnlineExceptionState.CantChange);
                }

                if (rqs.CouponId != null)
                {
                    var coupon = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == rqs.CouponId);
                    if (coupon != null)
                    {
                        linkDb.CouponCode = rqs.CouponId;
                    }
                    else
                    {
                        linkDb.CouponCode = null;
                    }

                }
                else
                {
                    linkDb.CouponCode = null;
                }


                await ctx.SaveChangesAsync();
            }
        }


        public async Task<LinkOptsResponse> GetAffiliateLinkWithOptions(int userId, string link)
        {
            using (var ctx = new DataContext())
            {
                var linkDb = await ctx.AffiliateLinks.FirstOrDefaultAsync(t => t.GeneratedLink == link && t.UserId == userId);
                linkDb.CheckExist("Link");

                var resp = new LinkOptsResponse { Link = linkDb.GeneratedLink, LimitedPageView = linkDb.LimitedPageView, ReturnUrl = linkDb.ReturnUrl, WithTrialPlans = linkDb.WithTrialPlans };
                if (linkDb.CouponCode != null)
                {

                    var coupon = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == linkDb.CouponCode);
                    if (coupon != null)
                    {
                        resp.CouponId = linkDb.CouponCode;
                        resp.Coupon = new CouponResponse()
                        {
                            AmountOff = coupon.AmountOff,
                            AvailableForInfluencers = coupon.AvailableForInfluencers,
                            Currency = coupon.Currency,
                            Id = coupon.Id,
                            Name = coupon.Name,
                            Duration = coupon.Duration,
                            DurationInMonths = coupon.DurationInMonths,
                            PercentOff = coupon.PercentOff
                        };
                    }
                }
                return resp;
            }
        }

        public async Task<List<CouponResponse>> GetAvailableCouponsForUser(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if (user.AffiliatorType == AffiliateModelType.Influencer)
                {
                    var data = await _prodManageSvc.GetCouponsForInfluencers();

                    return data;
                }

                return new List<CouponResponse>();
            }
        }


        public async Task<string> GetMyAffiliateLink(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                var link = await ctx.AffiliateLinks.Where(t => t.UserId == userId && t.ForCoach == false).OrderByDescending(t => t.CreateDate).FirstOrDefaultAsync();
                link.CheckExist("Link");
                return $"{ConfigData.Config.SiteUrl}/ref/{link.GeneratedLink}";
            }
        }

        public async Task<string> GetMyAffiliateLinkForCoach(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                var link = await ctx.AffiliateLinks.Where(t => t.UserId == userId && t.ForCoach == true).OrderByDescending(t => t.CreateDate).FirstOrDefaultAsync();
                link.CheckExist("Link");
                return $"{ConfigData.Config.SiteUrl}/ref/{link.GeneratedLink}";
            }
        }

        public async Task SendAffiliateEmailInvitation(int userId, string email)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                var affLink = await ctx.AffiliateLinks.Where(t => t.UserId == user.Id).OrderByDescending(t => t.CreateDate).FirstOrDefaultAsync();
                affLink.CheckExist("Link");

                string body = $"<a href='{Statics.ConfigData.Config.WebUrl}/?Join={affLink.GeneratedToken}'>Join Coachs-Online</a>";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailInvitation.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailInvitation.html");
                    body = body.Replace("###USER_INFO###", $"{user.FirstName} {user.Surname} ({user.EmailAddress})");
                    body = body.Replace("###INVITATIONURL###", $"{Statics.ConfigData.Config.WebUrl}/auth/register/student?Join={affLink.GeneratedToken}");

                }


                await _emailSvc.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = email,
                    ReceiverName = "",
                    Topic = "Vous avez une invitation à rejoindre Coachs-Online"
                });
            }
        }


        private async Task<List<AffiliatesTree>> GenerateAffiliatesTree()
        {
            List<AffiliatesTree> hostAffiliates = new List<AffiliatesTree>();
            using (var ctx = new DataContext())
            {
                var affiliates = await ctx.Affiliates.ToListAsync();

                var grouppedByHost = affiliates.GroupBy(t => t.HostUserId);



                foreach (var p in grouppedByHost)
                {
                    AffiliatesTree tree = new AffiliatesTree();
                    tree.HostUserId = p.Key;
                    tree.AffiliateChildren = new List<AffiliateChild>();

                    p.ToList().ForEach(x =>
                    {
                        tree.AffiliateChildren.Add(new AffiliateChild() { AffiliateUserId = x.AffiliateUserId, IsFirstGeneration = true, JoinDate = x.CreationDate, IsCoach = x.IsAffiliateACoach, AffiliateModelType = x.AffiliateModelType, DirectHostId = p.Key });

                        //find up to 2nd generation
                        var secondGeneration = affiliates.Where(t => t.HostUserId == x.AffiliateUserId).ToList();
                        secondGeneration.ForEach(s =>
                        {
                            if (!s.IsAffiliateACoach)
                            {
                                tree.AffiliateChildren.Add(new AffiliateChild() { AffiliateUserId = s.AffiliateUserId, IsFirstGeneration = false, JoinDate = s.CreationDate, AffiliateModelType = x.AffiliateModelType, DirectHostId = s.HostUserId });
                            }
                        });
                    });

                    hostAffiliates.Add(tree);
                }
            }

            return hostAffiliates;
        }

        private async Task<List<AffiliatesTree>> GenerateAffiliatesTreeForUser(int hostId)
        {
            List<AffiliatesTree> hostAffiliates = new List<AffiliatesTree>();
            using (var ctx = new DataContext())
            {
                var affiliates = await ctx.Affiliates.ToListAsync();

                var grouppedByHost = affiliates.Where(t => t.HostUserId == hostId).GroupBy(t => t.HostUserId);

                //Console.WriteLine($"Affiliates contains:{grouppedByHost.Count()} records for user {hostId}");

                foreach (var p in grouppedByHost)
                {
                    AffiliatesTree tree = new AffiliatesTree();
                    tree.HostUserId = p.Key;
                    tree.AffiliateChildren = new List<AffiliateChild>();

                    //Console.WriteLine($"Direct affiliates:{p.Count()}");

                    p.ToList().ForEach(x =>
                    {
                        tree.AffiliateChildren.Add(new AffiliateChild() { AffiliateUserId = x.AffiliateUserId, IsFirstGeneration = true, JoinDate = x.CreationDate, IsCoach = x.IsAffiliateACoach, AffiliateModelType = x.AffiliateModelType });

                        //find up to 2nd generation
                        var secondGeneration = affiliates.Where(t => t.HostUserId == x.AffiliateUserId).ToList();
                        secondGeneration.ForEach(s =>
                        {
                            if (!s.IsAffiliateACoach)
                            {
                                tree.AffiliateChildren.Add(new AffiliateChild() { AffiliateUserId = s.AffiliateUserId, IsFirstGeneration = false, JoinDate = s.CreationDate, AffiliateModelType = x.AffiliateModelType });
                            }
                        });
                    });

                    hostAffiliates.Add(tree);
                }
            }

            return hostAffiliates;
        }

        public async Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesGeneral(int userId)
        {
            var data = new List<AffiliateHostPaymentsAPI>();
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                var general = await ctx.AffiliatePayments.Where(t => t.HostId == userId).ToListAsync();

                var grouppedByCurrency = general.GroupBy(c => c.PaymentCurrency).ToList();

                foreach (var g in grouppedByCurrency)
                {
                    var itm = new AffiliateHostPaymentsAPI();
                    itm.UserId = userId;
                    itm.Total = g.Sum(t => t.PaymentValue);
                    itm.Withdrawn = g.Where(t => t.Transferred || t.PayPalPayoutId != null).Sum(t => t.PaymentValue);
                    itm.ToWithdraw = g.Where(t => !t.Transferred && t.PayPalPayoutId == null).Sum(t => t.PaymentValue);
                    itm.Currency = g.Key;

                    data.Add(itm);
                }

            }

            return data;
        }

        private async Task<string> GetUserSubName(int userId)
        {
            try
            {
                using (var ctx = new DataContext())
                {
                    var sub = await ctx.UserBillingPlans.Where(t => t.UserId == userId && t.Status == BillingPlanStatus.ACTIVE).Include(x => x.BillingPlanType).OrderByDescending(t => t.Id).FirstOrDefaultAsync();

                    if (sub != null)
                    {
                        return sub.BillingPlanType.Name;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return "";
        }

        private async Task<AffilationStatisticsHostResponse> GetHostData(int userId)
        {
            AffilationStatisticsHostResponse hostResp = new AffilationStatisticsHostResponse();
            using (var ctx = new DataContext())
            {
                var host = await ctx.users.Where(t => t.Id == userId && t.Status != UserAccountStatus.DELETED).FirstOrDefaultAsync();
                if (host != null)
                {
                    var today = DateTime.Today;
                    DateTime thisMonth = new DateTime(today.Year, today.Month, 1);
                    hostResp.Email = host.EmailAddress;
                    hostResp.Id = host.Id;
                    hostResp.LastName = host.Surname;
                    hostResp.FirstName = host.FirstName;
                    hostResp.UserRole = host.UserRole.ToString();
                    hostResp.RegistrationDate = host.AccountCreationDate.HasValue ? host.AccountCreationDate : null;
                    var hostEarnings = await ctx.AffiliatePayments.Where(t => t.HostId == host.Id).ToListAsync();
                    hostResp.TotalEarnings = hostEarnings.Sum(x => x.PaymentValue);
                    hostResp.TotalEarningsThisMonth = hostEarnings.Where(t => t.PaymentCreationDate >= thisMonth).Sum(x => x.PaymentValue);
                    hostResp.TotalEarningsLast3Months = hostEarnings.Where(t => t.PaymentCreationDate >= thisMonth.AddMonths(-2)).Sum(x => x.PaymentValue);
                    hostResp.Currency = hostEarnings.FirstOrDefault()?.PaymentCurrency;

                    hostResp.ChosenPlan = "";
                    if (host.UserRole == UserRoleType.STUDENT)
                    {

                        hostResp.ChosenPlan = await GetUserSubName(host.Id);

                    }

                    var isAffiliate = await ctx.Affiliates.Where(x => x.AffiliateUserId == host.Id).FirstOrDefaultAsync();
                    if (isAffiliate != null)
                    {
                        hostResp.Parent = await GetHostData(isAffiliate.HostUserId);
                    }

                    hostResp.Affiliates = new List<AffiliateAPI>();

                    var affiliates = await ctx.Affiliates.Where(x => x.HostUserId == host.Id).ToListAsync();

                    var mainHost = new HostUserInfoAPI() { Email = host.EmailAddress, FirstName = host.FirstName, LastName = host.Surname, UserId = host.Id, UserRole = host.UserRole.ToString() };
                    foreach (var aff in affiliates)
                    {

                        var affChild = await GetAffiliate(aff.AffiliateUserId, aff.CreationDate, mainHost, true);
                        if (affChild != null)
                        {
                            hostResp.Affiliates.Add(affChild);

                            if (!aff.IsAffiliateACoach)
                            {
                                var affHost = await ctx.Affiliates.Where(x => x.HostUserId == aff.AffiliateUserId).ToListAsync();

                                var childHost = new HostUserInfoAPI() { Email = affChild.Email, FirstName = affChild.FirstName, LastName = affChild.LastName, UserId = affChild.UserId, UserRole = affChild.UserRole };
                                foreach (var child in affHost)
                                {
                                    var affSecondChild = await GetAffiliate(child.AffiliateUserId, child.CreationDate, childHost, false);
                                    if (affSecondChild != null)
                                    {
                                        hostResp.Affiliates.Add(affSecondChild);
                                    }

                                }
                            }
                        }

                    }
                    hostResp.AffiliatesTotal = hostResp.Affiliates.Count;
                    hostResp.AffiliatesFirstLine = hostResp.Affiliates.Where(x => x.IsDirect).Count();
                    hostResp.AffiliatesSecondLine = hostResp.Affiliates.Where(x => !x.IsDirect).Count();
                    hostResp.AffiliatesCoaches = hostResp.Affiliates.Where(x => x.Type == "Coach").Count();
                    hostResp.AffiliatesSubscribers = hostResp.Affiliates.Where(x => x.Type == "Subscriber").Count();

                }
                else
                {
                    return null;
                }
            }

            return hostResp;
        }

        private async Task<AffiliateAPI> GetAffiliate(int affiliateId, DateTime createDate, HostUserInfoAPI host, bool direct)
        {
            AffiliateAPI affChild = new AffiliateAPI();

            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(x => x.Id == affiliateId && x.Status != UserAccountStatus.DELETED);
                if (usr != null)
                {
                    affChild.Email = usr.EmailAddress;
                    affChild.FirstName = usr.FirstName;
                    affChild.LastName = usr.Surname;
                    affChild.IsDirect = direct;
                    affChild.UserRole = usr.UserRole.ToString();
                    affChild.UserId = usr.Id;
                    affChild.ChosenPlan = await GetUserSubName(usr.Id);
                    var earnings = await ctx.AffiliatePayments.Where(t => t.HostId == host.UserId && t.AffiliateId == usr.Id).ToListAsync();
                    affChild.EarnedMoney = earnings.Sum(x => x.PaymentValue);
                    affChild.Currency = earnings.FirstOrDefault()?.PaymentCurrency;
                    affChild.JoinDate = createDate;
                    affChild.Host = host;
                    affChild.Type = usr.UserRole == UserRoleType.COACH ? "Coach" : "Subscriber";
                }
                else
                {
                    return null;
                }
            }

            return affChild;
        }


        public async Task<AffilationStatisticsResponse> GetAffiliateStats()
        {
            AffilationStatisticsResponse resp = new AffilationStatisticsResponse();
            using (var ctx = new DataContext())
            {
                //var today = DateTime.Today;
                //DateTime thisMonth = new DateTime(today.Year, today.Month, 1);
                var payments = await ctx.AffiliatePayments.ToListAsync();
                resp.TotalEarnings = payments.Sum(x => x.PaymentValue);
                resp.Hosts = new List<AffilationStatisticsHostResponse>();
                resp.Currency = payments.FirstOrDefault()?.PaymentCurrency;
                // resp.TotalEarningsLast3Months = payments.Where(t => t.PaymentCreationDate >= thisMonth.AddMonths(-2)).Sum(x => x.PaymentValue);
                // resp.TotalEarningsThisMonth = payments.Where(t => t.PaymentCreationDate >= thisMonth).Sum(x => x.PaymentValue);
                resp.PlatformEarningsFromAffilation = await GetPlatformSubscriptionEarningsFromAffiliation();
                var allAffiliates = await ctx.Affiliates.ToListAsync();
                var grouppedByHost = allAffiliates.GroupBy(t => t.HostUserId);

                foreach (var aff in grouppedByHost)
                {
                    var host = await GetHostData(aff.Key);
                    if (host != null)
                    {
                        resp.Hosts.Add(host);
                    }
                }

                resp.AffiliationUsersTotal = resp.Hosts.Sum(a => a.AffiliatesFirstLine);
            }

            return resp;
        }

        private async Task<decimal> GetPlatformSubscriptionEarningsFromAffiliation()
        {
            decimal value = 0;
            using (var ctx = new DataContext())
            {
                var affUsers = await ctx.Affiliates.ToListAsync();
                foreach (var aff in affUsers)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == aff.AffiliateUserId);

                    //if(!string.IsNullOrEmpty(user.StripeCustomerId))
                    //{
                    //    var paymentSvc = new Stripe.PaymentIntentService();
                    //    var payments = await paymentSvc.ListAsync(new Stripe.PaymentIntentListOptions() { Customer = user.StripeCustomerId });

                    //    foreach(var payment in payments)
                    //    {
                    //        if(payment.Status == "succeeded")
                    //        {
                    //            value += (decimal)payment.Amount / 100m;
                    //        }
                    //    }
                    //}

                    if (user != null)
                    {
                        var subs = await ctx.UserBillingPlans.Where(t => t.UserId == user.Id && t.StripeSubscriptionId != null).ToListAsync();
                        var invSvc = new Stripe.InvoiceService();
                        foreach (var sub in subs)
                        {
                            var invoices = await invSvc.ListAsync(new Stripe.InvoiceListOptions() { Subscription = sub.StripeSubscriptionId });
                            foreach (var inv in invoices)
                            {
                                if (inv.Status == "paid")
                                {
                                    value += Math.Round((decimal)inv.AmountPaid / 100m, 2);
                                }
                            }
                        }
                    }
                }
            }

            return value;
        }

        public async Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesForMonth(int userId, int month, int year)
        {
            var data = new List<AffiliateHostPaymentsAPI>();
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                DateTime dt = new DateTime(year, month, 1);
                var general = await ctx.AffiliatePayments.Where(t => t.HostId == userId && t.PaymentForMonth == dt).ToListAsync();

                var grouppedByCurrency = general.GroupBy(c => c.PaymentCurrency).ToList();

                foreach (var g in grouppedByCurrency)
                {
                    var itm = new AffiliateHostPaymentsAPI();
                    itm.UserId = userId;
                    itm.Total = g.Sum(t => t.PaymentValue);
                    itm.Withdrawn = g.Where(t => t.Transferred).Sum(t => t.PaymentValue);
                    itm.ToWithdraw = g.Where(t => !t.Transferred).Sum(t => t.PaymentValue);
                    itm.Currency = g.Key;

                    data.Add(itm);
                }

            }

            return data;
        }


        public async Task<List<AffiliateHostPaymentsAPI>> GetEarnedMoneyfromAffiliatesBetweenDates(int userId, DateTime start, DateTime end)
        {
            var data = new List<AffiliateHostPaymentsAPI>();
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                var general = await ctx.AffiliatePayments.Where(t => t.HostId == userId && t.PaymentCreationDate.Date >= start.Date && t.PaymentCreationDate <= end.Date).ToListAsync();

                var grouppedByCurrency = general.GroupBy(c => c.PaymentCurrency).ToList();

                foreach (var g in grouppedByCurrency)
                {
                    var itm = new AffiliateHostPaymentsAPI();
                    itm.UserId = userId;
                    itm.Total = g.Sum(t => t.PaymentValue);
                    itm.Withdrawn = g.Where(t => t.Transferred).Sum(t => t.PaymentValue);
                    itm.ToWithdraw = g.Where(t => !t.Transferred).Sum(t => t.PaymentValue);
                    itm.Currency = g.Key;

                    data.Add(itm);
                }

            }

            return data;
        }

        private async Task<Tuple<decimal, string>> CheckActualPrice(string stripeSubId)
        {
            var tuple = new Tuple<decimal, string>(0, "eur");
            decimal price = 0;

            var invSvc = new Stripe.InvoiceService();
            try
            {
                var invoices = await invSvc.ListAsync(new Stripe.InvoiceListOptions() { Subscription = stripeSubId, Status = "paid" });

                if (invoices.Data.Count > 0)
                {
                    var latestInvoice = invoices.OrderByDescending(t => t.Created).First();

                    price = (decimal)latestInvoice.AmountPaid / 100m;
                    tuple = new Tuple<decimal, string>(price, latestInvoice.Currency);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return tuple;
        }

        public async Task<AffiliateSubscriptionStatus> CheckUserSubscription(int userId)
        {
            AffiliateSubscriptionStatus status = new AffiliateSubscriptionStatus();
            status.UserId = userId;
            status.SubscriptionIsActive = false;

            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.Where(t => t.Id == userId).Include(u => u.UserBillingPlans).ThenInclude(p => p.BillingPlanType).ThenInclude(p => p.Price).FirstOrDefaultAsync();
                if (usr != null && usr.StripeCustomerId != null)
                {
                    if (usr.UserRole == Model.UserRoleType.COACH)
                    {
                        status.IsCoach = true;
                        status.SubscriptionIsActive = false;
                    }
                    else
                    {
                        status.UserStripeId = usr.StripeCustomerId;
                        var ubp = usr.UserBillingPlans.Where(t => t.Status == Model.BillingPlanStatus.ACTIVE).FirstOrDefault();
                        if (ubp != null)
                        {
                            var cancelledSubs = usr.UserBillingPlans.Where(t => t.Status == BillingPlanStatus.CANCELLED && t.ActivationDate.HasValue && t.ExpiryDate.HasValue).ToList();

                            var actualLatestPaidAmount = await CheckActualPrice(ubp.StripeSubscriptionId);
                            var startTimeContinued = CheckConsistencyWithSubscriptions(ubp, cancelledSubs);
                            status.SubscriptionId = ubp.Id;
                            status.StripeSubscriptionId = ubp.StripeSubscriptionId;
                            status.SubscriptionIsActive = true;
                            status.Currency = actualLatestPaidAmount.Item2; //ubp.BillingPlanType.Currency;
                                                                            // status.SubscriptionPrice = ubp.BillingPlanType.AmountPerMonth.HasValue? ubp.BillingPlanType.AmountPerMonth.Value : 0;

                            status.SubscriptionPeriodMths = ubp.BillingPlanType.Price.PeriodType == "month" ? 1 : ubp.BillingPlanType.Price.PeriodType == "year" ? 12 : 0;
                            if (status.SubscriptionPeriodMths == 12)
                            {
                                status.SubscriptionPrice = Math.Round(actualLatestPaidAmount.Item1 / 12m, 2);
                            }
                            else
                            {
                                status.SubscriptionPrice = actualLatestPaidAmount.Item1;
                            }
                            status.ActualActiveFromDate = ubp.ActivationDate;
                            status.ExpiryDate = ubp.ExpiryDate;
                            status.SubscriptionName = ubp.BillingPlanType.Name;
                            status.IsCoach = false;
                            status.ActiveFromDate = startTimeContinued.HasValue ? startTimeContinued.Value : ubp.ActivationDate;

                            if (ubp.QuestionaaireCancelReason.HasValue)
                            {
                                var response = await ctx.QuestionnaireAnswers.Where(x => x.Id == ubp.QuestionaaireCancelReason.Value).Include(r => r.Response).FirstOrDefaultAsync();
                                if (response != null && response.Response != null)
                                {
                                    status.SubCancellationReason = response.Response.IsOtherOption ? response.OtherResponse : response.Response.Option;
                                }
                            }
                        }
                        else
                        {
                            var ubpCancelled = usr.UserBillingPlans.OrderByDescending(x => x.Id).FirstOrDefault();
                            if (ubpCancelled != null)
                            {
                                status.SubscriptionName = ubpCancelled.BillingPlanType.Name;
                                status.IsCoach = false;
                                if (ubpCancelled.QuestionaaireCancelReason.HasValue)
                                {
                                    var response = await ctx.QuestionnaireAnswers.Where(x => x.Id == ubpCancelled.QuestionaaireCancelReason.Value).Include(r => r.Response).FirstOrDefaultAsync();
                                    if (response != null && response.Response != null)
                                    {
                                        status.SubCancellationReason = response.Response.IsOtherOption ? response.OtherResponse : response.Response.Option;
                                    }
                                }
                            }
                        }
                    }
                }

                return status;
            }
        }

        private DateTime? CheckConsistencyWithSubscriptions(UserBillingPlan active, List<UserBillingPlan> cancelled)
        {
            if (cancelled == null || !cancelled.Any())
            {
                return null;
            }
            cancelled = cancelled.OrderBy(t => t.ActivationDate).ToList();
            DateTime? firstSubStart = null;
            DateTime? firtsSubEnd = null;
            DateTime? lastSubEnd = null;

            foreach (var s in cancelled)
            {
                if (!firstSubStart.HasValue && !firtsSubEnd.HasValue)
                {
                    //1st iteration
                    firstSubStart = s.ActivationDate.Value;
                    firtsSubEnd = s.ExpiryDate.Value;
                }
                else
                {
                    //next iteration
                    if (s.ActivationDate.Value.Date <= firtsSubEnd.Value.Date.AddDays(1))
                    {
                        //leave old dates stand, its continuing
                    }
                    else
                    {
                        firstSubStart = s.ActivationDate.Value;
                        firtsSubEnd = s.ExpiryDate.Value;
                    }
                }

                lastSubEnd = s.ExpiryDate.Value;
            }

            if (lastSubEnd.HasValue && active.ActivationDate.Value.Date <= lastSubEnd.Value.Date.AddDays(1))
            {
                return firstSubStart;
            }
            else
            {
                return null;
            }
        }

        public async Task CheckAffiliatePayments()
        {
            var affTree = await GenerateAffiliatesTree();

            foreach (var a in affTree)
            {
                //if(a.HostUserId == 307)
                //{
                //    Console.WriteLine("");
                //}

                var hostSubscription = await CheckUserSubscription(a.HostUserId);
                //if(hostSubscription.SubscriptionIsActive || hostSubscription.IsCoach)
                //{
                foreach (var u in a.AffiliateChildren)
                {
                    if (u.AffiliateModelType == AffiliateModelType.Regular)
                    {
                        var affiliateSubscription = await CheckUserSubscription(u.AffiliateUserId);

                        if (affiliateSubscription.SubscriptionIsActive || affiliateSubscription.IsCoach)
                        {
                            //tutaj cale obliczenia, inaczej nie naliczamy
                            await CalculateAffiliatePayment(hostSubscription, affiliateSubscription, u.IsFirstGeneration, u.JoinDate);

                        }
                    }
                    else if (u.AffiliateModelType == AffiliateModelType.Influencer)
                    {
                        var affiliateSubscription = await CheckUserSubscription(u.AffiliateUserId);

                        if (affiliateSubscription.SubscriptionIsActive)
                        {
                            //tutaj cale obliczenia, inaczej nie naliczamy
                            await CalculateAffiliatePaymentForModel2(hostSubscription, affiliateSubscription, u.IsFirstGeneration, u.JoinDate, u.DirectHostId);

                        }
                    }
                }
                //}
            }

        }


        public async Task<List<AffiliateAPI>> GetMyAffiliates(int userId)
        {
            var data = new List<AffiliateAPI>();
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId && t.Status != UserAccountStatus.DELETED);
                usr.CheckExist("User");

                var affTree = await GenerateAffiliatesTreeForUser(userId);

                var dictClass = new AffiliateTypesDictionary();



                foreach (var a in affTree)
                {

                    foreach (var aff in a.AffiliateChildren)
                    {
                        //name, email, chosen plan, surname
                        Console.WriteLine($"Affiliate: {aff.AffiliateUserId}");
                        var item = await GetAffiliateAPI(aff.AffiliateUserId, userId, aff.IsFirstGeneration, aff.JoinDate, dictClass.Dictionary, aff.AffiliateModelType, null);
                        if (item != null)
                        {
                            data.Add(item);
                        }
                    }
                }
            }

            data = data.OrderByDescending(x => x.ChosenPlan).ThenBy(x => x.JoinDate).ToList();

            return data;
        }

        private async Task<DateTime?> GetNextPotentialPaymentDateForAffiliate(int mthPeriod, bool isDirect, int affiliateId, DateTime joinDate, bool isCoach, AffiliateModelType modelType)
        {
            using (var ctx = new DataContext())
            {
                var payments = await ctx.AffiliatePayments.Where(x => x.AffiliateId == affiliateId).OrderByDescending(x => x.PaymentCreationDate).ToListAsync();

                if (payments.Any())
                {
                    var lastPayment = payments.First();

                    if (modelType == AffiliateModelType.Regular)
                    {
                        return lastPayment.PaymentCreationDate.AddMonths(1).Date;
                    }
                    else
                    {
                        var nextPaymentDate = lastPayment.NextPlannedPaymentDate.HasValue ? lastPayment.NextPlannedPaymentDate.Value : lastPayment.PaymentCreationDate.AddMonths(1);

                        return nextPaymentDate.Date;
                    }
                }
                else
                {
                    if (isDirect)
                    {
                        if (isCoach)
                        {
                            //DateTime joinDateMth = new DateTime(joinDate.Year, joinDate.Month, 1);
                            DateTime currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                            return currentMonth.AddMonths(1);

                        }
                        else
                        {
                            if (modelType == AffiliateModelType.Influencer)
                            {
                                if (mthPeriod == 12)
                                {
                                    return joinDate.Date;
                                }
                                else if (mthPeriod == 1)
                                {
                                    return joinDate.AddMonths(1).Date;
                                }
                            }
                            else
                            {
                                //if (mthPeriod == 12)
                                //{
                                //   return joinDate.AddMonths(1).Date;
                                // }
                                //else if (mthPeriod == 1)
                                //{
                                return joinDate.AddMonths(1).Date;
                                //}
                            }
                        }
                    }
                    else
                    {
                        if (modelType == AffiliateModelType.Influencer)
                        {
                            return joinDate.AddMonths(1).Date;
                        }
                        else
                        {
                            return joinDate.AddMonths(3).Date;
                        }
                    }
                }
                return null;
            }
        }

        private decimal GetPotentialYearlyIncome(int mthPeriod, decimal price, bool isDirect, AffiliateModelType modelType)
        {
            if (modelType == AffiliateModelType.Influencer)
            {
                if (isDirect)
                {
                    decimal retVal = 0;
                    if (mthPeriod == 12)
                    {
                        retVal = (price * 0.7m) + (price * 11 * 0.2m);
                    }
                    else
                    {
                        retVal = (price * 0.7m) + (price * 0.2m) + (price * 10 * 0.2m);
                    }
                    return retVal;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (mthPeriod == 12)
                {
                    if (isDirect)
                    {
                        return (price * 0.7m) + (price * 0.2m * 11);
                    }
                    else
                    {
                        return price * 0.05m * 8;
                    }
                }
                else
                {
                    if (isDirect)
                    {
                        return (price * 0.7m) + (price * 0.2m * 8);
                    }
                    else
                    {
                        return price * 0.05m * 8;
                    }
                }
            }

        }

        public async Task<AffiliateHostsRankingPagesResponse> GetAffiliateHostsRanking(HostsRankingType type, int page = 1, int? userId = null)
        {

            if (page < 1)
            {
                throw new CoachOnlineException("Wrong page number. Minimum page no is 1.", CoachOnlineExceptionState.DataNotValid);
            }
            var data = await GetHostsDataForRanking();

            if (type == HostsRankingType.SUBSCRIBER)
            {
                data = data.OrderByDescending(x => x.TotalSubscribersEarnings).ThenByDescending(x => x.AffiliateUsersTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }
            else if (type == HostsRankingType.COACH)
            {
                data = data.OrderByDescending(x => x.TotalCoachesEarnings).ThenByDescending(x => x.AffiliateCoachesTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }
            else
            {
                data = data.OrderByDescending(x => x.TotalEarnings).ThenByDescending(x => x.AffiliateSubscribersTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }

            int rank = 1;
            bool isInTop10 = false;
            foreach (var d in data)
            {
                if (userId.HasValue && userId.Value == d.HostId)
                {
                    d.IsCurrentUser = true;
                    if (rank <= 10)
                    {
                        isInTop10 = true;
                    }
                }
                d.RankId = rank;
                rank++;
            }
            var x = (decimal)data.Count / 10;
            var modulo = data.Count % 10;

            int pages = (int)x + (modulo > 0 ? 1 : 0);

            if (page > pages)
            {
                throw new CoachOnlineException("Page does not exist", CoachOnlineExceptionState.DataNotValid);
            }

            AffiliateHostsRankingPagesResponse respData = new AffiliateHostsRankingPagesResponse();
            respData.TotalRecordsCount = data.Count;
            respData.PageNo = page;
            respData.PagesCount = pages;
            respData.Data = data.Skip((page - 1) * 10).Take(10).ToList();
            respData.PageRecordsCount = respData.Data.Count;

            return respData;
        }

        public async Task<List<AffiliateHostsRankingResponse>> GetAffiliateHostsRanking(HostsRankingType type, bool topTen, int? userId = null)
        {
            var data = await GetHostsDataForRanking();

            if (type == HostsRankingType.SUBSCRIBER)
            {
                data = data.OrderByDescending(x => x.TotalSubscribersEarnings).ThenByDescending(x => x.AffiliateUsersTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }
            else if (type == HostsRankingType.COACH)
            {
                data = data.OrderByDescending(x => x.TotalCoachesEarnings).ThenByDescending(x => x.AffiliateCoachesTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }
            else
            {
                data = data.OrderByDescending(x => x.TotalEarnings).ThenByDescending(x => x.AffiliateSubscribersTotal).ThenBy(x => x.AffiliationStartDate).ToList();
            }

            int rank = 1;
            bool isInTop10 = false;
            foreach (var d in data)
            {
                if (userId.HasValue && userId.Value == d.HostId)
                {
                    d.IsCurrentUser = true;
                    if (rank <= 10)
                    {
                        isInTop10 = true;
                    }
                }
                d.RankId = rank;
                rank++;
            }


            if (topTen && userId.HasValue)
            {
                if (isInTop10)
                {
                    return data.Take(10).ToList();
                }
                else
                {
                    var temp = data.FirstOrDefault(t => t.IsCurrentUser);

                    var tempList = data.Take(10).ToList();

                    tempList.Add(temp);

                    return tempList;
                }
            }
            else if (topTen)
            {
                return data.Take(10).ToList();
            }
            else
            {
                return data;
            }
        }

        public enum HostsRankingType : int
        {
            FULL,
            COACH,
            SUBSCRIBER
        }

        public async Task ChangeAffiliationModelForUser(int userId, AffiliateModelType affType)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId && t.Status != UserAccountStatus.DELETED);

                user.CheckExist("User");

                if (user.UserRole == UserRoleType.INSTITUTION_STUDENT)
                {
                    throw new CoachOnlineException("User is an institution student. Cannot change affiliation model type.", CoachOnlineExceptionState.DataNotValid);
                }

                user.AffiliatorType = affType;

                await ctx.SaveChangesAsync();
            }
        }


        private async Task<List<AffiliateHostsRankingResponse>> GetHostsDataForRanking()
        {
            var affTree = await GenerateAffiliatesTree();

            List<AffiliateHostsRankingResponse> ranking = new List<AffiliateHostsRankingResponse>();

            using (var ctx = new DataContext())
            {
                foreach (var affData in affTree)
                {

                    var host = await ctx.users.FirstOrDefaultAsync(x => x.Id == affData.HostUserId && x.Status != UserAccountStatus.DELETED);
                    if (host != null)
                    {
                        AffiliateHostsRankingResponse rankData = new AffiliateHostsRankingResponse();

                        rankData.HostId = affData.HostUserId;
                        rankData.FirstName = host.FirstName;
                        rankData.LastName = host.Surname;
                        rankData.UserRole = host.UserRole.ToString();
                        rankData.Email = host.EmailAddress;
                        rankData.AffiliateUsersTotal = affData.AffiliateChildren.Count;
                        rankData.AffiliateSubscribersTotal = affData.AffiliateChildren.Where(x => !x.IsCoach).Count();
                        rankData.AffiliateCoachesTotal = affData.AffiliateChildren.Where(x => x.IsCoach).Count();
                        rankData.SecondLineSubscribersTotal = affData.AffiliateChildren.Where(x => !x.IsFirstGeneration).Count();
                        rankData.AffiliationStartDate = (await ctx.Affiliates.Where(x => x.HostUserId == affData.HostUserId).OrderBy(x => x.CreationDate).FirstOrDefaultAsync())?.CreationDate;
                        var affPayments = await ctx.AffiliatePayments.Where(x => x.HostId == affData.HostUserId).ToListAsync();
                        if (affPayments.Any())
                        {
                            rankData.Currency = affPayments.First().PaymentCurrency;
                            rankData.TotalEarnings = affPayments.Sum(x => x.PaymentValue);
                            rankData.TotalSubscribersEarnings = affPayments.Where(x => !x.IsAffiliateCoach).Sum(x => x.PaymentValue);
                            rankData.TotalCoachesEarnings = affPayments.Where(x => x.IsAffiliateCoach).Sum(x => x.PaymentValue);
                        }

                        ranking.Add(rankData);
                    }
                }
            }

            return ranking;
        }

        private async Task<AffiliateAPI> GetAffiliateAPI(int affiliateId, int hostId, bool isFirstGen, DateTime joinDate, Dictionary<AffiliateNotifType, RetModel> dictionary, AffiliateModelType modelType, int? indirectHost)
        {
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == affiliateId && t.Status != UserAccountStatus.DELETED);
                if (usr != null)
                {
                    var sub = await CheckUserSubscription(affiliateId);
                    AffiliateAPI api = new AffiliateAPI();
                    api.FirstName = usr.FirstName;
                    api.LastName = usr.Surname;
                    api.UserId = usr.Id;
                    api.Email = usr.EmailAddress;
                    api.IsDirect = isFirstGen;
                    api.UserRole = usr.UserRole.ToString();
                    api.Type = usr.UserRole == UserRoleType.COACH ? "Coach" : "Subscriber";
                    api.AffiliatorType = modelType;
                    if (api.Type != "Coach" && sub != null && sub.SubscriptionIsActive)
                    {
                        api.ChosenPlan = sub.SubscriptionName;
                        api.PotentialNextPaymentDate = await GetNextPotentialPaymentDateForAffiliate(sub.SubscriptionPeriodMths, isFirstGen, usr.Id, joinDate, false, modelType);
                        api.PotentialYearlyIncome = Math.Round(GetPotentialYearlyIncome(sub.SubscriptionPeriodMths, sub.SubscriptionPrice, isFirstGen, modelType), 2, MidpointRounding.ToZero);

                        if (modelType == AffiliateModelType.Influencer && indirectHost.HasValue)
                        {
                            var paymentLtst = await ctx.AffiliatePayments.Where(x => x.AffiliateId == affiliateId && x.HostId == hostId).OrderByDescending(x => x.PaymentCreationDate).FirstOrDefaultAsync();
                            if (paymentLtst != null)
                            {
                                if (paymentLtst.FullYearPayment)
                                {
                                    api.PotentialYearlyIncome = Math.Round(paymentLtst.PaymentValue * 0.05m, 2);
                                }
                                else
                                {
                                    api.PotentialYearlyIncome = Math.Round(paymentLtst.PaymentValue * 0.05m * 12, 2);
                                }
                            }
                        }
                    }
                    if (sub != null && !string.IsNullOrEmpty(sub.SubCancellationReason))
                    {
                        api.SubCancellationReason = sub.SubCancellationReason;
                    }

                    if (api.Type == "Coach")
                    {
                        api.PotentialNextPaymentDate = await GetNextPotentialPaymentDateForAffiliate(1, true, usr.Id, joinDate, true, modelType);
                    }
                    api.JoinDate = joinDate;

                    List<AffiliatePayment> payments = new List<AffiliatePayment>();
                    if (indirectHost.HasValue)
                    {
                        payments = await ctx.AffiliatePayments.Where(t => t.AffiliateId == affiliateId && t.HostId == indirectHost.Value).ToListAsync();
                    }
                    else
                    {
                        payments = await ctx.AffiliatePayments.Where(t => t.AffiliateId == affiliateId && t.HostId == hostId).ToListAsync();
                    }


                    api.EarnedMoney = payments.Sum(t => t.PaymentValue);
                    api.Currency = payments.FirstOrDefault()?.PaymentCurrency;
                    api.Host = new HostUserInfoAPI();
                    var hostUser = await ctx.users.Where(t => t.Id == hostId && t.Status != UserAccountStatus.DELETED).FirstOrDefaultAsync();
                    api.Host.UserId = hostId;
                    if (hostUser != null)
                    {
                        api.Host.Email = hostUser.EmailAddress;
                        api.Host.LastName = hostUser.Surname;
                        api.Host.FirstName = hostUser.FirstName;
                        api.Host.UserRole = hostUser.UserRole.ToString();
                    }

                    if (sub == null || !sub.SubscriptionIsActive)
                    {
                        api.TooltipData = dictionary[AffiliateNotifType.NoSub];
                    }
                    else if (usr.UserRole == UserRoleType.COACH)
                    {
                        api.TooltipData = dictionary[AffiliateNotifType.FirstLineCoach];
                    }
                    else if (isFirstGen)
                    {
                        if (modelType == AffiliateModelType.Regular)
                        {
                            api.TooltipData = sub.SubscriptionPeriodMths == 12 ? dictionary[AffiliateNotifType.YearlyFirstLineStudentRegular] : dictionary[AffiliateNotifType.MonthlyFirstLineStudentRegular];
                        }
                        else
                        {
                            api.TooltipData = sub.SubscriptionPeriodMths == 12 ? dictionary[AffiliateNotifType.YearlyFirstLineStudentInfluencer] : dictionary[AffiliateNotifType.MonthlyFirstLineStudentInfluencer];
                        }
                    }
                    else
                    {
                        //second gen
                        if (modelType == AffiliateModelType.Regular)
                        {
                            api.TooltipData = dictionary[AffiliateNotifType.SecondLineStudentRegular];
                        }
                        else
                        {
                            api.TooltipData = dictionary[AffiliateNotifType.SecondLineStudentInfluencer];
                        }
                    }


                    if (isFirstGen)
                    {
                        api.Affiliates = new List<AffiliateAPI>();
                        var childAffs = await ctx.Affiliates.Where(x => x.HostUserId == usr.Id).ToListAsync();
                        foreach (var c in childAffs)
                        {
                            if (!c.IsAffiliateACoach)
                            {
                                var item = await GetAffiliateAPI(c.AffiliateUserId, usr.Id, false, c.CreationDate, dictionary, c.AffiliateModelType, hostId);
                                if (item != null)
                                {
                                    api.Affiliates.Add(item);
                                }
                            }

                        }
                    }

                    return api;
                }

                return null;
            }
        }


        public async Task CheckPaymentStatuses()
        {
            try
            {
                using (var ctx = new DataContext())
                {
                    var checkTransactionStatus = await ctx.AffiliatePayments.Where(t => !t.Transferred && t.PayPalPayoutId != null).ToListAsync();

                    foreach (var tr in checkTransactionStatus)
                    {
                        PayPalPayoutResponse paymentstatus = null;

                        paymentstatus = await _payPal.GetPaymentStatus(tr.PayPalPayoutId);

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
                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (CoachOnlineException ex)
            {
                _logger.LogInformation(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public async Task WithdrawPaymentByPaypal(int userId)
        {
            using (var ctx = new DataContext())
            {

                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                user.CheckExist("User");

                if (string.IsNullOrEmpty(user.PayPalPayerId) && string.IsNullOrEmpty(user.PayPalPayerEmail))
                {
                    throw new CoachOnlineException("User did not sign in via paypal to allow payouts.", CoachOnlineExceptionState.NotExist);
                }

                //var data = await GetEarnedMoneyfromAffiliatesGeneral(userId);

                var payouts = await ctx.AffiliatePayments.Where(t => t.HostId == userId && !t.Transferred && !t.RequestedPaymentId.HasValue && string.IsNullOrEmpty(t.PayPalPayoutId)).ToListAsync();

                var groupped = payouts.GroupBy(t => t.PaymentCurrency).ToList();

                string payment_id = "";
                foreach (var g in groupped)
                {
                    var value = g.Sum(t => t.PaymentValue);

                    if (value > 0)
                    {

                        var requestedPayment = new RequestedPayment();
                        requestedPayment.Currency = g.Key;
                        requestedPayment.UserId = userId;
                        requestedPayment.PayPalEmail = user.PayPalPayerEmail;
                        requestedPayment.PayPalPayerId = user.PayPalPayerId;
                        requestedPayment.PayPalPhone = user.PayPalPayerPhone;
                        requestedPayment.Status = RequestedPaymentStatus.Prepared;
                        requestedPayment.RequestDate = DateTime.Now;
                        requestedPayment.PaymentValue = 0;
                        requestedPayment.PaymentType = RequestedPaymentType.Affiliation;
                        ctx.RequestedPayments.Add(requestedPayment);
                        await ctx.SaveChangesAsync();

                        payment_id = "payout_" + userId.ToString() + "_" + requestedPayment.Id;

                        bool result = true;
                        foreach (var x in g.ToList())
                        {
                            var res = await UpdatePaypalPayoutId(x.Id, payment_id, requestedPayment.Id, requestedPayment.RequestDate, true);
                            if(res == false)
                            {
                                result = false;
                            }
                        }

                        if (result)
                        {
                            requestedPayment.Status = RequestedPaymentStatus.Requested;
                            requestedPayment.PaymentValue = value;
                            await ctx.SaveChangesAsync();
                        }
                        else
                        {
                            //revoke
                            foreach (var x in g.ToList())
                            {
                                var res = await UpdatePaypalPayoutId(x.Id, null, null, null, false);
                            }

                            ctx.RequestedPayments.Remove(requestedPayment);
                            await ctx.SaveChangesAsync();

                            throw new CoachOnlineException("Payment status already changed in meantime. Try again.", CoachOnlineExceptionState.AlreadyChanged);
                        }


                    }
                    else
                    {
                        throw new CoachOnlineException("Payment value must be greater than 0.", CoachOnlineExceptionState.DataNotValid);
                    }
                }
            }
        }
     

        //public async Task WithdrawPaymentByPaypal(int userId)
        //{
        //    using (var ctx = new DataContext())
        //    {

        //        var coach = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
        //        coach.CheckExist("User");

        //        if (string.IsNullOrEmpty(coach.PayPalPayerId) && string.IsNullOrEmpty(coach.PayPalPayerEmail))
        //        {
        //            throw new CoachOnlineException("User did not sign in via paypal to allow payouts.", CoachOnlineExceptionState.NotExist);
        //        }


        //        var data = await GetEarnedMoneyfromAffiliatesGeneral(userId);

        //        var payouts = await ctx.AffiliatePayments.Where(t => t.HostId == userId && !t.Transferred && string.IsNullOrEmpty(t.PayPalPayoutId)).ToListAsync();

        //        var groupped = payouts.GroupBy(t => new { t.AffiliateId, t.PaymentCurrency }).ToList();

        //        string payment_id = "";
        //        string payer_id = coach.PayPalPayerId != null ? coach.PayPalPayerId : coach.PayPalPayerEmail;
        //        foreach (var g in groupped)
        //        {
        //            var userEmail = await GetUserEmail(g.Key.AffiliateId);
        //            var value = g.Sum(t => t.PaymentValue);
        //            if (value > 0)
        //            {
        //                payment_id = "payout_" + userId.ToString() + "_" + g.Key.AffiliateId.ToString();
        //                payment_id += $"_{DateTime.Now.ToString("ddMMyy_HHmmss")}";

        //                var payout = await _payPal.Payout(payer_id, value, g.Key.PaymentCurrency, "CoachsOnline payout for affiliates", "CoachsOnline - Affiliate payout for your PayPal account", $"Payout for user {userEmail} has been sent.", payment_id, coach.PayPalPayerId == null);
        //                if (payout != null)
        //                {


        //                    Console.WriteLine("payout is transferred");
        //                    Console.WriteLine($"batch id is: {payout.batch_header.payout_batch_id}");

        //                    var paymentstatus = await _payPal.GetPaymentStatus(payout.batch_header.payout_batch_id);
        //                    if (paymentstatus.batch_header.batch_status == "SUCCESS")
        //                    {
        //                        foreach (var p in g.ToList())
        //                        {
        //                            await UpdatePaypalPayoutId(p.Id, payout.batch_header.payout_batch_id, DateTime.Now, true);
        //                        }
        //                        Console.WriteLine("payout successfull");
        //                    }
        //                    else
        //                    {
        //                        foreach (var p in g.ToList())
        //                        {
        //                            await UpdatePaypalPayoutId(p.Id, payout.batch_header.payout_batch_id, DateTime.Now, false);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var p in g.ToList())
        //                    {
        //                        await UpdatePaypalPayoutId(p.Id, null, null, false);
        //                    }

        //                    throw new CoachOnlineException("Payout unsuccessfull.", CoachOnlineExceptionState.UNKNOWN);
        //                }
        //            }





        //        }

        //        await CheckPaymentStatuses();
        //    }
        //}

        //private async Task UpdatePaypalPayoutId(int affPaymentId, string payPalPayoutId, DateTime? transferDate, bool transferred)
        //{
        //    using(var ctx = new DataContext())
        //    {
        //        var payment = await ctx.AffiliatePayments.FirstOrDefaultAsync(t => t.Id == affPaymentId);
        //        payment.CheckExist("Affiliate Payment");
        //        Console.WriteLine("Updating payment with batch " + payPalPayoutId);
        //        payment.PayPalPayoutId = payPalPayoutId;
        //        payment.TransferDate = transferDate;

        //        await ctx.SaveChangesAsync();
        //    }
        //}

        private async Task<bool> UpdatePaypalPayoutId(int affPaymentId, string payPalPayoutId, int? requestedPaymentId, DateTime? transferDate, bool transferred)
        {
            using (var ctx = new DataContext())
            {
                var payment = await ctx.AffiliatePayments.FirstOrDefaultAsync(t => t.Id == affPaymentId);
                if (payment != null)
                {
                    //in case when someone tried to transfer in the meantime
                    if(transferred && payment.PayPalPayoutId != null && payment.Transferred)
                    {
                        return false;
                    }
                    try
                    {
                        Console.WriteLine("Updating payment with batch " + payPalPayoutId);
                        payment.PayPalPayoutId = payPalPayoutId;
                        payment.TransferDate = transferDate;
                        payment.RequestedPaymentId = requestedPaymentId;
                        payment.Transferred = transferred;

                        await ctx.SaveChangesAsync();
                        return true;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return false;
                    }
                   
                }
                else
                {
                    return false;
                }
            }
        }

        private async Task<string> GetUserEmail(int userId)
        {
            using(var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                usr.CheckExist("User");

                return usr.EmailAddress;
            }
        }

        public class CoachEarnings
        {
            public decimal Value { get; set; }
            public string Currency { get; set; }
        }

        private async Task<CoachEarnings> GetCoachEarningsForMonth(int coachId, DateTime month)
        {
            CoachEarnings ce = new CoachEarnings();
            ce.Value = 0;
            ce.Currency = "";
            using(var ctx = new DataContext())
            {
                var balance = await ctx.MonthlyBalances.Where(x => x.Year == month.Year && x.Month == month.Month).FirstOrDefaultAsync();

                if (balance != null)
                {
                    ce.Currency = balance.Currency;

                    var coachBalance = await ctx.CoachMonthlyBalance.Where(x => x.MonthlyBalanceId == balance.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                    if (coachBalance != null)
                    {
                        var coachBalanceValue = coachBalance.DayBalances.Where(x => x.Calculated).Sum(x => x.BalanceValue);

                        if (coachBalanceValue > 0)
                        {
                            ce.Value = Math.Round(coachBalanceValue / 100, 2);
                        }
                    }
                }
            }
            return ce;
        }

        private async Task CalculateAffiliatePayment(AffiliateSubscriptionStatus hostSubscription, AffiliateSubscriptionStatus childSubscription, bool isFirstGeneration, DateTime joinDate)
        {
            if(childSubscription.IsCoach && isFirstGeneration)
            {
                var currMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var joinMth = new DateTime(joinDate.Year, joinDate.Month, 1);

                //else coach created account this month, he has no earnings closed for previous month
                if(currMonth > joinMth)
                {
                    //check coach earnings always for previous month because it is calculated this way back
                    var earnings = await GetCoachEarningsForMonth(childSubscription.UserId, currMonth.AddMonths(-1));
                    if (earnings.Value > 0)
                    {
                        using (var ctx = new DataContext())
                        {
                            var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                            if (payments.Any(t => t.IsFirstPayment))
                            {

                                //next payment
                                if (!payments.Any(t => t.PaymentForMonth == currMonth))//inaczej juz zostalo policzone
                                {
                                    var payment = new AffiliatePayment();
                                    payment.IsFirstPayment = false;
                                    payment.HostId = hostSubscription.UserId;
                                    payment.AffiliateId = childSubscription.UserId;
                                    payment.PaymentCreationDate = DateTime.Now;
                                    payment.PaymentCurrency = earnings.Currency;
                                    payment.PaymentValue = Math.Round(earnings.Value * 0.05m, 2);
                                    payment.PaymentForMonth = currMonth;
                                    payment.Transferred = false;
                                    payment.FirstGeneration = true;
                                    payment.IsAffiliateCoach = true;
                                    ctx.AffiliatePayments.Add(payment);
                                    await ctx.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                //first payment
                                var payment = new AffiliatePayment();
                                payment.IsFirstPayment = true;
                                payment.HostId = hostSubscription.UserId;
                                payment.AffiliateId = childSubscription.UserId;
                                payment.PaymentCreationDate = DateTime.Now;
                                payment.PaymentCurrency = earnings.Currency;
                                payment.PaymentValue = Math.Round(earnings.Value * 0.05m, 2);
                                payment.PaymentForMonth = currMonth;
                                payment.Transferred = false;
                                payment.FirstGeneration = true;
                                payment.IsAffiliateCoach = true;
                                ctx.AffiliatePayments.Add(payment);
                                await ctx.SaveChangesAsync();
                            }
                        }
                    }
                    //inaczej coach nic nie zarobil, nie liczymy
                }
                //inaczej to pierwszy miesiac nie liczymy wyplaty
            }
            else if(isFirstGeneration && !childSubscription.IsCoach)
            {
                if (childSubscription.ActiveFromDate.HasValue)
                {
                    if (childSubscription.SubscriptionPeriodMths == 12 && childSubscription.ActiveFromDate <= DateTime.Now)//yearly
                    {
                        using (var ctx = new DataContext())
                        {
                            var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                            if (payments.Any(t => t.IsFirstPayment))
                            {
                                //next payment
                                var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                                if (!payments.Any(t => t.PaymentForMonth == currentMonth))//inaczej juz zostalo policzone
                                {
                                    var lastPayment = payments.OrderBy(t => t.PaymentForMonth).Last();
                                    var lastPaymentDate = lastPayment.PaymentCreationDate.AddMonths(1);
                                    if (lastPaymentDate <= DateTime.Now)
                                    {
                                        var payment = new AffiliatePayment();
                                        payment.IsFirstPayment = false;
                                        payment.HostId = hostSubscription.UserId;
                                        payment.AffiliateId = childSubscription.UserId;
                                        payment.PaymentCreationDate = DateTime.Now;
                                        payment.PaymentCurrency = childSubscription.Currency;
                                        payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                        payment.PaymentForMonth = currentMonth;
                                        payment.Transferred = false;
                                        payment.FirstGeneration = true;
                                        ctx.AffiliatePayments.Add(payment);
                                        await ctx.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                var payment = new AffiliatePayment();
                                payment.IsFirstPayment = true;
                                payment.HostId = hostSubscription.UserId;
                                payment.AffiliateId = childSubscription.UserId;
                                payment.PaymentCreationDate = DateTime.Now;
                                payment.PaymentCurrency = childSubscription.Currency;
                                payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.7m,2) + Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                payment.PaymentForMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                                payment.Transferred = false;
                                payment.FirstGeneration = true;
                                ctx.AffiliatePayments.Add(payment);
                                await ctx.SaveChangesAsync();

                            }
                        }
                    }
                    else if(childSubscription.SubscriptionPeriodMths == 1)//monthly
                    {
                        var calcStartsFrom = childSubscription.ActiveFromDate.Value.AddMonths(1);
                        if (calcStartsFrom<= DateTime.Now && (!childSubscription.ExpiryDate.HasValue || childSubscription.ExpiryDate.Value> DateTime.Now))
                        {
                            using(var ctx = new DataContext())
                            {
                                var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                                if(payments.Any(t=>t.IsFirstPayment))
                                {
                                    //next payment
                                    var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                                   
                                    if(!payments.Any(t=>t.PaymentForMonth == currentMonth))//inaczej juz zostalo policzone
                                    {
                                        var lastPayment = payments.OrderBy(t => t.PaymentForMonth).Last();
                                        var lastPaymentDate = lastPayment.PaymentCreationDate.AddMonths(1);
                                        if (lastPaymentDate <= DateTime.Now)
                                        {
                                            var payment = new AffiliatePayment();
                                            payment.IsFirstPayment = false;
                                            payment.HostId = hostSubscription.UserId;
                                            payment.AffiliateId = childSubscription.UserId;
                                            payment.PaymentCreationDate = DateTime.Now;
                                            payment.PaymentCurrency = childSubscription.Currency;
                                            payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                            payment.PaymentForMonth = currentMonth;
                                            payment.Transferred = false;
                                            payment.FirstGeneration = true;
                                            ctx.AffiliatePayments.Add(payment);
                                            await ctx.SaveChangesAsync();
                                        }
                                    }
                                }
                                else
                                {
                                    var payment = new AffiliatePayment();
                                    payment.IsFirstPayment = true;
                                    payment.HostId = hostSubscription.UserId;
                                    payment.AffiliateId = childSubscription.UserId;
                                    payment.PaymentCreationDate = DateTime.Now;
                                    payment.PaymentCurrency = childSubscription.Currency;
                                    payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.7m, 2) + Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                    payment.PaymentForMonth = new DateTime(calcStartsFrom.Year, calcStartsFrom.Month, 1);
                                    payment.Transferred = false;
                                    payment.FirstGeneration = true;
                                    ctx.AffiliatePayments.Add(payment);
                                    await ctx.SaveChangesAsync();

                                }
                            }
                        }
                    }
                }
            }
            else if(!childSubscription.IsCoach)
            {
                if (childSubscription.ActiveFromDate.HasValue)
                {
                    var calcStartsFrom = childSubscription.ActiveFromDate.Value.AddMonths(3);
                    if (calcStartsFrom <= DateTime.Now && (!childSubscription.ExpiryDate.HasValue || childSubscription.ExpiryDate.Value > DateTime.Now))
                    {
                        using (var ctx = new DataContext())
                        {
                            var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                            if (payments.Any(t => t.IsFirstPayment))
                            {
                                //next payment
                                var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                                if (!payments.Any(t => t.PaymentForMonth == currentMonth))//inaczej juz zostalo policzone
                                {
                                    var lastPayment = payments.OrderBy(t => t.PaymentForMonth).Last();
                                    var lastPaymentDate = lastPayment.PaymentCreationDate.AddMonths(1);
                                    if (lastPaymentDate <= DateTime.Now)
                                    {
                                        var payment = new AffiliatePayment();
                                        payment.IsFirstPayment = false;
                                        payment.HostId = hostSubscription.UserId;
                                        payment.AffiliateId = childSubscription.UserId;
                                        payment.PaymentCreationDate = DateTime.Now;
                                        payment.PaymentCurrency = childSubscription.Currency;
                                        payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.05m, 2);
                                        payment.PaymentForMonth = currentMonth;
                                        payment.Transferred = false;
                                        payment.FirstGeneration = false;
                                        ctx.AffiliatePayments.Add(payment);
                                        await ctx.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                var payment = new AffiliatePayment();
                                payment.IsFirstPayment = true;
                                payment.HostId = hostSubscription.UserId;
                                payment.AffiliateId = childSubscription.UserId;
                                payment.PaymentCreationDate = DateTime.Now;
                                payment.PaymentCurrency = childSubscription.Currency;
                                payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice*0.05m,2);
                                payment.PaymentForMonth = new DateTime(calcStartsFrom.Year, calcStartsFrom.Month, 1);
                                payment.Transferred = false;
                                payment.FirstGeneration = false;
                                ctx.AffiliatePayments.Add(payment);
                                await ctx.SaveChangesAsync();

                            }
                        }
                    }

                }
            }
    
        }


        private async Task CalculateAffiliatePaymentForModel2(AffiliateSubscriptionStatus hostSubscription, AffiliateSubscriptionStatus childSubscription, bool isFirstGeneration, DateTime joinDate, int directHostId)
        {
            if(childSubscription.IsCoach)
            {
                return;
            }

            if(!childSubscription.SubscriptionIsActive)
            { return; }

            if(isFirstGeneration)
            {
                if (childSubscription.ActiveFromDate.HasValue)
                {
                    if (childSubscription.SubscriptionPeriodMths == 12 && childSubscription.ActiveFromDate <= DateTime.Now)//yearly
                    {
                        using (var ctx = new DataContext())
                        {
                            var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                            if (payments.Any())
                            {
                                //next payment
                                var nextPaymentPeriod = payments.Where(x => x.FullYearPayment && x.NextPlannedPaymentDate.HasValue).OrderByDescending(x => x.NextPlannedPaymentDate).FirstOrDefault();

                                if (nextPaymentPeriod != null && nextPaymentPeriod.NextPlannedPaymentDate.Value <= DateTime.Now)//inaczej juz zostalo policzone
                                {
                                    var lastPayment = payments.OrderBy(t => t.PaymentForMonth).Last();
                                    var lastPaymentDate = lastPayment.PaymentCreationDate.AddMonths(1);
                                    if (lastPaymentDate <= DateTime.Now)
                                    {
                                        var payment = new AffiliatePayment();
                                        payment.IsFirstPayment = false;
                                        payment.HostId = hostSubscription.UserId;
                                        payment.AffiliateId = childSubscription.UserId;
                                        payment.PaymentCreationDate = DateTime.Now;
                                        payment.PaymentCurrency = childSubscription.Currency;
                                        payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.2m * 12m, 2);
                                        payment.PaymentForMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                                        payment.Transferred = false;
                                        payment.FirstGeneration = true;
                                        payment.NextPlannedPaymentDate = payment.PaymentCreationDate.AddYears(1);
                                        payment.AffiliateModelType = AffiliateModelType.Influencer;
                                        payment.FullYearPayment = true;
                                        ctx.AffiliatePayments.Add(payment);
                                        await ctx.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                var payment = new AffiliatePayment();
                                payment.IsFirstPayment = true;
                                payment.HostId = hostSubscription.UserId;
                                payment.AffiliateId = childSubscription.UserId;
                                payment.PaymentCreationDate = DateTime.Now;
                                payment.PaymentCurrency = childSubscription.Currency;
                                payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.7m, 2) + Math.Round(childSubscription.SubscriptionPrice * 0.2m * 11m, 2);
                                payment.PaymentForMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                                payment.Transferred = false;
                                payment.FirstGeneration = true;
                                payment.NextPlannedPaymentDate = payment.PaymentCreationDate.AddYears(1);
                                payment.FullYearPayment = true;
                                payment.AffiliateModelType = AffiliateModelType.Influencer;
                                ctx.AffiliatePayments.Add(payment);
                                await ctx.SaveChangesAsync();

                            }
                        }
                    }
                    else//monthly
                    {
                        var calcStartsFrom = childSubscription.ActiveFromDate.Value.AddMonths(1);
                        if (calcStartsFrom <= DateTime.Now && (!childSubscription.ExpiryDate.HasValue || childSubscription.ExpiryDate.Value > DateTime.Now))
                        {
                            using (var ctx = new DataContext())
                            {
                                var payments = await ctx.AffiliatePayments.Where(t => t.HostId == hostSubscription.UserId && t.AffiliateId == childSubscription.UserId).ToListAsync();
                                if (payments.Any())
                                {
                                    //next payment
                                    var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                                    if (!payments.Any(t => t.PaymentForMonth == currentMonth))//inaczej juz zostalo policzone
                                    {
                                        var lastPayment = payments.OrderBy(t => t.PaymentForMonth).Last();
                                        var lastPaymentDate = lastPayment.PaymentCreationDate.AddMonths(1);
                                        if (lastPaymentDate <= DateTime.Now)
                                        {
                                            var payment = new AffiliatePayment();
                                            payment.IsFirstPayment = false;
                                            payment.HostId = hostSubscription.UserId;
                                            payment.AffiliateId = childSubscription.UserId;
                                            payment.PaymentCreationDate = DateTime.Now;
                                            payment.PaymentCurrency = childSubscription.Currency;
                                            payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                            payment.PaymentForMonth = currentMonth;
                                            payment.Transferred = false;
                                            payment.FirstGeneration = true;
                                            payment.AffiliateModelType = AffiliateModelType.Influencer;
                                            payment.FullYearPayment = false;
                                            ctx.AffiliatePayments.Add(payment);
                                            await ctx.SaveChangesAsync();
                                        }
                                    }
                                }
                                else
                                {
                                    var payment = new AffiliatePayment();
                                    payment.IsFirstPayment = true;
                                    payment.HostId = hostSubscription.UserId;
                                    payment.AffiliateId = childSubscription.UserId;
                                    payment.PaymentCreationDate = DateTime.Now;
                                    payment.PaymentCurrency = childSubscription.Currency;
                                    payment.PaymentValue = Math.Round(childSubscription.SubscriptionPrice * 0.7m, 2)+ Math.Round(childSubscription.SubscriptionPrice * 0.2m, 2);
                                    payment.PaymentForMonth = new DateTime(calcStartsFrom.Year, calcStartsFrom.Month, 1);
                                    payment.Transferred = false;
                                    payment.FirstGeneration = true;
                                    payment.AffiliateModelType = AffiliateModelType.Influencer;
                                    payment.FullYearPayment = false;
                                    ctx.AffiliatePayments.Add(payment);
                                    await ctx.SaveChangesAsync();

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ///
                //second generation
                ///
                if (childSubscription.ActiveFromDate.HasValue)
                {

                   using(var ctx = new DataContext())
                    {
                        var directHostPayments = await ctx.AffiliatePayments.Where(x => x.AffiliateId == childSubscription.UserId && x.HostId == directHostId).ToListAsync();

                        if(directHostPayments.Any())
                        {
                            var hostLastPayment = directHostPayments.OrderByDescending(x => x.PaymentCreationDate).FirstOrDefault();
                            var affPayments = await ctx.AffiliatePayments.Where(x => x.AffiliateId == childSubscription.UserId && x.HostId == hostSubscription.UserId).ToListAsync();

                            if(affPayments.Any())
                            {
                                //next payment
                                var lastPayment = affPayments.OrderByDescending(t => t.PaymentCreationDate).FirstOrDefault();
                                if(lastPayment != null && hostLastPayment != null)
                                {
                                    //var currMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                                    if (lastPayment.FullYearPayment && lastPayment.NextPlannedPaymentDate.HasValue && lastPayment.NextPlannedPaymentDate.Value <= DateTime.Now
                                        && (!lastPayment.DirectHostPayIdRef.HasValue || (lastPayment.DirectHostPayIdRef.HasValue && lastPayment.DirectHostPayIdRef.Value != hostLastPayment.Id)))
                                    {
                                        //next payment
                                        var payment = new AffiliatePayment();
                                        payment.IsFirstPayment = false;
                                        payment.HostId = hostSubscription.UserId;
                                        payment.AffiliateId = childSubscription.UserId;
                                        payment.PaymentCreationDate = DateTime.Now;
                                        payment.PaymentCurrency = hostLastPayment.PaymentCurrency;
                                        payment.PaymentValue = Math.Round(hostLastPayment.PaymentValue * 0.05m, 2);
                                        payment.PaymentForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                        payment.Transferred = false;
                                        payment.FirstGeneration = false;
                                        payment.AffiliateModelType = AffiliateModelType.Influencer;
                                        payment.FullYearPayment = hostLastPayment.FullYearPayment;
                                        payment.NextPlannedPaymentDate = hostLastPayment.FullYearPayment ? (DateTime?)payment.PaymentCreationDate.AddYears(1) : null;
                                        payment.DirectHostPayIdRef = hostLastPayment.Id;
                                        ctx.AffiliatePayments.Add(payment);
                                        await ctx.SaveChangesAsync();
                                    }
                                    else if(!lastPayment.FullYearPayment && lastPayment.PaymentCreationDate.AddMonths(1) <= DateTime.Now
                                        && (!lastPayment.DirectHostPayIdRef.HasValue || (lastPayment.DirectHostPayIdRef.HasValue && lastPayment.DirectHostPayIdRef.Value != hostLastPayment.Id)))
                                    {
                                        //next payment monthly?
                                        var payment = new AffiliatePayment();
                                        payment.IsFirstPayment = false;
                                        payment.HostId = hostSubscription.UserId;
                                        payment.AffiliateId = childSubscription.UserId;
                                        payment.PaymentCreationDate = DateTime.Now;
                                        payment.PaymentCurrency = hostLastPayment.PaymentCurrency;
                                        payment.PaymentValue = Math.Round(hostLastPayment.PaymentValue * 0.05m, 2);
                                        payment.PaymentForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                        payment.Transferred = false;
                                        payment.FirstGeneration = false;
                                        payment.AffiliateModelType = AffiliateModelType.Influencer;
                                        payment.FullYearPayment = hostLastPayment.FullYearPayment;
                                        payment.NextPlannedPaymentDate = hostLastPayment.FullYearPayment ? (DateTime?)payment.PaymentCreationDate.AddYears(1) : null;
                                        payment.DirectHostPayIdRef = hostLastPayment.Id;
                                        ctx.AffiliatePayments.Add(payment);
                                        await ctx.SaveChangesAsync();

                                    }
                                }
                            }
                            else
                            {
                                //first payment      

                                if(hostLastPayment != null)
                                {
                                    var payment = new AffiliatePayment();
                                    payment.IsFirstPayment = true;
                                    payment.HostId = hostSubscription.UserId;
                                    payment.AffiliateId = childSubscription.UserId;
                                    payment.PaymentCreationDate = DateTime.Now;
                                    payment.PaymentCurrency = hostLastPayment.PaymentCurrency;
                                    payment.PaymentValue = Math.Round(hostLastPayment.PaymentValue * 0.05m, 2);
                                    payment.PaymentForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                    payment.Transferred = false;
                                    payment.FirstGeneration = false;
                                    payment.AffiliateModelType = AffiliateModelType.Influencer;
                                    payment.FullYearPayment = hostLastPayment.FullYearPayment;
                                    payment.NextPlannedPaymentDate = hostLastPayment.FullYearPayment? (DateTime?)payment.PaymentCreationDate.AddYears(1):null;
                                    payment.DirectHostPayIdRef = hostLastPayment.Id;
                                    ctx.AffiliatePayments.Add(payment);
                                    await ctx.SaveChangesAsync();
                                }
                                
                            }
                        }
                    }
                    
                }
            }
        }
    }
}

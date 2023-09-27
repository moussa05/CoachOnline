using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Adress { get; set; }
        public string Gender { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string Nick { get; set; }
        public string PhoneNo { get; set; }
        public int? ProfessionId { get; set; }
        public int? InstitutionId { get; set; }
        public UserAccountStatus Status { get; set; }
        public UserRoleType UserRole { get; set; }
        //public Category AccountCategory { get; set; }
        public CompanyInfo companyInfo { get; set; }
        public virtual List<Course> OwnedCourses { get; set; }
        public virtual List<TwoFATokens> TwoFATokens { get; set; }
        public virtual List<UserLogins> UserLogins { get; set; }
        //public Category AccountCategory { get; set; }
        public virtual List<Category> AccountCategories { get; set; }
        public Terms TermsAccepted { get; set; }

        public string StripeAccountId { get; set; }
        public bool PaymentsEnabled { get; set; }
        public bool WithdrawalsEnabled { get; set; }
        public string StripeCustomerId { get; set; }
        public bool SubscriptionActive { get; set; }
        public string AvatarUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PayPalPayerId { get; set; }
        public string PayPalPayerEmail { get; set; }
        public string PayPalPayerPhone { get; set; }
        public DateTime? AccountCreationDate { get; set; }

        public virtual ICollection<UserBillingPlan> UserBillingPlans { get; set; }

        public bool? SocialLogin { get; set; }
        public string SocialProvider { get; set; }
        public string SocialAccountId { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }
        public string CouponId { get; set; }
        public DateTime? CouponValidDate { get; set; }
        public bool AllowHiddenProducts { get; set; }

        public UserDocument UserCV { get; set; }
        public virtual List<UserDocument> UserAttestations { get; set; }
        public virtual List<UserDocument> UserReturns { get; set; }
        public virtual List<WebLink> WebLinks { get; set; }

        public EndOfDiscoveryMode EndOdDiscoveryModeStatus{ get; set; }
        public DateTime? EndOdDiscoveryModeEmailSendDate { get; set; }
    }

    public enum EndOfDiscoveryMode : byte { NOT_SEND, FIRST_EMAIL, SECOND_EMAIL}
    public enum UserAccountStatus : byte { AWAITING_EMAIL_CONFIRMATION, CONFIRMED, BANNED, DELETED }
    public enum UserRoleType: byte { STUDENT, COACH, ADMIN, INSTITUTION_STUDENT}



}

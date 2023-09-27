using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{

    public class UserStudentCard
    {
        [Key]
        public int Id { get; set; }
        public string StudentsCardPhotoName { get; set; }
        [NotMapped]
        public string StudentCardUrl
        {
            get
            {
                if (StudentsCardPhotoName == null) return null;
                return $"student_cards/{StudentsCardPhotoName}.jpg";
            }
        }
        public int UserBillingPlanId { get; set; }
        [JsonIgnore]
        public virtual UserBillingPlan UserBillingPlan { get; set; }
    }
    public class UserBillingPlan
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BillingPlanTypeId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PlannedActivationDate { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string StripeSubscriptionScheduleId { get; set; }
        public string StripePriceId { get; set; }
        public string StripeProductId { get; set; }
        public string CouponId { get; set; }

        public BillingPlanStatus Status { get; set; }

        public bool IsStudent { get; set; }
        public StudentCardStatus StudentCardVerificationStatus { get; set; }
        public List<UserStudentCard> StudentCardData { get; set; }

        [NotMapped]
        public string StatusStr
        {
            get { return Status.ToString(); }
        }

        [NotMapped]
        public string StudentCardVerificationStatusStr
        {
            get {
                if (IsStudent)
                    return StudentCardVerificationStatus.ToString();
                else return null;
            }
        }


        public User User { get; set; }
        public BillingPlan BillingPlanType { get; set; }

        public virtual StudentCardRejection StudentCardRejection { get; set; }

        public int? QuestionaaireCancelReason { get; set; }
    }

    public enum BillingPlanStatus : byte { PENDING, CANCELLED, AWAITING_PAYMENT, ACTIVE, AWAITING_ACTIVATION, PAYMENT_REJECTED,DELETED }
    public enum StudentCardStatus : byte { AWAITING_STUDENT_CARD, IN_VERIFICATION, ACCEPTED, REJECTED, CANCELLED}
}

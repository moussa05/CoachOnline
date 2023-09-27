using CoachOnline.Model.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class UserSubscriptionAPIReponse
    {
        public SubscriptionInfoAPIResponse ActiveSubscription { get; set; }
        public SubscriptionInfoAPIResponse AwaitingSubscription { get; set; }
        public List<InvoiceResponse> Invoices { get; set; }
    }

    public class SubscriptionInfoAPIResponse
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionStatusStr { get; set; }
        public BillingPlanStatus SubscriptionStatus { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public decimal Price { get; set; }
        public string Period { get; set; }
        public bool StudentOption { get; set; }
        public string Currency { get; set; }
        public int UserId { get; set; }
        public StudentOptionInfoAPIRespponse StudentSubscriptionStatus { get; set; }

    }

    public class StudentOptionInfoAPIRespponse
    {
        public List<StudentCardImg> StudentCardData{ get; set; }
        public StudentCardStatus StudentCardStatus { get; set; }
        public string StudentCardStatusStr { get; set; }
    }

    public class StudentCardImg
    {
        public string PhotoUrl { get; set; }
        public string PhotoName { get; set; }
    }
}

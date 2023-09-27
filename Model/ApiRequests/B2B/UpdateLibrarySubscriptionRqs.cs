using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class UpdateLibrarySubscriptionRqs
    {
        public decimal? NegotiatedPrice { get; set; }
        public bool? AutoRenew { get; set; }
    }

    public class UpdateLibrarySubscriptionRqsWithToken:TokenOnlyRequest
    {
        public decimal? NegotiatedPrice { get; set; }
        public bool? AutoRenew { get; set; }
    }
}

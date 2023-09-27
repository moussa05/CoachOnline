using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class ExtractAffiliateHostDataResponse: ExtractUserDataResponse
    {
       public int FirstLineAffiliatesQty { get; set; }
        public int SecondLineAffiliatesQty { get; set; }
        public int TotalAffiliatesQty { get; set; }
        public int CoachAffiliatesQty { get; set; }
        public int SubscribersAffiliatesQty { get; set; }
        public decimal TotalIncome { get; set; }
        public string Currency { get; set; }
        public string AffiliateType { get; set; }
        public AffiliateModelType AffiliatorType { get; set; }
        public string AffiliatorTypeStr
        {
            get
            {
                return AffiliatorType.ToString();
            }
        }
    }
}

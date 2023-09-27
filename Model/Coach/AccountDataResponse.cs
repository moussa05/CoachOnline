using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Coach
{
    public class AccountDataResponse
    {
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string StreetNo { get; set; }
        public string Country { get; set; }
        public string BankAccountNo { get; set; }
        public string SiretNo { get; set; }
        public string VatNo { get; set; }
        public string Last4Digits { get; set; }
    }
}

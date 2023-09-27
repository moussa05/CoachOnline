using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class UpdateCompanyInfoRequest
    {
        public string AuthToken { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string SiretNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string RegisterAddress { get; set; }
        public string Country { get; set; }
        public string VatNumber { get; set; }
        public string BICNumber { get; set; }
        //[RegularExpression(@"(?:0[1-9]|[13-8][0-9]|2[ab1-9]|9[0-5])(?:[0-9]{3})?|9[78][1-9](?:[0-9]{2})?", ErrorMessage = "Provide correct French ZipCode")]
        [MaxLength(15)]
        public string PostCode { get; set; }


    }
}

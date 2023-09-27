using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class CompanyInfo
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string SiretNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string RegisterAddress { get; set; }
        public string Country { get; set; }
        public string VatNumber { get; set; }
        public string ZipCode { get; set; }
        public string BICNumber { get; set; }
    
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public ContractType Type { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }

    public enum ContractType : byte
    {
        Agreement,
        PrivacyPolicy,
        GeneralConditionsOfUsage,
        TermsAndConditions
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class ContractResponse
    {
        public int ContractId { get; set; }
        public ContractType Type { get; set; }
        public string ContractName { get; set; }
        public string ContractBody { get; set; }
    }

    public class ContractResponseAdmin: ContractResponse
    {
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsCurrent { get; set; }
    }
}

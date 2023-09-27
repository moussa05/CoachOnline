using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateContractRqs
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public bool? IsCurrent { get; set; }
    }
}

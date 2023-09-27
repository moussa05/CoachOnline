using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class ManageServicesForB2BAccountRqs
    {
        public List<ServiceRqs> Services { get; set; }
    }

    public class ServiceRqs
    {
        public int ServiceId { get; set; }
        public decimal? Comission { get; set; }
        public string ComissionCurrency { get; set; }
        public bool? RemoveService { get; set; }
    }
}

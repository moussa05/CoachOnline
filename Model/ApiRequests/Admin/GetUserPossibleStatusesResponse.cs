using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class GetUserPossibleStatusesResponse
    {
        public List<ItemOfGetUserPossibleStatusesResponse> Statuses { get; set; }
    }

    public class ItemOfGetUserPossibleStatusesResponse
    {
        public UserAccountStatus Status { get; set; }
        public string StatusString { get; set; }
    }
}

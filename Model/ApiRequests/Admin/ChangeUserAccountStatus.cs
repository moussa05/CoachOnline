using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class ChangeUserAccountStatusRequest
    {
        public string AdminAuthToken { get; set; }
        public int UserId { get; set; }
        public UserAccountStatus NewStatus { get; set; }
    }
}

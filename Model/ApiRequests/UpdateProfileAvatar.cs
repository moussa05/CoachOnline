using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class UpdateProfileAvatarRequest
    {
        public string AuthToken { get; set; }
        public string PhotoBase64 { get; set; }
        public bool RemoveAvatar { get; set; }
    }

    public class UpdateProfileAvatarResponse
    {
        public string FileName { get; set; }
    }

}

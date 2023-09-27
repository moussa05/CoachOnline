using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.LibraryManagementService;

namespace CoachOnline.Model.ApiResponses.B2B
{
    public class LibraryBasicInfoResp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string LibraryName { get; set; }
        public string PhotoUrl { get; set; }
        public string Website { get; set; }
        public string Link { get; set; }

    }

    public class LibraryBasicInfoRespWithAccountType: LibraryBasicInfoResp
    {
        public B2BAccountType AccountType { get; set; }
        public string AccountTypeStr { get; set; }
    }
}

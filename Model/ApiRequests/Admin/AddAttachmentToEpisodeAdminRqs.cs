using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class AddAttachmentToEpisodeAdminRqs
    {
        public string AttachmentName { get; set; }
        public string AttachmentBase64 { get; set; }
        public string AttachmentExtension { get; set; }
    }

}

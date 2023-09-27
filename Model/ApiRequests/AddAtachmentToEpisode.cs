using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class AddAtachmentToEpisodeRequest
    {
        public string AuthToken { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentBase64 { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
        public string AttachmentExtension { get; set; }

    }

    public class AddAtachmentToEpisodeResponse
    {
        public List<EpisodeAttachment> attachments { get; set; }
    }

    public class RemoveAttachmentFromEpisodeRequest
    {
        public string AuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
        public int AttachmentId { get; set; }
    }
}

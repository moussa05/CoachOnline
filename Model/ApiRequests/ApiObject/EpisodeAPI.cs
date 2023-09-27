using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.ApiObject
{
    public class EpisodeAPI
    {
        internal bool needConversion;

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MediaId { get; set; }
        public List<EpisodeAttachment> Attachments { get; set; }
        public long Created { get; set; }
        public int OrdinalNumber { get; set; }
        public double Length { get; internal set; }
        public bool IsPromo { get; set; }
        public EpisodeState EpisodeState { get; set; }
        public string EpisodeStateStr
        {
            get { return EpisodeState.ToString(); }
        }
    }
    public class EpisodeAttachmentAPI
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Added { get; set; }
    }
}

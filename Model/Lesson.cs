using CoachOnline.Model.Student;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
//using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Episode
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MediaId { get; set; }
        public List<EpisodeAttachment> Attachments { get; set; }
        public long Created { get; set; }
        public int OrdinalNumber { get; set; }
        public bool MediaNeedsConverting { get; set; }
        public double MediaLenght { get; set; }
        public bool? IsPromo { get; set; }
        public EpisodeState EpisodeState { get; set; }
    }

    public enum EpisodeState:byte
    {
        BEFORE_UPLOAD,
        UPLOADED,
        BEFORE_CONVERSION,
        CONVERTED,
        ERROR_WITH_CONVERSION
    }

    public class EpisodeAttachment
    {
        [Key]
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Added { get; set; }
        public int EpisodeId { get; set; }
        [JsonIgnore]
        public virtual Episode Episode { get; set; }

        [NotMapped]
        public virtual UserEpisodeAttachemntPermission UserTokenPermission { get; set; }
        [NotMapped]
        public string QueryString { get; set; }

    }


}

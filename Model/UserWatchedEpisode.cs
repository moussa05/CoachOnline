using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class UserWatchedEpisode
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EpisodeId { get; set; }
        public DateTime Day {get; set;}
        public decimal EpisodeWatchedTime { get; set; }
        public decimal EpisodeDuration { get; set; }
        public bool IsWatched { get; set; }
    }
}

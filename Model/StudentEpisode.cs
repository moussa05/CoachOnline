using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class StudentEpisode
    {
        [Key]
        public int Id { get; set; }
        public int EpisodeId { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public virtual User Student { get; set; }
        public virtual Course Course { get; set; }
        public decimal StoppedAtTimestamp { get; set; }
        public DateTime LastWatchDate { get; set; }
        public DateTime FirstOpenDate { get; set; }
        public WatchStatus WatchedStatus { get; set; }
        public virtual Episode Episode { get; set; }
        [NotMapped]
        public decimal Duration { get; set; }
    }

    public enum WatchStatus:byte
    {
        OPENED, IN_PROGRESS, WATCHED
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Hubs.Model
{
    public class HubUserInfo
    {
        public string ConnectionId { get; set; }
        public int UserId { get; set; }
        public int EpisodeId { get; set; }
    }
}

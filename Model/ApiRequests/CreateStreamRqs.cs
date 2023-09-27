using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CreateStreamRqs
    {
        public string ChannelName { get; set; }
        public string UserId { get; set; }
        public bool IsHost { get; set; }
        public uint ValidUntil { get; set; } = 0;
    }
}

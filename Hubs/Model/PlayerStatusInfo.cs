using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Hubs.Model
{
    public class PlayerStatusInfo
    {
        public int episodeId { get; set; }
        public string authToken { get; set; }
        public decimal duration { get; set; }
        public decimal currentSecond { get; set; }
        public decimal rate { get; set; }
    }
}

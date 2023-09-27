using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface ICounter
    {
        Task CountAllEpisodes();
        Task SuggestVideos(DateTime day, int monthPeriod);
        Task ReSuggestVideosForDay(DateTime day, int monthPeriod);
    }
}

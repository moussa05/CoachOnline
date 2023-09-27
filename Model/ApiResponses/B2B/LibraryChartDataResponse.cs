using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.B2B
{
    public class LibraryChartDataResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AgesChartDataResponse> Ages { get; set; }
    }

    public class AgesChartDataResponse
    {
        public string AgeGroup { get; set; }
        public int StartAge { get; set; }
        public int EndAge { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public int Sum { get; set; }
    }
}

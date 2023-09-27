using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class QuestionaaireStatsResponse
    {
        public int FormId { get; set; }
        public string FormName { get; set; }
        public int TotalResponses { get; set; }
        public List<QuestionaaireStatsDetailsResponse> Responses { get; set; }
    }

    public class QuestionaaireStatsDetailsResponse
    {
        public string Response { get; set; }
        public int CountedResponses { get; set; }
        public decimal Percentage { get; set; }
    }
}

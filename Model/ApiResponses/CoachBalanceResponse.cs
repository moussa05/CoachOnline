using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class CoachBalanceResponse
    {
        public DateTime Month { get; set; }
        public decimal TotalBalance { get; set; }
        public string Currency { get; set; }
        public decimal TotalWatchedMinutes { get; set; }
        public List<CoachDayBalanceResponse> DayBalances { get; set; } = new List<CoachDayBalanceResponse>();
    }

    public class CoachDayBalanceResponse
    {
        public DateTime Day { get; set; }
        public decimal DayBalance { get; set; }
        public decimal TotalWatchedMinutes { get; set; }
    }


    public class CoachSummarizedMinutesReponse
    {
        public DateTime Month { get; set; }
        public decimal TotalWatchedMinutesCurrentMonth { get; set; }
        public decimal TotalWatchedMinutesPreviousMonth { get; set; }
    }

    public class CoachSummarizedRankingReponse
    {
        public DateTime? Month { get; set; }
        public int CoachId { get; set; }
        public int RankPosition { get; set; }
        public decimal TotalMinutes { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime? JoinDate { get; set; }
        public bool IsMe { get; set; } = false;
    }

    public class CoachSummarizedBalanceReponse
    {
        public DateTime Month { get; set; }
        public string Currency { get; set; }
        public decimal TotalBalanceToWithdrawCurrentMonth { get; set; }
        public decimal TotalBalanceCurrentMonth { get; set; }
        public decimal TotalBalanceToWithdrawPreviousMonth { get; set; }
        public decimal TotalBalancePreviousMonth { get; set; }
    }
}

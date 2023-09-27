using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class QuestionnaireRqs
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public QuestionnaireType Type { get; set; }
        public List<QuestionnaireOptionRqs> Responses { get; set; }
        public bool HasOtherOption { get; set; }
    }

    public class QuestionnaireOptionRqs
    {
        [Required]
        public string Response { get; set; }
    }
}

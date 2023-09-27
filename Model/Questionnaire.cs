using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public enum QuestionnaireType : byte
    {
        WhereAreYouFrom,
        CancelSub
    }
    public class Questionnaire
    {
        [Key]
        public int Id { get; set; }
        public string Question { get; set; }
        public List<QuestionnaireOption> Options { get; set; }
        public List<QuestionnaireAnswer> Answers { get; set; }
        public QuestionnaireType QuestionnaireType { get; set; }
    }

    public class QuestionnaireOption
    {
        [Key]
        public int Id { get; set; }
        public int? QuestionnaireId { get; set; }
        public virtual Questionnaire Questionnaire { get; set; }
        public string Option { get; set; }
        public bool IsOtherOption { get; set; }
    }

    public class QuestionnaireAnswer
    {
        [Key]
        public int Id { get; set; }
        public int QuestionnaireId { get; set; }
        public virtual Questionnaire Questionnaire { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ResponseId { get; set; }
        public virtual QuestionnaireOption Response { get; set; }
        public string OtherResponse { get; set; }
    }
}

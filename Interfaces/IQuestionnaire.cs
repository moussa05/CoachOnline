using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IQuestionnaire
    {
        Task<FormResponse> GetForm(QuestionnaireType type);
        Task<FormResponse> GetForm();
        Task<int> RespondToForm(int formId, int userId, QuestionnaireAnswerRqs rqs);
        Task<int> AddForm(QuestionnaireRqs rqs);
        Task<int> EditForm(int formId, QuestionnaireRqs rqs);
        Task<string> UserAnswer(int userId, QuestionnaireType type);
        Task<bool> UserHasAnswered(int userId);
        Task<QuestionaaireStatsResponse> GetStats(QuestionnaireType? formType = null);
    }
}

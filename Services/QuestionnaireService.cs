using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.QuestionnaireService;

namespace CoachOnline.Services
{

    public class QuestionnaireService: IQuestionnaire
    {

        private readonly ILogger<QuestionnaireService> _logger;
        private readonly IUser _userSvc;
        public QuestionnaireService(ILogger<QuestionnaireService> logger, IUser userSvc)
        {
            _logger = logger;
            _userSvc = userSvc;
        }

        public async Task<int> AddForm(QuestionnaireRqs rqs)
        {
            using(var ctx = new DataContext())
            {

                var exists = await ctx.Questionnaires.Where(x=>x.QuestionnaireType == rqs.Type).AnyAsync();

                if(exists)
                {
                    throw new CoachOnlineException("Questionnaire already exists", CoachOnlineExceptionState.AlreadyExist);
                }

                var form = new Questionnaire();
                form.Question = rqs.Question;
                form.QuestionnaireType = rqs.Type;
                form.Options = new List<QuestionnaireOption>();

                if (rqs.Responses != null)
                {
                    foreach (var resp in rqs.Responses)
                    {
                        form.Options.Add(new QuestionnaireOption { IsOtherOption = false, Option = resp.Response });
                    }
                }

                if(rqs.HasOtherOption)
                {
                    form.Options.Add(new QuestionnaireOption { IsOtherOption = true, Option = "autre" });
                }

                ctx.Questionnaires.Add(form);
                await ctx.SaveChangesAsync();

                return form.Id;
            }
        }

     

        public async Task<QuestionaaireStatsResponse> GetStats(QuestionnaireType? type = null)
        {
            QuestionaaireStatsResponse resp = new QuestionaaireStatsResponse();
            FormResponse form = null;
            if (type == null)
            {
                form = await GetForm();
                form.CheckExist("Form");
            }
            else
            {
                form = await GetForm(type.Value);
               
            }
            form.CheckExist("Form");
            resp.FormName = form.Qustion;
            resp.FormId = form.Id;
            resp.Responses = new List<QuestionaaireStatsDetailsResponse>();
            using(var ctx = new DataContext())
            {
                var userResponses = await ctx.QuestionnaireAnswers.Where(x => x.QuestionnaireId == form.Id).ToListAsync();

                var groupped = userResponses.GroupBy(x => x.ResponseId);

                resp.TotalResponses = userResponses.Count;

                Dictionary<string, int> opts = new Dictionary<string, int>();
                foreach(var g in groupped)
                {
                    var answer = await ctx.QuestionnaireOptions.FirstOrDefaultAsync(x => x.Id == g.Key);
                    if(answer != null)
                    {
                        if(answer.IsOtherOption)
                        {
                            if (opts.ContainsKey(answer.Option))
                            {
                                opts[answer.Option] += g.Count();
                            }
                            else
                            {
                                opts.Add(answer.Option, g.Count());
                            }
                        }
                        else
                        {
                            if(opts.ContainsKey(answer.Option))
                            {
                                opts[answer.Option] += g.Count();
                            }
                            else
                            {
                                opts.Add(answer.Option, g.Count());
                            }
                        }
                    }
                }

                foreach(var k in opts.Keys)
                {
                    var counted = opts[k];
                    decimal percentage = 0;
                    if (resp.TotalResponses > 0)
                    {
                        percentage = Math.Round((decimal)counted / (decimal)resp.TotalResponses,2);
                    }
                    resp.Responses.Add(new QuestionaaireStatsDetailsResponse { Response = k, CountedResponses = counted, Percentage = percentage });
                }

                return resp;
            }
        }

        public async Task<int> EditForm(int formId, QuestionnaireRqs rqs)
        {
            using (var ctx = new DataContext())
            {
                var questionnaire = await ctx.Questionnaires.Where(x => x.Id == formId).Include(x => x.Options).FirstOrDefaultAsync();
                questionnaire.CheckExist("Form");


                questionnaire.Question = rqs.Question;

                if(questionnaire.Options.Any())
                {
                    questionnaire.Options.RemoveAll(x=>x.Id == x.Id);
                }

                if (rqs.Responses != null)
                {
                    foreach (var resp in rqs.Responses)
                    {
                        questionnaire.Options.Add(new QuestionnaireOption { IsOtherOption = false, Option = resp.Response });
                    }
                }

          

                if (rqs.HasOtherOption)
                {
                    questionnaire.Options.Add(new QuestionnaireOption { IsOtherOption = true, Option = "Autre" });
                }

                await ctx.SaveChangesAsync();

                return questionnaire.Id;
            }
        }

        public async Task<FormResponse> GetForm()
        {
            using (var ctx = new DataContext())
            {
                FormResponse resp = new FormResponse();
                var questionnaire = await ctx.Questionnaires.Where(x=>x.QuestionnaireType == QuestionnaireType.WhereAreYouFrom).Include(x => x.Options).FirstOrDefaultAsync();
                if(questionnaire == null)
                {
                    return null;
                }

                resp.Id = questionnaire.Id;
                resp.Qustion = questionnaire.Question;
                resp.Responses = new List<FormOptionResponse>();

                foreach (var r in questionnaire.Options)
                {
                    resp.Responses.Add(new FormOptionResponse { Id = r.Id, Response = r.Option, OtherResponse = r.IsOtherOption });
                }
                return resp;
            }
        }


        public async Task<FormResponse> GetForm(QuestionnaireType type)
        {
            using(var ctx = new DataContext())
            {
                FormResponse resp = new FormResponse();
                var questionnaire = await ctx.Questionnaires.Where(x => x.QuestionnaireType == type).Include(x => x.Options).FirstOrDefaultAsync();
                questionnaire.CheckExist("Form");


                resp.Id = questionnaire.Id;
                resp.Qustion = questionnaire.Question;
                resp.Responses = new List<FormOptionResponse>();

                foreach(var r in questionnaire.Options)
                {
                    resp.Responses.Add(new FormOptionResponse { Id = r.Id, Response = r.Option, OtherResponse = r.IsOtherOption });
                }
                return resp;
            }
        }

        public async Task<string> UserAnswer(int userId, QuestionnaireType type)
        {
            using(var ctx = new DataContext())
            {
                var answer = await ctx.QuestionnaireAnswers.Include(r=>r.Response).Include(q=>q.Questionnaire).Where(q=>q.Questionnaire.QuestionnaireType == type).FirstOrDefaultAsync(x => x.UserId == userId);

                if(answer != null && answer.Response != null)
                {
                    if(answer.Response.IsOtherOption)
                    {
                        return answer.OtherResponse;
                    }
                    else
                    {
                        return answer.Response.Option;
                    }
                }

                return null;
            }
        }

        public async Task<bool> UserHasAnswered(int userId)
        {
            using (var ctx = new DataContext())
            {
                var answer = await ctx.QuestionnaireAnswers.Include(r => r.Response).Include(q=>q.Questionnaire).Where(x=>x.Questionnaire.QuestionnaireType == QuestionnaireType.WhereAreYouFrom).FirstOrDefaultAsync(x => x.UserId == userId);

                if (answer != null)
                {
                    return true;
                }

                return false;
            }
        }

        public async Task<int> RespondToForm(int formId, int userId, QuestionnaireAnswerRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var user = _userSvc.GetUserById(userId);
                user.CheckExist("User");
                var questionnaire = await ctx.Questionnaires.Where(x => x.Id == formId).Include(x => x.Options).FirstOrDefaultAsync();
                questionnaire.CheckExist("Form");
                
                if(!questionnaire.Options.Any(x=>x.Id == rqs.ResponseId))
                {
                    throw new CoachOnlineException("Such response does not exist",CoachOnlineExceptionState.NotExist);
                }

                if (questionnaire.QuestionnaireType != QuestionnaireType.CancelSub)
                {
                    if (await ctx.QuestionnaireAnswers.AnyAsync(x => x.UserId == userId && x.QuestionnaireId == formId))
                    {
                        throw new CoachOnlineException("Answer to this form already exists.", CoachOnlineExceptionState.AlreadyExist);
                    }
                }

                var optionSelected = questionnaire.Options.Where(x => x.Id == rqs.ResponseId).FirstOrDefault();

                if(optionSelected.IsOtherOption && string.IsNullOrEmpty(rqs.Additional))
                {
                    throw new CoachOnlineException("Other option information not provided.", CoachOnlineExceptionState.DataNotValid);
                }

                var answer = new QuestionnaireAnswer();
                answer.UserId = userId;
                answer.ResponseId = rqs.ResponseId;
                answer.QuestionnaireId = formId;
                answer.OtherResponse = optionSelected.IsOtherOption ? rqs.Additional : null;

                ctx.QuestionnaireAnswers.Add(answer);

                await ctx.SaveChangesAsync();

                return answer.Id;
            }
        }
    }
}

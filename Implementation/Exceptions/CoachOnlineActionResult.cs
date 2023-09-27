using CoachOnline.Model.ApiRequests;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation.Exceptions
{
    public class CoachOnlineActionResult : ActionResult
    {
        public CoachOnlineActionResult(CoachOnlineException e)
        {
            this.exception = e;
        }

        CoachOnlineException exception;

        private int createCode(CoachOnlineExceptionState state)
        {
            int httpCode = 400;
            switch (state)
            {
                case CoachOnlineExceptionState.UNKNOWN:
                    httpCode = 400;
                    break;
                case CoachOnlineExceptionState.WrongDataSent:
                    httpCode = 400;
                    break;
                case CoachOnlineExceptionState.NotAuthorized:
                    httpCode = 401;
                    break;
                case CoachOnlineExceptionState.WrongToken:
                    httpCode = 401;
                    break;
                case CoachOnlineExceptionState.TokenNotActive:
                    httpCode = 401;
                    break;
                case CoachOnlineExceptionState.PermissionDenied:
                    httpCode = 401;
                    break;
                case CoachOnlineExceptionState.NotExist:
                    httpCode = 404;
                    break;
                case CoachOnlineExceptionState.SubscriptionNotExist:
                    httpCode = 403;
                    break;
                default:
                    httpCode = 400;
                    break;
            }
            return httpCode;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            WebApiError error = new WebApiError();
            error.Error = exception.Message ?? "";
            error.Code = exception.ExceptionStatus;
            error.CodeString = exception.ExceptionStatus.ToString();
            string value = JsonConvert.SerializeObject(error);
            int statusCode = createCode(exception.ExceptionStatus);
            var result = new ObjectResult(exception)
            {
                StatusCode = statusCode,
                Value = value
            };
            await result.ExecuteResultAsync(context);

        }
    }
}

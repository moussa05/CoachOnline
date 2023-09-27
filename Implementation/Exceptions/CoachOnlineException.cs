using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation.Exceptions
{
    public class CoachOnlineException : Exception
    {
        public CoachOnlineException(string message, CoachOnlineExceptionState status) : base(message)
        {
            this.ExceptionStatus = status;
        }

        public CoachOnlineExceptionState ExceptionStatus;

    }
    public enum CoachOnlineExceptionState
    {
        UNKNOWN,
        PasswordsNotMatch,
        Internal,
        AlreadyExist,
        WeakPassword,
        NotExist,
        WrongPassword,
        Expired,
        WrongToken,
        WrongDataSent,
        IncorrectFormat,
        DataNotValid,
        PermissionDenied,
        NotAuthorized,
        AlreadyChanged,
        CantChange,
        Deprecated,
        TokenNotActive,
        SubscriptionNotExist,
        UserIsBanned 
    }
}




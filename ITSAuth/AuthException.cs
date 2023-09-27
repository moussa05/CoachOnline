using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string Error, AuthExCode ExceptionCode) : base(Error)
        {

        }

        public enum AuthExCode { Internal, AlreadyExist, WeakPassword, NotExist, WrongPassword, Expired, WrongToken }

    }
}

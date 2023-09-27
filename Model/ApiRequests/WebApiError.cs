using CoachOnline.Implementation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class WebApiError
    {
        public string Error { get; set; }
        public string DisplayError { get; set; }
        public CoachOnlineExceptionState Code { get; set; }
        public string CodeString { get; set; }
    }
}

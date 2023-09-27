using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class FormResponse
    {
        public int Id { get; set; }
        public string Qustion { get; set; }
        public List<FormOptionResponse> Responses { get; set; }
    }

    public class FormOptionResponse
    {
        public int Id { get; set; }
        public string Response { get; set; }
        public bool OtherResponse { get; set; }
    }
}

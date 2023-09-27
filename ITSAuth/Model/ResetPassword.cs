using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Model
{
    public class ResetPassword
    {
        public int Id { get; set; }
        public int ValidTo { get; set; }
        public string Hash { get; set; }
        public int Requested { get; set; }
        public bool Used { get; set; }
    }
}

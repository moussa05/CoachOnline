using ITSAuth.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITSAuth.Model
{
    public class Auth
    {
        
        public string Identificator { get; set; }
        public string Secret { get; set; }

    }
}

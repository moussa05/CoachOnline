﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class ConfirmEmailRequest
    {
        public string EmailToken { get; set; }
    }
}

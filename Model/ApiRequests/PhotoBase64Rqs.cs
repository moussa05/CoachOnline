using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class PhotoBase64Rqs
    {
        [Required]
        public string ImgBase64 { get; set; }
    }
}

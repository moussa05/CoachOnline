using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class FAQTopic
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Tags { get; set; }
        public string Topic { get; set; }
        public string Body { get; set; }
        public FAQCategory Category { get; set; }
    }
}

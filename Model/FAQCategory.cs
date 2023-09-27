using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class FAQCategory
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public ICollection<FAQTopic> Topics { get; set; }
    }
}

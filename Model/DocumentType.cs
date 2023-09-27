using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }
        public string TypeName { get; set; }
        // public ICollection<FAQTopic> Topics { get; set; }
    }
}

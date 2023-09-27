using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class UserDocument
    {
        [Key]
        public int Id { get; set; }
        public string DocumentUrl { get; set; }
        // public DocumentType DocumentType { get; set; }
    }
}

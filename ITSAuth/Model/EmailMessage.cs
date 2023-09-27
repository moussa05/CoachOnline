using ITSAuth.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITSAuth.Model
{
    public class EmailMessage
    {
        [Key]
        public int Id { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverName { get; set; }
        public string Topic { get; set; }
        public string Body { get; set; }

        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }

    }



}

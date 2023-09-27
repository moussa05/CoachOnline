using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Model
{
    public class SMS
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string CustomAuthor { get; set; }
        public string AuthorNumber { get; set; }
        public string ReceiverNumber { get; set; }
    }
}

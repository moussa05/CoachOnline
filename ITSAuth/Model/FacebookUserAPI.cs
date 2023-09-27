using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Model
{
    public class FacebookUserData
    {
        public int height { get; set; }
        public bool is_silhouette { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class FacebookUserPicture
    {
        public FacebookUserData data { get; set; }
    }

    public class FacebookUserAPI
    {
        public string name { get; set; }
        public string gender { get; set; }
        public FacebookUserPicture picture { get; set; }
        public string birthday { get; set; }
        public string id { get; set; }
    }
}

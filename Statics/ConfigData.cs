using CoachOnline.Model.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Statics
{
    public static class ConfigData
    {
        public static Config Config = new Config();
        public static void UpdateConfig()
        {
#if DEBUG
            if (System.IO.File.Exists("./Config.dev.json"))
            {
                string configDirty = System.IO.File.ReadAllText("./Config.dev.json");
                Config = JsonConvert.DeserializeObject<Config>(configDirty);
            }
            else
            {
                Config.SiteUrl = "http://127.0.0.1:5050";
                Config.WebUrl = "http://127.0.0.1:5050";
            }
#else


            if (System.IO.File.Exists("/home/ubuntu/coachonlinebuild/Config.json"))
            {

                string configDirty = System.IO.File.ReadAllText("/home/ubuntu/coachonlinebuild/Config.json");
                Config = JsonConvert.DeserializeObject<Config>(configDirty);

            }
            else
            {
                Config.SiteUrl = "http://127.0.0.1:5050";
                Config.WebUrl = "http://127.0.0.1:5050";

            }
#endif

        }
    }
}

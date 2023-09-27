using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Config
{
    public class Config
    {
        public string SiteUrl { get; set; }
        public string WebUrl { get; set; }
        public string StripeWebhookKey { get; set; }
        public string StripeWebhookKeyAccount { get; set; }
        public string StripeRk { get; set; }
        public string MongoDBServer { get; set; }
        public string MongoDBName { get; set; }
        public ElasticSearch ElasticSearch { get; set; }
        public string EnviromentPath { get; set; }
        public string PayPalClientID { get; set; }
        public string PayPalSecret { get; set; }
        public string PayPalBaseUrl { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleSecret { get; set; }
        public string SubdomainUrl { get; set; }
        public AWS AWS { get; set; }



    }

    public class AWS
    {
        public string Region { get; set; }
        public string Profile { get; set; }
    }

    public class ElasticSearch
    {
        public string ElasticSearchNode { get; set; }
        public string CoachIdx { get; set; }
        public string CategoryIdx { get; set; }
        public string CourseIdx { get; set; }
        public string EpisodeIdx { get; set; }
    }
}

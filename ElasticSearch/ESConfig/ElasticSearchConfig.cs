using CoachOnline.ElasticSearch.Models;
using CoachOnline.Statics;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.ESConfig
{
    public static class ElasticSearchConfig
    {
        public static void AddElasticSearch(this IServiceCollection services)
        {
            var url = ConfigData.Config.ElasticSearch.ElasticSearchNode;

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(ConfigData.Config.ElasticSearch.CourseIdx);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            CreateCoachIndex(client, ConfigData.Config.ElasticSearch.CoachIdx);
            CreateCourseIndex(client, ConfigData.Config.ElasticSearch.CourseIdx);
            CreateCategoryIndex(client, ConfigData.Config.ElasticSearch.CategoryIdx);
            //CreateEpisodeIndex(client, ConfigData.Config.ElasticSearch.EpisodeIdx);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {

        }

        private static void CreateCoachIndex(IElasticClient client, string indexName)
        {

            //var createIndexResponse = client.Indices.Create(indexName,
            //    index => index.Map<CoachIndex>(x => x.AutoMap())
            //);

            var createIndexResponse = client.Indices.Create(indexName, c => c
            .Settings(s => s
            .Analysis(analysis => analysis
              .TokenFilters(f => f.AsciiFolding("my_ascii_folding", t => t.PreserveOriginal(true)))
                .Analyzers(analyzers => analyzers
                .Custom("folding", cc => cc
                .Tokenizer("standard")
                .Filters("my_ascii_folding", "lowercase")))
                )
            )
            .Map<CoachIndex>(x => x.AutoMap()));
        }

        private static void CreateCourseIndex(IElasticClient client, string indexName)
        {
            //var createIndexResponse = client.Indices.Create(indexName,
            //    index => index.Map<CoachIndex>(x => x.AutoMap())
            //);

            var createIndexResponse = client.Indices.Create(indexName, c => c
            .Settings(s => s
            
            .Analysis(analysis => analysis


                .TokenFilters(f => f.AsciiFolding("my_ascii_folding", t => t.PreserveOriginal(false)))
                .Analyzers(analyzers => analyzers
                .Custom("folding", cc => cc
                .Tokenizer("standard")
                .Filters("my_ascii_folding", "lowercase")))
                )

            )

            
            .Map<CourseIndex>(x => x.AutoMap()));
        }

        private static void CreateCategoryIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName, c => c
           .Settings(s => s
           .Analysis(analysis => analysis
             .TokenFilters(f => f.AsciiFolding("my_ascii_folding", t => t.PreserveOriginal(true)))
               .Analyzers(analyzers => analyzers
               .Custom("folding", cc => cc
               .Tokenizer("standard")
               .Filters("my_ascii_folding", "lowercase")))
               )
           )
           .Map<CategoryIndex>(x => x.AutoMap()));
        }

    }
}

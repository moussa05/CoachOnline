using CoachOnline.ElasticSearch.Models;
using CoachOnline.Implementation;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Services
{
    public interface ISearch
    {
        Task ReindexAll();
        Task ReindexByCourse(int courseId);
        Task ReindexCourses();
        Task ReindexCoaches();
        Task ReindexCategories();
        Task DeleteCourse(int courseId);
        Task DeleteCoach(int coachId);
        Task DeleteCategory(int categoryId);
        Task ReaddCategory(int categoryId);
        Task ReaddCoach(int coachId);
        Task ReaddCourse(int courseId);
        Task<IReadOnlyCollection<CourseIndex>> FindCourses(string query, int page = 1, int pageSize = 1000);
        Task<IReadOnlyCollection<CoachIndex>> FindCoaches(string query, int page = 1, int pageSize = 1000);
        Task<IReadOnlyCollection<CategoryIndex>> FindCategories(string query, int page = 1, int pageSize = 1000);
        Task<CombinedResults> SearchByCat(string searchQuery);
        Task<CombinedResults> Find(string query, int page = 1, int pageSize = 1000);
        Task<PlatformBasicInfoResponse> GetPlatformBasicInfo();
    }

    public class SearchSvc : ISearch
    {
        private bool IsCoursesReindexing { get; set; } = false;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SearchSvc> _logger;
        public SearchSvc(IServiceProvider svcProvider, ILogger<SearchSvc> logger)
        {
            _elasticClient = (IElasticClient)svcProvider.GetService(typeof(IElasticClient));
            _logger = logger;
        }


        public async Task<PlatformBasicInfoResponse> GetPlatformBasicInfo()
        {
            PlatformBasicInfoResponse resp = new PlatformBasicInfoResponse();
            using (var ctx = new DataContext())
            {

                var coaches = await ctx.users.Where(t => t.UserRole == UserRoleType.COACH && t.Status == UserAccountStatus.CONFIRMED).Include(c => c.OwnedCourses).ToListAsync();
                var categories = await ctx

                    .courseCategories
                    .Where(x => !x.ParentId.HasValue || x.ParentId == 0)

                    //.Include(c => c.CategoryCourses)
                    //.Include(x => x.Children)
                    //.ThenInclude(x => x.CategoryCourses)
                    .ToListAsync();




                resp.TotalCoaches = coaches.Where(t => t.OwnedCourses != null && t.OwnedCourses.Any(t => t.State == CourseState.APPROVED)).Count();


                resp.TotalCategories = categories
                    //.Where(t => CheckCourses(t.CategoryCourses) || (t.Children != null && t.Children.Any(o => CheckCourses(o.CategoryCourses))))
                    //.Where(t => !t.ParentId.HasValue || t.ParentId.Value == 0)
                    .Count();




                resp.TotalCourses = await ctx.courses.CountAsync(t => t.State == CourseState.APPROVED);

                resp.TotalMediaTime = await ctx.courses.Where(t => t.State == CourseState.APPROVED)
                    .Include(e => e.Episodes)
                    .SumAsync(e => e.Episodes.Sum(t => t.MediaLenght));

                resp.TotalMediaTime = Math.Round(resp.TotalMediaTime, 2);


                return resp;
            }
        }


        private bool CheckCourses(ICollection<Course> data)
        {
            return data != null && data.Any(x => x.State == CourseState.APPROVED);
        }


        public async Task<CombinedResults> Find(string query, int page = 1, int pageSize = 100)
        {
            var combined = new CombinedResults();

            combined.Courses = await FindCourses(query, page, pageSize);
            combined.Coaches = await FindCoaches(query, page, pageSize);
            combined.Categories = await FindCategories(query, page, pageSize);
            combined.Categories = combined.Categories.OrderBy(t => t.Name).ThenBy(t=>t.ChildCategories.OrderBy(t=>t.Name)).ToList();
            return combined;
        }

        public async Task<IReadOnlyCollection<CourseIndex>> FindCourses(string query, int page = 1, int pageSize = 1000)
        {
            if (query == null || query.Trim() == "")
            {
                pageSize = 10000;
            }
            else
            {
                pageSize = 50;
            }

            if (string.IsNullOrEmpty(query))
            {
                var response = await _elasticClient.SearchAsync<CourseIndex>
                (
                    s => s.Index(ConfigData.Config.ElasticSearch.CourseIdx).Query(q => q.QueryString(q => q.Fields(sf => sf
                    .Field(f => f.CourseName, 3)
                    .Field(f => f.CourseName_, 3)
                    .Field(f => f.Category.Name, 3)
                    .Field(f => f.Category.Name_, 3)
                    .Field(f => f.CourseDescription)
                    .Field(f => f.CourseDescription_)
                    .Field(f => f.Coach.LastName)
                    .Field(f => f.Coach.FirstName)
                     .Field(f => f.Coach.LastName_)
                    .Field(f => f.Coach.FirstName_)
                    .Field(f => f.Episodes.Select(t => t.Title))
                     .Field(f => f.Episodes.Select(t => t.Title_))
                    .Field(f => f.Episodes.Select(t => t.Description))

                    )
                    .Fuzziness(Fuzziness.Auto)
                    .PhraseSlop(2) //liczba wyrazów między frazą
                    .DefaultOperator(Operator.Or)
                    .Query(query + "*"))
                )
                  .From((page - 1) * pageSize)
                 .Size(pageSize));

                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search documents");
                    return new List<CourseIndex>();
                }

                return response.Documents;
            }
            else
            {

                var response = await _elasticClient.SearchAsync<CourseIndex>
                (
                    s => s.Index(ConfigData.Config.ElasticSearch.CourseIdx).Query(q => q.MultiMatch(m =>
                         m.Fields(sf => sf
                    //              .Field(f => f.CourseName, 3)
                    //.Field(f => f.Category.Name, 3)
                    //.Field(f => f.CourseDescription)
                    //.Field(f => f.Coach.LastName)
                    //.Field(f => f.Coach.FirstName)
                    //.Field(f => f.Episodes.Select(t => t.Title))
                    //.Field(f => f.Episodes.Select(t => t.Description))
                    ).Operator(Operator.Or)
                    .Type(TextQueryType.BestFields)
                   .Fuzziness(Fuzziness.AutoLength(5, 7))
                   .Slop(3)
                   .Analyzer("folding")

                   .Query(query)))
           .From((page - 1) * pageSize)
           .Size(pageSize));

                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search documents");
                    return new List<CourseIndex>();
                }

                return response.Documents;
            }
        }

        public async Task ReindexAll()
        {

            await ReindexCategories();
            Console.WriteLine("reindexed categories");
            await ReindexCourses();
            Console.WriteLine("reindexed courses");
            await ReindexCoaches();
            Console.WriteLine("reindexed coaches");
        }


        public async Task ReindexByCourse(int courseId)
        {


            using (var ctx = new DataContext())
            {
                var course = await ctx.courses.Where(t => t.Id == courseId).Include(c => c.Category).FirstOrDefaultAsync();
                if (course != null)
                {
                    if (course.Category != null)
                    {

                        await ReaddCategory(course.CategoryId);

                    }

                    await ReaddCoach(course.UserId);

                    await ReaddCourse(course.Id);
                }
            }


        }

        public async Task<IReadOnlyCollection<CategoryIndex>> FindCategories(string query, int page = 1, int pageSize = 1000)
        {
            if (query == null || query.Trim() == "")
            {
                pageSize = 1000;
            }
            else
            {
                pageSize = 1000;
            }

            if (string.IsNullOrEmpty(query))
            {

                var response = await _elasticClient.SearchAsync<CategoryIndex>
                (

                    s => s.Index(ConfigData.Config.ElasticSearch.CategoryIdx)
                    .Query(q => q.QueryString(q => q
                      .Fuzziness(Fuzziness.Auto)
                      .PhraseSlop(3) //liczba wyrazów między frazą
                      .DefaultOperator(Operator.Or)
                      .Query(query + "*"))
                )
                    .From((page - 1) * pageSize)
                .Size(pageSize));

                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search documents");
                    return new List<CategoryIndex>();
                }

                return response.Documents;
            }
            else
            {
                var response = await _elasticClient.SearchAsync<CategoryIndex>
                (
                    s => s.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(q => q.MultiMatch(m =>
                         m.Fields(sf => sf
                         .Field(f => f.Name,2)
                         .Field(f=>f.ParentName)
                         .Field(f=>f.ChildCategories.Select(t=>t.Name))
                         .Field(f=>f.Courses.Select(t=>t.Coach.Categories))
                 //        .Field(f => f.Email)
                 //        .Field(f => f.FirstName)
                 //        .Field(f => f.City).Field(f => f.Bio))
                 ).Operator(Operator.Or)

                  .Type(TextQueryType.BestFields)
                   .Fuzziness(Fuzziness.AutoLength(5, 7))
                   .Slop(3)
                   .Analyzer("folding")
                   .Query(query)
                  ))
           .From((page - 1) * pageSize)
           .Size(pageSize));

                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search documents");
                    return new List<CategoryIndex>();
                }

                return response.Documents;
            }

        }

        public async Task<IReadOnlyCollection<CoachIndex>> FindCoaches(string query, int page = 1, int pageSize = 1000)
        {
            if (query == null || query.Trim() == "")
            {
                pageSize = 1000;
            }
            else
            {
                pageSize = 10;
            }

            if (string.IsNullOrEmpty(query))
            {
                var response = await _elasticClient.SearchAsync<CoachIndex>
          (
              s => s.Index(ConfigData.Config.ElasticSearch.CoachIdx).Query(q => q.QueryString(q => q
                  //q.Fields(sf => sf
                  //        .Field(f => f.LastName, 2)
                  //        .Field(f => f.Email)
                  //        .Field(f => f.FirstName)
                  //        .Field(f => f.City).Field(f => f.Bio
                  .Fuzziness(Fuzziness.Auto)
                  .PhraseSlop(3) //liczba wyrazów między frazą
                  .DefaultOperator(Operator.Or)
                  .Query(query + "*")))


           .From((page - 1) * pageSize)
           .Size(pageSize));



                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search coach documents");
                    return new List<CoachIndex>();
                }

                return response.Documents;
            }
            else
            {
                var response = await _elasticClient.SearchAsync<CoachIndex>
                (
                    s => s.Index(ConfigData.Config.ElasticSearch.CoachIdx).Query(q => q.MultiMatch(m =>
                         m.Fields(sf => sf
                 //        .Field(f => f.LastName, 2)
                 //        .Field(f => f.Email)
                 //        .Field(f => f.FirstName)
                 //        .Field(f => f.City).Field(f => f.Bio))
                 ).Operator(Operator.Or)
                   .Fuzziness(Fuzziness.Auto)
                   .Type(TextQueryType.MostFields)
                   .Slop(3)
                   .Analyzer("folding")

                   //.PhraseSlop(3) //liczba wyrazów między frazą
                   //.DefaultOperator(Operator.Or)
                   .Query(query)
                  ))
           .From((page - 1) * pageSize)
           .Size(pageSize));


                if (!response.IsValid)
                {
                    _logger.LogInformation("Failed to search coach documents");
                    return new List<CoachIndex>();
                }

                return response.Documents;
            }




        }


        public async Task<CombinedResults> SearchByCat(string searchQuery)
        {
            if(string.IsNullOrEmpty(searchQuery))
            {
                return await Find(searchQuery);
            }

            var splitted = searchQuery.Split(",");
 
            var data = await FindCategories(searchQuery, 1, 1000);

            var combined = new CombinedResults();

            var courses= new List<CourseIndex>();
            var coaches = new List<CoachIndex>();
            combined.Categories = data;

            foreach(var d in data)
            {   

                foreach (var child in d.ChildCategories)
                {

                    foreach (var c in child.Courses)
                    {

                        if (splitted.Any(t => t.Trim().ToLower() == child.Name.Trim().ToLower()))
                        {
                            Console.WriteLine($"searching by name: {child.Name}");
                            c.Category = new CategoryIndex() { Id = child.Id, Name = child.Name, ParentId = child.ParentId, AdultOnly = child.AdultOnly, ParentName = child.ParentName };
                            courses.Add(c);
                            if (!coaches.Any(t => t.UserId == c.CoachId))
                            {
                                coaches.Add(c.Coach);
                            }
                        }
                        else if(splitted.Any(t => t.Trim().ToLower() == child.ParentName.Trim().ToLower()))
                        {
                            c.Category = new CategoryIndex() { Id = child.Id, Name = child.Name, ParentId = child.ParentId, AdultOnly = child.AdultOnly, ParentName = child.ParentName };
                            courses.Add(c);
                            if (!coaches.Any(t => t.UserId == c.CoachId))
                            {
                                coaches.Add(c.Coach);
                            }
                        }
                    }
                }

                var catIdx = new CategoryIndex() { Id = d.Id, Name = d.Name, ParentId = d.ParentId, AdultOnly = d.AdultOnly, ParentName = d.ParentName };
                foreach (var c in d.Courses)
                {
                    if (splitted.Any(t => t.Trim().ToLower() == catIdx.Name.Trim().ToLower()))
                    {
                        c.Category = catIdx;
                        courses.Add(c);
                        if (!coaches.Any(t => t.UserId == c.CoachId))
                        {
                            coaches.Add(c.Coach);
                        }
                    }
                }


            }

            combined.Courses = courses;
            combined.Coaches = coaches;

            return combined;
        }


        public async Task ReaddCourse(int courseId)
        {

            using (var ctx = new DataContext())
            {
                var c = await ctx.courses.Where(s => s.Id == courseId).Include(e=>e.Evaluations).Include(e => e.Episodes).Include(u => u.User).Include(c => c.Category).ThenInclude(p => p.Parent).FirstOrDefaultAsync();

                if (c != null)
                {
                    if (c.State == CourseState.APPROVED)
                    {
                        var courseIdx = new CourseIndex();

                        courseIdx.Category = new CategoryIndex { Id = c.Category.Id, Name = c.Category.Name, Name_ = Helpers.Extensions.RemoveDiacritics(c.Category.Name), 
                            ParentId = c.Category.ParentId, ParentName = c.Category?.Parent?.Name, ParentName_ = Helpers.Extensions.RemoveDiacritics(c.Category?.Parent?.Name) };
                        courseIdx.CategoryId = c.Category.Id;
                        courseIdx.CoachId = c.UserId;
                        if (c.User != null)
                        {
                            var coachCats = await GetCoachCategories(c.UserId);
                            courseIdx.Coach = new CoachIndex
                            { UserId = c.User.Id, Bio = c.User.Bio, City = c.User.City, Email = c.User.EmailAddress, 
                                FirstName = c.User.FirstName, FirstName_ = Helpers.Extensions.RemoveDiacritics(c.User.FirstName), LastName = c.User.Surname, LastName_ = Helpers.Extensions.RemoveDiacritics(c.User.Surname),
                                PhotoUrl = c.User.AvatarUrl, Categories = coachCats };
                        }
                        courseIdx.CourseDescription = c.Description;
                        courseIdx.CourseDescription_ = Helpers.Extensions.RemoveDiacritics(c.Description);
                        courseIdx.CourseId = c.Id;
                        courseIdx.CourseName = c.Name;
                        courseIdx.CourseName_ = Helpers.Extensions.RemoveDiacritics(c.Name);
                        courseIdx.CoursePhotoUrl = c.PhotoUrl;
                        courseIdx.HasPromo = c.HasPromo.HasValue && c.HasPromo.Value;
                        courseIdx.BannerPhotoUrl = c.BannerPhotoUrl;
                        courseIdx.Episodes = new List<EpisodeIndex>();
                        courseIdx.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;
                        if (c.Episodes != null)
                        {
                            foreach (var e in c.Episodes)
                            {
                                courseIdx.Episodes.Add(new EpisodeIndex
                                {
                                    EpisodeId = e.Id,
                                    CourseId = c.Id,
                                    Title = e.Title,
                                    Title_ = Helpers.Extensions.RemoveDiacritics(e.Title),
                                    Description = e.Description,
                                    OrdinalNumber = e.OrdinalNumber,
                                    IsPromo = e.IsPromo.HasValue && e.IsPromo.Value
                                });
                            }
                        }




                        await _elasticClient.IndexAsync<CourseIndex>(new IndexRequest<CourseIndex>(courseIdx, ConfigData.Config.ElasticSearch.CourseIdx, courseIdx.CourseId));

                        //Console.WriteLine("course reindexed");
                    }
                    else
                    {

                        await _elasticClient.DeleteAsync<CourseIndex>(courseId, i => i.Index(ConfigData.Config.ElasticSearch.CourseIdx));
                        // Console.WriteLine("course deleted");
                    }

                }

            }
        }

        public async Task ReindexCourses()
        {

            using (var ctx = new DataContext())
            {
                var courses = await ctx.courses.Where(s => s.State == Model.CourseState.APPROVED).Include(e=>e.Evaluations).Include(e => e.Episodes).Include(u => u.User).Include(c => c.Category).ThenInclude(p => p.Parent).ToListAsync();
                var existingCoursesList = new List<int>();
                foreach (var c in courses)
                {
                    existingCoursesList.Add(c.Id);
                    var courseIdx = new CourseIndex();

                    courseIdx.Category = new CategoryIndex { Id = c.Category.Id, Name = c.Category.Name, Name_ = Helpers.Extensions.RemoveDiacritics(c.Category.Name), 
                        ParentId = c.Category.ParentId, ParentName = c.Category?.Parent?.Name, ParentName_ = Helpers.Extensions.RemoveDiacritics(c.Category?.Parent?.Name)
                    };
                    courseIdx.CategoryId = c.Category.Id;
                    courseIdx.CoachId = c.UserId;
                    if (c.User != null)
                    {
                        var coachCats = await GetCoachCategories(c.UserId);
                        courseIdx.Coach = new CoachIndex
                        { UserId = c.User.Id, Bio = c.User.Bio, City = c.User.City, Email = c.User.EmailAddress, FirstName = c.User.FirstName, 
                            FirstName_ = Helpers.Extensions.RemoveDiacritics(c.User.FirstName), LastName_ = Helpers.Extensions.RemoveDiacritics(c.User.Surname),  LastName = c.User.Surname, PhotoUrl = c.User.AvatarUrl, Categories = coachCats };
                    }
                    courseIdx.CourseDescription = c.Description;
                    courseIdx.CourseDescription_ = Helpers.Extensions.RemoveDiacritics(c.Description);
                    courseIdx.CourseId = c.Id;
                    courseIdx.CourseName = c.Name;
                    courseIdx.CourseName_ = Helpers.Extensions.RemoveDiacritics(c.Name);
                    courseIdx.CoursePhotoUrl = c.PhotoUrl;
                    courseIdx.BannerPhotoUrl = c.BannerPhotoUrl;
                    courseIdx.HasPromo = c.HasPromo.HasValue && c.HasPromo.Value;
                    courseIdx.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;
                    courseIdx.Episodes = new List<EpisodeIndex>();
                    if (c.Episodes != null)
                    {
                        foreach (var e in c.Episodes)
                        {
                            courseIdx.Episodes.Add(new EpisodeIndex
                            {
                                EpisodeId = e.Id,
                                CourseId = c.Id,
                                Title = e.Title,
                                Title_ = Helpers.Extensions.RemoveDiacritics(e.Title),
                                Description = e.Description,
                                OrdinalNumber = e.OrdinalNumber,
                                IsPromo = e.IsPromo.HasValue && e.IsPromo.Value
                            });
                        }
                    }

                    await _elasticClient.IndexAsync<CourseIndex>(new IndexRequest<CourseIndex>(courseIdx, ConfigData.Config.ElasticSearch.CourseIdx, courseIdx.CourseId));
                }

                var allCourses = await _elasticClient.SearchAsync<CourseIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CourseIdx).Query(rq => rq.MatchAll()));

                var docs = allCourses.Documents.ToList();

                foreach (var d in docs)
                {
                    if (!existingCoursesList.Any(t => t == d.CourseId))
                    {
                        await _elasticClient.DeleteAsync<CourseIndex>(d.CourseId, q => q.Index(ConfigData.Config.ElasticSearch.CourseIdx));
                    }
                }
            }
        }

        public async Task ReindexCoaches()
        {
            using (var ctx = new DataContext())
            {
                var coaches = await ctx.users.Where(t => t.UserRole == Model.UserRoleType.COACH && t.Status == UserAccountStatus.CONFIRMED).ToListAsync();
                var existingCoaches = new List<int>();
                foreach (var c in coaches)
                {

                    existingCoaches.Add(c.Id);

                    CoachIndex idx = new CoachIndex();
                    idx.Bio = c.Bio;
                    idx.Bio_ = Helpers.Extensions.RemoveDiacritics(c.Bio);
                    idx.City = c.City;
                    idx.Email = c.EmailAddress;
                    idx.FirstName = c.FirstName;
                    idx.FirstName_ = Helpers.Extensions.RemoveDiacritics(c.FirstName);
                    idx.LastName = c.Surname;
                    idx.LastName_ = Helpers.Extensions.RemoveDiacritics(c.Surname);
                    idx.PhotoUrl = c.AvatarUrl;
                    idx.UserId = c.Id;
                    idx.Gender = c.Gender;
                    idx.Categories = await GetCoachCategories(c.Id);
                    idx.Courses = await GetCoachCourses(c.Id);

                    //await _elasticClient.DeleteByQueryAsync<CoachIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CoachIdx).Query(rq => rq.Match(m => m.Field(f => f.UserId.ToString()).Query(c.Id.ToString()))));
                    //await _elasticClient.IndexDocumentAsync(idx);
                    //await _elasticClient.IndexAsync(idx, i => i.Index(ConfigData.Config.ElasticSearch.CoachIdx));
                    await _elasticClient.IndexAsync<CoachIndex>(new IndexRequest<CoachIndex>(idx, ConfigData.Config.ElasticSearch.CoachIdx, idx.UserId));
                }

                var allCoaches = await _elasticClient.SearchAsync<CoachIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CoachIdx).Query(rq => rq.MatchAll()));

                var docs = allCoaches.Documents.ToList();
                // Console.WriteLine("Getting all the coaches docs in reindex");
                foreach (var d in docs)
                {
                    if (!existingCoaches.Any(t => t == d.UserId))
                    {
                        Console.WriteLine("Deleting coach " + d.FirstName + " " + d.LastName);
                        await _elasticClient.DeleteAsync<CoachIndex>(d.UserId, q => q.Index(ConfigData.Config.ElasticSearch.CoachIdx));
                    }
                }
            }
        }

        private async Task<List<CourseIndex>> GetCoachCourses(int userId)
        {
            var data = new List<CourseIndex>();

            using (var ctx = new DataContext())
            {
                var courses = await ctx.courses.Where(t => t.UserId == userId && t.State == CourseState.APPROVED).Include(e=>e.Evaluations).Include(e => e.Episodes).Include(c => c.Category).ThenInclude(p => p.Parent).ToListAsync();

                foreach (var c in courses)
                {
                    var idx = new CourseIndex();
                    idx.CategoryId = c.CategoryId;
                    idx.Category = new CategoryIndex() { AdultOnly = c.Category.AdultOnly, Id = c.Category.Id, Name = c.Category.Name, ParentId = c.Category.ParentId, ParentName = c.Category?.Parent?.Name };
                    idx.CoachId = c.UserId;
                    idx.CourseId = c.Id;
                    idx.CourseDescription = c.Description;
                    idx.CourseName = c.Name;
                    idx.CoursePhotoUrl = c.PhotoUrl;
                    idx.BannerPhotoUrl = c.BannerPhotoUrl;
                    idx.HasPromo = c.HasPromo.HasValue ? c.HasPromo.Value : false;
                    idx.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;
                    idx.Episodes = new List<EpisodeIndex>();
                    if (c.Episodes != null)
                    {
                        foreach (var e in c.Episodes)
                        {
                            idx.Episodes.Add(new EpisodeIndex
                            {
                                EpisodeId = e.Id,
                                CourseId = c.Id,
                                Title = e.Title,
                                Description = e.Description,
                                OrdinalNumber = e.OrdinalNumber,
                                IsPromo = e.IsPromo.HasValue && e.IsPromo.Value
                            });
                        }
                    }
                    data.Add(idx);
                }
            }

            return data;
        }

        public async Task DeleteCoach(int coachId)
        {
            await _elasticClient.DeleteAsync<CoachIndex>(coachId, q => q.Index(ConfigData.Config.ElasticSearch.CoachIdx));
        }

        public async Task DeleteCategory(int categoryId)
        {
            await _elasticClient.DeleteAsync<CategoryIndex>(categoryId, q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx));
        }

        public async Task ReaddCategory(int categoryId)
        {
            using (var ctx = new DataContext())
            {
                var cat = await ctx.courseCategories.Where(t => t.Id == categoryId).Include(c => c.Children).Include(p => p.Parent).Include(c => c.CategoryCourses).ThenInclude(e=>e.Evaluations).FirstOrDefaultAsync();
                if (cat != null && cat.Parent == null)
                {
                    await _elasticClient.DeleteByQueryAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(rq => rq.Match(m => m.Field(f => f.Id.ToString()).Query(cat.Id.ToString()))));

                    var catIdx = await ReccurentCategories(cat, cat.Children.ToList());

                    await _elasticClient.IndexAsync<CategoryIndex>(new IndexRequest<CategoryIndex>(catIdx, ConfigData.Config.ElasticSearch.CategoryIdx, catIdx.Id));
                }
                else
                {
                    var catParent = await ctx.courseCategories.Where(t => t.Id == cat.ParentId).Include(c => c.Children).Include(p => p.Parent).Include(c => c.CategoryCourses).ThenInclude(e => e.Evaluations).FirstOrDefaultAsync();
                    if (catParent != null)
                    {
                        await _elasticClient.DeleteByQueryAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(rq => rq.Match(m => m.Field(f => f.Id.ToString()).Query(catParent.Id.ToString()))));

                        var catIdx = await ReccurentCategories(catParent, catParent.Children.ToList());

                        await _elasticClient.IndexAsync<CategoryIndex>(new IndexRequest<CategoryIndex>(catIdx, ConfigData.Config.ElasticSearch.CategoryIdx, catIdx.Id));
                    }
                }
            }
        }

        public async Task ReaddCoach(int coachId)
        {
            using (var ctx = new DataContext())
            {
                var c = await ctx.users.Where(t => t.Id == coachId).FirstOrDefaultAsync();

                await _elasticClient.DeleteByQueryAsync<CoachIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CoachIdx).Query(rq => rq.Match(m => m.Field(f => f.UserId.ToString()).Query(c.Id.ToString()))));

                CoachIndex idx = new CoachIndex();
                idx.Bio = c.Bio;
                idx.Bio_ = Helpers.Extensions.RemoveDiacritics(c.Bio);
                idx.City = c.City;
                idx.Email = c.EmailAddress;
                idx.FirstName = c.FirstName;
                idx.FirstName_ = Helpers.Extensions.RemoveDiacritics(c.FirstName);
                idx.LastName = c.Surname;
                idx.LastName_ = Helpers.Extensions.RemoveDiacritics(c.Surname);
                idx.PhotoUrl = c.AvatarUrl;
                idx.UserId = c.Id;
                idx.Categories = await GetCoachCategories(c.Id);

                //await _elasticClient.IndexDocumentAsync(idx);
                await _elasticClient.IndexAsync<CoachIndex>(idx, i => i.Index(ConfigData.Config.ElasticSearch.CoachIdx));

            }

        }

        public async Task ReindexCategories()
        {
            try
            {
                //await _elasticClient.DeleteByQueryAsync<CategoryIndex>(q => q.MatchAll());
                var existingCategories = new List<int>();
                using (var ctx = new DataContext())
                {
                    //root categories

                    var categories = await ctx.courseCategories.Where(t => !t.ParentId.HasValue || t.ParentId.Value == 0).Include(c => c.Children).Include(c => c.CategoryCourses).ThenInclude(e => e.Evaluations).ToListAsync();

                    //var getAllCatIdx = await _elasticClient.SearchAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).MatchAll());

                    foreach (var cat in categories)
                    {
                        Console.WriteLine("Reindexing Category " + cat.Name);
                        existingCategories.Add(cat.Id);
                        // Console.WriteLine("deleting category " +cat.Id);
                        // await _elasticClient.DeleteByQueryAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(rq => rq.Match(m => m.Field(f => f.CategoryId.ToString()).Query(cat.Id.ToString()))));
                        //  Console.WriteLine("deleted");

                        var catIdx = await ReccurentCategories(cat, cat.Children.ToList());

                        //await _elasticClient.IndexAsync(catIdx, i => i.Index(ConfigData.Config.ElasticSearch.CategoryIdx));
                        await _elasticClient.IndexAsync<CategoryIndex>(new IndexRequest<CategoryIndex>(catIdx, ConfigData.Config.ElasticSearch.CategoryIdx, catIdx.Id));

                    }

                    var allCategories = await _elasticClient.SearchAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(rq => rq.MatchAll()));

                    var docs = allCategories.Documents.ToList();
                    Console.WriteLine("Getting all the categories docs in reindex");
                    foreach (var d in docs)
                    {
                        if (!existingCategories.Any(t => t == d.Id))
                        {
                            Console.WriteLine("Deleting category " + d.Name);
                            await _elasticClient.DeleteAsync<CategoryIndex>(d.Id, q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx));
                            //await _elasticClient.DeleteByQueryAsync<CategoryIndex>(q => q.Index(ConfigData.Config.ElasticSearch.CategoryIdx).Query(rq => rq.Match(m => m.Field(f => f.CategoryId.ToString()).Query(d.CategoryId.ToString()))));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task DeleteCourse(int courseId)
        {
            await _elasticClient.DeleteAsync<CourseIndex>(courseId, q => q.Index(ConfigData.Config.ElasticSearch.CourseIdx));
        }


        private async Task<List<CoachCategories>> GetCoachCategories(int coachId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == coachId).Include(c => c.AccountCategories).FirstOrDefaultAsync();
                var categories = user.AccountCategories;
                List<CoachCategories> cats = new List<CoachCategories>();
                if (categories != null)
                {
                    categories.ForEach(el =>
                    {
                        cats.Add(new CoachCategories { Id = el.Id, Name = el.Name});
                    });
                }

                return cats;
            }
        }
        private async Task<CoachIndex> GetCoachDataByCourse(Course c)
        {
            CoachIndex coach = new CoachIndex();

            using (var ctx = new DataContext())
            {
                var data = await ctx.courses.Where(x => x.Id == c.Id).Include(x => x.User).FirstOrDefaultAsync();
                if (data != null)
                {
                    coach.Bio = data.User.Bio;
                    coach.Bio_ = Helpers.Extensions.RemoveDiacritics(data.User.Bio);
                    coach.City = data.User.City;
                    coach.Email = data.User.EmailAddress;
                    coach.FirstName = data.User.FirstName;
                    coach.LastName = data.User.Surname;
                    coach.FirstName_ = Helpers.Extensions.RemoveDiacritics(data.User.FirstName);
                    coach.LastName_ = Helpers.Extensions.RemoveDiacritics(data.User.Surname);
                    coach.PhotoUrl = data.User.AvatarUrl;
                    coach.UserId = data.User.Id;
                    coach.Gender = data.User.Gender;

                    coach.Categories = await GetCoachCategories(data.User.Id);



                }


            }

            return coach;
        }

        private async Task<CategoryIndex> ReccurentCategories(Category cat, List<Category> children)
        {
            var catIdx = new CategoryIndex();
            catIdx.Id = cat.Id;
            catIdx.Name = cat.Name;
            catIdx.Name_ = Helpers.Extensions.RemoveDiacritics(cat.Name);
            catIdx.ParentId = cat.ParentId;
            catIdx.ParentName = cat.Parent?.Name;
            catIdx.ParentName_ = Helpers.Extensions.RemoveDiacritics(cat.Parent?.Name);
            catIdx.AdultOnly = cat.AdultOnly;

            //courses in category
            if (cat.CategoryCourses != null)
            {
                foreach (var c in cat.CategoryCourses)
                {
                    if (c.State == CourseState.APPROVED)
                    {
                        var coach = await GetCoachDataByCourse(c);
                        catIdx.Courses.Add(new CourseIndex
                        {
                            CategoryId = cat.Id,
                            CourseDescription = c.Description,
                            CourseDescription_ = Helpers.Extensions.RemoveDiacritics(c.Description),
                            CourseId = c.Id,
                            CourseName = c.Name,
                            CourseName_ = Helpers.Extensions.RemoveDiacritics(c.Name),
                            CoursePhotoUrl = c.PhotoUrl,
                            BannerPhotoUrl = c.BannerPhotoUrl,
                            LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0,
                            CoachId = c.UserId,
                            Coach = coach
                        }) ; 
                    }
                }
            }

            //process child categories
            if (children != null)
            {
                foreach (var child in children)
                {
                    using (var ctx = new DataContext())
                    {
                        var category = await ctx.courseCategories.Where(t => t.Id == child.Id).Include(p => p.Parent).Include(c => c.Children).Include(c => c.CategoryCourses).ThenInclude(e => e.Evaluations).FirstOrDefaultAsync();
                        if (category != null)
                        {
                            var childIdx = await ReccurentCategories(category, category.Children?.ToList());

                            catIdx.ChildCategories.Add(childIdx);
                        }
                    }
                }
            }

            return catIdx;
        }



        private static string GetSearchUrl(string query, int page, int pageSize)
        {
            return $"/search?query={Uri.EscapeDataString(query ?? "")}&page={page}&pagesize={pageSize}/";
        }

    }
}

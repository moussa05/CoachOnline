using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.B2B;
using CoachOnline.Mongo;
using CoachOnline.Mongo.Models;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class LibraryManagementService : ILibraryManagement
    {
        private readonly ILogger<LibraryManagementService> _logger;
        private readonly MongoCtx _mongoCtx;
        public LibraryManagementService(ILogger<LibraryManagementService> logger, MongoCtx mongoCtx)
        {
            _logger = logger;
            _mongoCtx = mongoCtx;
        }


        public async Task<LibraryBasicInfoRespWithAccountType> GetLibraryInfo(int libraryId)
        {
            var resp = new LibraryBasicInfoRespWithAccountType();

            using(var ctx = new DataContext())
            {
                var lib = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE);
                lib.CheckExist("Library");

                resp.Email = lib.Email;
                resp.Id = lib.Id;
                resp.LibraryName = lib.LibraryName;
                resp.PhotoUrl = lib.LogoUrl;
                resp.Website = lib.Website;
                resp.AccountType = B2BAccountType.LIBRARY_ACCOUNT;
                
            }

            return resp;
        }

        public async Task UpdateLibraryAccountPassword(int libraryId, string secret, string repeat, string oldPassword)
        {
            using(var ctx = new DataContext())
            {
                var library = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE);
                library.CheckExist("Library");
                
                if(secret != repeat)
                {
                    throw new CoachOnlineException("Passwords don't match.", CoachOnlineExceptionState.PasswordsNotMatch);
                }

                if(LetsHash.ToSHA512(oldPassword) != library.Password)
                {
                    throw new CoachOnlineException("Wrong password", CoachOnlineExceptionState.WrongPassword);
                }

                var hashed = LetsHash.ToSHA512(secret);

                library.Password = hashed;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<int> GetLibraryAccountIdByToken(string token)
        {
            using (var ctx = new DataContext())
            {
               var auth = await ctx.LibraryAccessTokens.Where(t => t.Token == token).Include(x=>x.LibraryAccount).FirstOrDefaultAsync();
               auth.CheckExist("Token");

                if(auth.Disposed)
                {
                    throw new CoachOnlineException("Token is not active", CoachOnlineExceptionState.TokenNotActive);
                }

                if(auth.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    auth.Disposed = true;
                    await ctx.SaveChangesAsync();

                    throw new CoachOnlineException("Token is not active", CoachOnlineExceptionState.TokenNotActive);
                }

                if (auth.LibraryAccount.AccountStatus == AccountStatus.DELETED)
                {
                    throw new CoachOnlineException("Library Account does not exist.", CoachOnlineExceptionState.NotExist);
                }

                return auth.LibraryAccountId;
            }
        }

        public async Task<string> Login(string email, string password)
        {
            using(var ctx = new DataContext())
            {
                var lib = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Email == email && t.AccountStatus == AccountStatus.ACTIVE);
                if(lib == null)
                {
                    throw new CoachOnlineException("Library with given email does not exist", CoachOnlineExceptionState.NotExist);
                }

                var pass = LetsHash.ToSHA512(password);

                if(pass != lib.Password)
                {
                    throw new CoachOnlineException("Wrong password", CoachOnlineExceptionState.WrongPassword);
                }

                LibraryAcessToken token = new LibraryAcessToken();
                token.LibraryAccountId = lib.Id;
                token.Token = LetsHash.RandomHash(lib.Password);
                token.Created = ConvertTime.ToUnixTimestampLong(DateTime.Now);
                token.Disposed = false;
                token.ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30));

                ctx.LibraryAccessTokens.Add(token);

                await ctx.SaveChangesAsync();

                return token.Token;

            }
        }

        public async Task<int> GetConnectionsLimitForLibrary(int libraryId)
        {
            using(var ctx = new DataContext())
            {
                var lib = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE).Include(i => i.Subscriptions).FirstOrDefaultAsync();
                lib.CheckExist("Library");

                if(lib.Subscriptions != null && lib.Subscriptions.Count >0)
                {
                    var activeSub = lib.Subscriptions.FirstOrDefault(x => x.Status == LibrarySubscriptionStatus.ACTIVE);
                    if(activeSub != null)
                    {
                        return activeSub.NumberOfActiveUsers;
                    }
                }

                return 0;
            }
        }

        public async Task<int> GetCurrentConnections(int libraryId)
        {
            var allConnections = await _mongoCtx.InstitureUsersCollection.FindByInstituteId(libraryId);

            return allConnections.Where(t => !t.ConnectionEndTime.HasValue).Count();
        }

        public async Task<int> IsUserCurrentlyConnectedAndAllowed(int libraryId, int userId)
        {
            var allConnections = await _mongoCtx.InstitureUsersCollection.FindByUserId(userId);

            return allConnections.Where(t => t.InstituteId == libraryId && !t.ConnectionEndTime.HasValue && t.IsAllowedToView.HasValue && t.IsAllowedToView.Value).Count();
        }

        public async Task<int> GetRegisteredUsersFilteredByProfessionGenderAndAgeGroup(int institutionId, int? professionId, string gender, int? ageGroupStart, int? ageGroupEnd)
        {
            using(var ctx = new DataContext())
            {
                var users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId).ToListAsync();
                if(!string.IsNullOrEmpty(gender))
                {
                    users = users.Where(t => t.Gender == gender).ToList();
                }

                if(professionId.HasValue)
                {
                    users = users.Where(t => t.ProfessionId.HasValue && t.ProfessionId == professionId.Value).ToList();
                }

                if(ageGroupStart.HasValue && ageGroupEnd.HasValue)
                {
                    var year = DateTime.Now.Year;

                    users = users.Where(t => t.YearOfBirth.HasValue && ageGroupStart.Value <= (year - t.YearOfBirth.Value) && ageGroupEnd.Value >= (year - t.YearOfBirth.Value)).ToList();
                }

                return users.Count;
            }
        }

        public async Task<LibraryConnectionsByKeyHeaderResponse> GetLibraryStatsByKey(int libraryId, string key, DateTime? startDate, DateTime? endDate)
        {
            using (var ctx = new DataContext())
            {
                bool isDateRange = startDate.HasValue && endDate.HasValue;

                List<InstituteUserConnection> allConnections = await _mongoCtx.InstitureUsersCollection.FindByInstituteId(libraryId);
                List<User> users = null;
                if(isDateRange)
                {
                    if(endDate.Value < startDate.Value)
                    {
                        throw new CoachOnlineException("Wrong date range", CoachOnlineExceptionState.DataNotValid);
                    }

                    allConnections = allConnections.Where(x => x.ConnectionStartTime >= startDate.Value && x.ConnectionEndTime <= endDate.Value).ToList();
                    users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == libraryId && t.UserRole == UserRoleType.INSTITUTION_STUDENT 
                    && t.AccountCreationDate.HasValue && t.AccountCreationDate.Value>= startDate.Value && t.AccountCreationDate.Value <= endDate.Value).ToListAsync();
                }
                else
                {
                    users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == libraryId && t.UserRole == UserRoleType.INSTITUTION_STUDENT).ToListAsync();
                }

                var resp = new LibraryConnectionsByKeyHeaderResponse();
                resp.TotalRegisteredUsers = users.Count;
                resp.CurrentlyConnected = allConnections.Where(t => !t.ConnectionEndTime.HasValue).Count();

                resp.TotalConnections = allConnections.Count;

                resp.ConnectionsTotalTime = Math.Round(allConnections.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);

                if (resp.TotalConnections > 0)
                {
                    resp.ConnectrionsAverageTime = Math.Round(allConnections.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                }

                resp.Key = key;

                resp.Data = new List<LibraryConnectionsByKeyResponse>();

                if(key == "PROFESSION")
                {
                    var data = await GetConnectionsByStudentProfessions(allConnections, libraryId, startDate, endDate);

                    foreach(var d in data)
                    {
                        resp.Data.Add(new LibraryConnectionsByKeyResponse
                        {
                            Id = d.ProfessionId,
                            Name = d.ProfessionName,
                            ConnectionsTotalTime = d.ConnectionsTotalTime,
                            ConnectrionsAverageTime = d.ConnectrionsAverageTime,
                            TotalConnections = d.TotalConnections,
                            TotalRegisteredUsers = d.TotalRegisteredUsers
                        });
                    }
                }
                else if(key =="GENDER")
                {
                    var data = await GetConnectionsByStudentGender(allConnections, libraryId, startDate, endDate);

                    foreach (var d in data)
                    {
                        resp.Data.Add(new LibraryConnectionsByKeyResponse
                        {
                            Id = d.Id,
                            Name = d.Gender,
                            ConnectionsTotalTime = d.ConnectionsTotalTime,
                            ConnectrionsAverageTime = d.ConnectrionsAverageTime,
                            TotalConnections = d.TotalConnections,
                            TotalRegisteredUsers = d.TotalRegisteredUsers
                        });
                    }
                }
                else if (key == "AGE")
                {
                    var data = await GetConnectionsByStudentBirthDate(allConnections, libraryId, startDate, endDate);

                    foreach (var d in data)
                    {
                        resp.Data.Add(new LibraryConnectionsByKeyResponse
                        {
                            Id = d.Id,
                            Name =d.Label,
                            ConnectionsTotalTime = d.ConnectionsTotalTime,
                            ConnectrionsAverageTime = d.ConnectrionsAverageTime,
                            TotalConnections = d.TotalConnections,
                            TotalRegisteredUsers = d.TotalRegisteredUsers
                        });
                    }
                }
                else if(key == "CITY")
                {
                    var data = await GetConnectionsByStudentCity(allConnections, libraryId, startDate, endDate);

                    foreach (var d in data)
                    {
                        resp.Data.Add(new LibraryConnectionsByKeyResponse
                        {
                            Id = d.Id,
                            Name = d.City,
                            ConnectionsTotalTime = d.ConnectionsTotalTime,
                            ConnectrionsAverageTime = d.ConnectrionsAverageTime,
                            TotalConnections = d.TotalConnections,
                            TotalRegisteredUsers = d.TotalRegisteredUsers
                        });
                    }
                }
                else if (key == "REGION")
                {
                    var data = await GetConnectionsByStudentRegion(allConnections, libraryId, startDate, endDate);

                    foreach (var d in data)
                    {
                        resp.Data.Add(new LibraryConnectionsByKeyResponse
                        {
                            Id = d.Id,
                            Name = d.Region,
                            ConnectionsTotalTime = d.ConnectionsTotalTime,
                            ConnectrionsAverageTime = d.ConnectrionsAverageTime,
                            TotalConnections = d.TotalConnections,
                            TotalRegisteredUsers = d.TotalRegisteredUsers
                        });
                    }
                }

                return resp;
            }
        }

            public async Task<List<LibraryChartDataResponse>> GetRegisteredUsersForChart(int institutionId, DateTime? startDate, DateTime? endDate)
        {
            using(var ctx = new DataContext())
            {
                var users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.UserRole == UserRoleType.INSTITUTION_STUDENT && t.ProfessionId.HasValue).ToListAsync();

                if(startDate.HasValue && endDate.HasValue)
                {
                    users = users.Where(t => t.AccountCreationDate.HasValue && t.AccountCreationDate >= startDate.Value && t.AccountCreationDate <= endDate.Value).ToList();
                }

                var professions = await ctx.Professions.ToListAsync();

                var returnData = new List<LibraryChartDataResponse>();
                foreach(var prof in professions)
                {
                    LibraryChartDataResponse resp = new LibraryChartDataResponse();
                    resp.Id = prof.Id;
                    resp.Name = prof.Name;
                    var ageGroups = new List<AgesChartDataResponse>();
                    for (int i = 0; i < 100; i = i + 5)
                    {
                        ageGroups.Add(new AgesChartDataResponse() { AgeGroup = $"{i}-{i+4}", StartAge = i, EndAge = i + 4, Female = 0, Male = 0, Sum = 0 });
                    }
                    resp.Ages = ageGroups;

                    var usersByProf = users.Where(t => t.ProfessionId == prof.Id).ToList();

                    if(usersByProf.Count>0)
                    {
                        foreach(var u in usersByProf)
                        {
                            var age = DateTime.Now.Year - u.YearOfBirth;

                            var ageGroup = resp.Ages.FirstOrDefault(t => t.StartAge <= age && t.EndAge >= age);

                            if(ageGroup!= null)
                            {
                                ageGroup.Sum += 1;

                                if(u.Gender.ToLower().Trim() == "female")
                                {
                                    ageGroup.Female += 1;
                                }
                                else
                                {
                                    ageGroup.Male += 1;
                                }
                            }
                        }
                    }

                    returnData.Add(resp);

                }

                return returnData;
            }
        }

  

        public async Task<int> GetRegisteredUsersFilteredByProfessionGenderAndAgeGroupWithinTimeRange(int institutionId, DateTime start, DateTime end, int? professionId, string gender, int? ageGroupStart, int? ageGroupEnd)
        {
            using (var ctx = new DataContext())
            {
                var users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate.Value && end>= t.AccountCreationDate.Value).ToListAsync();
                if (!string.IsNullOrEmpty(gender))
                {
                    users = users.Where(t => t.Gender == gender).ToList();
                }

                if (professionId.HasValue)
                {
                    users = users.Where(t => t.ProfessionId.HasValue && t.ProfessionId == professionId.Value).ToList();
                }

                if (ageGroupStart.HasValue && ageGroupEnd.HasValue)
                {
                    var year = DateTime.Now.Year;

                    users = users.Where(t => t.YearOfBirth.HasValue && ageGroupStart.Value <= (year - t.YearOfBirth.Value) && ageGroupEnd.Value >= (year - t.YearOfBirth.Value)).ToList();
                }

                return users.Count;
            }
        }

        public async Task<LibraryConnectionsSummarizedResponse> GetTotalConnectionsFromBeggining(int instituteId)
        {
            var resp = new LibraryConnectionsSummarizedResponse();
            var allConnections = await _mongoCtx.InstitureUsersCollection.FindByInstituteId(instituteId);

            resp.CurrentlyConnected = allConnections.Where(t => !t.ConnectionEndTime.HasValue).Count();

            resp.TotalConnections = allConnections.Count;       

            resp.ConnectionsTotalTime = Math.Round(allConnections.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);

            if (resp.TotalConnections > 0)
            {
                resp.ConnectrionsAverageTime = Math.Round(allConnections.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
            }

            resp.ByProfession = await GetConnectionsByStudentProfessions(allConnections, instituteId);
            resp.ByYearOfBirth = await GetConnectionsByStudentBirthDate(allConnections, instituteId);
            resp.ByGender = await GetConnectionsByStudentGender(allConnections, instituteId);

            using(var ctx = new DataContext())
            {
                resp.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == instituteId).CountAsync();
            }

            return resp;
        }

        public async Task<LibraryConnectionsSummarizedResponse> GetTotalConnectionsWithinTimeRange(int instituteId, DateTime start, DateTime end)
        {
            var resp = new LibraryConnectionsSummarizedResponse();
            var allConnections = await _mongoCtx.InstitureUsersCollection.FindByInstituteId(instituteId);

            resp.CurrentlyConnected = allConnections.Where(t => !t.ConnectionEndTime.HasValue).Count();

           allConnections = allConnections.Where(t => t.ConnectionStartTime >= start && t.ConnectionStartTime <= end).ToList();

           

            resp.TotalConnections = allConnections.Count;

            resp.ConnectionsTotalTime = Math.Round(allConnections.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);

            if (resp.TotalConnections > 0)
            {
                resp.ConnectrionsAverageTime = Math.Round(allConnections.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
            }

            resp.ByProfession = await GetConnectionsByStudentProfessions(allConnections, instituteId, start, end);
            resp.ByYearOfBirth = await GetConnectionsByStudentBirthDate(allConnections, instituteId, start, end);
            resp.ByGender = await GetConnectionsByStudentGender(allConnections, instituteId, start, end);

            using (var ctx = new DataContext())
            {
                resp.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == instituteId
                && t.AccountCreationDate.HasValue && start<= t.AccountCreationDate.Value && end >= t.AccountCreationDate.Value).CountAsync();
            }
            return resp;
        }

        private async Task<List<ConnectionsByProfession>> GetConnectionsByStudentProfessions(List<InstituteUserConnection> connections, int institutionId, DateTime? start=null, DateTime? end = null)
        {
            var data = new List<ConnectionsByProfession>();

            using(var ctx = new DataContext())
            {
                var professions = await ctx.Professions.ToListAsync();

                foreach(var p in professions)
                {
                    var ent = new ConnectionsByProfession();
                    ent.ProfessionId = p.Id;
                    ent.ProfessionName = p.Name;
                    if (start.HasValue && end.HasValue)
                    {
                        ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.ProfessionId.HasValue && t.ProfessionId == p.Id
                        && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                    }
                    else
                    {
                        ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.ProfessionId.HasValue && t.ProfessionId == p.Id).CountAsync();
                    }
                    data.Add(ent);
                }
                var groupped = connections.GroupBy(u => u.UserId).ToList();
               
                foreach(var g in groupped)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == g.Key);
                    if(user!= null)
                    {
                        if(user.ProfessionId.HasValue)
                        {
                            var foundEntity = data.FirstOrDefault(x => x.ProfessionId == user.ProfessionId);
                            if(foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                            }
                        }
                    }
                }
            }

            return data;
        }

        private async Task<List<ConnectionsByBirthDate>> GetConnectionsByStudentBirthDate(List<InstituteUserConnection> connections, int institutionId, DateTime? start = null, DateTime? end = null)
        {
            var data = new List<ConnectionsByBirthDate>();
            using (var ctx = new DataContext())
            {
                if (start.HasValue && end.HasValue)
                {
                    var users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId == institutionId && t.YearOfBirth.HasValue && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).ToListAsync();
                    data.Add(new ConnectionsByBirthDate() { FromAge = 0, ToAge = 17, usersByBirthdate = users , Id=1, Label = $"0-17"});
                    data.Add(new ConnectionsByBirthDate() { FromAge = 18, ToAge = 29, usersByBirthdate = users, Id = 2, Label =$"18-29" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 30, ToAge = 49, usersByBirthdate = users, Id = 3, Label = $"30-49" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 50, ToAge = 69, usersByBirthdate = users , Id = 4, Label = $"50-69" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 70, ToAge = 100, usersByBirthdate = users, Id = 5, Label = $"70-100" });
                }
                else
                {
                    var users = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId == institutionId && t.YearOfBirth.HasValue).ToListAsync();
                    data.Add(new ConnectionsByBirthDate() { FromAge = 0, ToAge = 17, usersByBirthdate = users, Id = 1, Label = $"0-17" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 18, ToAge = 29, usersByBirthdate = users, Id = 2, Label = $"18-29" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 30, ToAge = 49, usersByBirthdate = users, Id = 3, Label = $"30-49" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 50, ToAge = 69, usersByBirthdate = users, Id = 4, Label = $"50-69" });
                    data.Add(new ConnectionsByBirthDate() { FromAge = 70, ToAge = 100, usersByBirthdate = users, Id = 5, Label = $"70-100" });
                }
    
                var groupped = connections.GroupBy(u => u.UserId).ToList();

                foreach (var g in groupped)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == g.Key);
                   
                    if (user != null)
                    {
                        if (user.YearOfBirth.HasValue)
                        {
                            var age = DateTime.Today.Year - user.YearOfBirth.Value;
                            var foundEntity = data.FirstOrDefault(x => x.FromAge<= age && x.ToAge>= age);
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                            }
                        }
                    }
                }
            }

            return data;
        }

        private async Task<List<ConnectionsByGender>> GetConnectionsByStudentGender(List<InstituteUserConnection> connections, int institutionId, DateTime? start = null, DateTime? end = null)
        {
            var data = new List<ConnectionsByGender>();

            using (var ctx = new DataContext())
            {
                var genders = new List<string> { "Female", "Male", "Unknown" };
                int gId = 1;
                foreach (var g in genders)
                {
                    var ent = new ConnectionsByGender();
                    ent.Gender = g;
                    ent.Id = gId;

                    if (g!="Unknown")
                    {        
                        if(start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Gender == g
                            && t.AccountCreationDate.HasValue && start<= t.AccountCreationDate && end>= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Gender == g).CountAsync();
                        }

                        data.Add(ent);
                    }
                    else
                    {
                        if (start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Gender == null
                            && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Gender == null).CountAsync();
                        }

                        data.Add(ent);
                    }

                    gId++;
                }

                var groupped = connections.GroupBy(u => u.UserId).ToList();

                foreach (var g in groupped)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == g.Key);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.Gender))
                        {
                            var foundEntity = data.FirstOrDefault(x => x.Gender == user.Gender);
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes),2);
                            }
                        }
                        else
                        {
                            var foundEntity = data.FirstOrDefault(x => x.Gender == "Unknown");
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                            }
                        }
                    }
                }
            }

            return data;
        }

        private async Task<List<ConnectionsByCity>> GetConnectionsByStudentCity(List<InstituteUserConnection> connections, int institutionId, DateTime? start = null, DateTime? end = null)
        {
            var data = new List<ConnectionsByCity>();

            using (var ctx = new DataContext())
            {
                var cities = await ctx.users.Where(x => x.UserRole == UserRoleType.INSTITUTION_STUDENT && x.City != null).Select(c => c.City).Distinct().OrderBy(x => x).ToListAsync();

                cities.Add("Unknown");
                int gId = 1;
                foreach (var c in cities)
                {
                    var ent = new ConnectionsByCity();
                    ent.City = c;
                    ent.Id = gId;

                    if (c != "Unknown")
                    {
                        if (start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.City == c
                            && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.City == c).CountAsync();
                        }

                        data.Add(ent);
                    }
                    else
                    {
                        if (start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.City == null
                            && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.City == null).CountAsync();
                        }

                        data.Add(ent);
                    }

                    gId++;
                }

                var groupped = connections.GroupBy(u => u.UserId).ToList();

                foreach (var g in groupped)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == g.Key);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.City))
                        {
                            var foundEntity = data.FirstOrDefault(x => x.City == user.City);
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                            }
                        }
                        else
                        {
                            var foundEntity = data.FirstOrDefault(x => x.City == "Unknown");
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                            }
                        }
                    }
                }
            }

            return data;
        }

        private async Task<List<ConnectionsByRegion>> GetConnectionsByStudentRegion(List<InstituteUserConnection> connections, int institutionId, DateTime? start = null, DateTime? end = null)
        {
            var data = new List<ConnectionsByRegion>();

            using (var ctx = new DataContext())
            {
                var regions = await ctx.users.Where(x =>x.UserRole == UserRoleType.INSTITUTION_STUDENT && x.Region != null).Select(c => c.Region).Distinct().OrderBy(x => x).ToListAsync();

                regions.Add("Unknown");
                int gId = 1;
                foreach (var c in regions)
                {
                    var ent = new ConnectionsByRegion();
                    ent.Region = c;
                    ent.Id = gId;

                    if (c != "Unknown")
                    {
                        if (start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Region == c
                            && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Region == c).CountAsync();
                        }

                        data.Add(ent);
                    }
                    else
                    {
                        if (start.HasValue && end.HasValue)
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Region == null
                            && t.AccountCreationDate.HasValue && start <= t.AccountCreationDate && end >= t.AccountCreationDate).CountAsync();
                        }
                        else
                        {
                            ent.TotalRegisteredUsers = await ctx.users.Where(t => t.InstitutionId.HasValue && t.InstitutionId.Value == institutionId && t.Region == null).CountAsync();
                        }

                        data.Add(ent);
                    }

                    gId++;
                }

                var groupped = connections.GroupBy(u => u.UserId).ToList();

                foreach (var g in groupped)
                {
                    var user = await ctx.users.FirstOrDefaultAsync(x => x.Id == g.Key);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.Region))
                        {
                            var foundEntity = data.FirstOrDefault(x => x.Region == user.Region);
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                            }
                        }
                        else
                        {
                            var foundEntity = data.FirstOrDefault(x => x.Region == "Unknown");
                            if (foundEntity != null)
                            {
                                foundEntity.TotalConnections += g.Count();
                                if (g.Count() > 0)
                                {
                                    foundEntity.ConnectrionsAverageTime += Math.Round(g.Average(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                                }
                                foundEntity.ConnectionsTotalTime += Math.Round(g.Sum(t => ((t.ConnectionEndTime.HasValue ? t.ConnectionEndTime.Value : DateTime.Now) - t.ConnectionStartTime).TotalMinutes), 2);
                            }
                        }
                    }
                }
            }

            return data;
        }
        public enum B2BAccountType : byte
        {
            B2B_ACCOUNT,
            LIBRARY_ACCOUNT
        }
    }
}

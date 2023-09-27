using CoachOnline.Model.ApiResponses.B2B;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface ILibraryManagement
    {
        Task<string> Login(string email, string password);
        Task UpdateLibraryAccountPassword(int libraryId, string secret, string repeat, string oldPassword);
        Task<LibraryBasicInfoRespWithAccountType> GetLibraryInfo(int libraryId);
        Task<int> GetLibraryAccountIdByToken(string token);
        Task<LibraryConnectionsSummarizedResponse> GetTotalConnectionsFromBeggining(int instituteId);
        Task<LibraryConnectionsSummarizedResponse> GetTotalConnectionsWithinTimeRange(int instituteId, DateTime start, DateTime end);
        Task<int> GetConnectionsLimitForLibrary(int libraryId);
        Task<int> GetCurrentConnections(int libraryId);
        Task<int> IsUserCurrentlyConnectedAndAllowed(int libraryId, int userId);
        Task<int> GetRegisteredUsersFilteredByProfessionGenderAndAgeGroup(int institutionId, int? professionId, string gender, int? ageGroupStart, int? ageGroupEnd);
        Task<int> GetRegisteredUsersFilteredByProfessionGenderAndAgeGroupWithinTimeRange(int institutionId, DateTime start, DateTime end, int? professionId, string gender, int? ageGroupStart, int? ageGroupEnd);
        Task<List<LibraryChartDataResponse>> GetRegisteredUsersForChart(int institutionId, DateTime? startDate, DateTime? endDate);
        Task<LibraryConnectionsByKeyHeaderResponse> GetLibraryStatsByKey(int libraryId, string key, DateTime? startDate, DateTime? endDate);
    }
}

using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IUserInfo
    {

        Task<CoachInfoResponse> GetCoachData(int coachId, bool forCourse = false, bool forAdmin = false);
        Task<CoachInfoDocumentResponse> GetCoachDocumentData(int coachId);
        Task<CourseResponse> GetCourse(int courseId, int userId);
        Task<CourseInfoResponse> GetCourseInfoData(int courseId);
        Task<List<EpisodeResponse>> GetCourseEpisodes(int courseId);
        Task<CategoryAPI> GetCourseCategory(int categoryId);
    }
}

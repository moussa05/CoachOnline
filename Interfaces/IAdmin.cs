using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.ApiObject;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IAdmin
    {
        Task AcceptCourse(string AdminAuthToken, int CourseId);
        Task AssignChildToCategory(AssignDismissChildRequest request);
        Task ChangeCourseState(string AdminAuthToken, int CourseId, CourseState state);
        Task ChangeUserState(string AdminAuthToken, int UserId, UserAccountStatus userAccountStatus);
        Task<int> CreateCategory(string AdminAuthToken, string CategoryName);
        Task DismissChildFromCategory(AssignDismissChildRequest request);
        Task<string> GetAdminAuthToken(string email, string password);
        Task<GetAdminCategoriesResponse> GetCategories(GetAdminCategoriesRequest request);
        Task<GetCoursesAsAdminResponse> GetCoursesWithUsers(GetCoursesAsAdminRequest request);
        Task<GetUsersResponse> GetUsers(string authToken, int count, int lastId, bool fromOldest, UserRoleType? byRole = null);
        Task<UserAPI> GetUserData(int userId);
        Task RejectCourse(string AdminAuthToken, int CourseId, string Reason);
        Task RemoveAttachmentFromCourse(string AdminAuthToken, int CourseId, int EpisodeId, int AttachmentId);
        Task RemoveCategory(string AdminAuthToken, int Categoryid);
        Task RemoveCourse(string AdminAuthToken, int CourseId);
        Task RemoveEpisodeFromCourse(string AdminAuthToken, int CourseId, int LessonId);
        Task RemoveMediaFromEpisode(string AdminAuthToken, int CourseId, int EpisodeId);
        Task UpdateCategoryFamily(UpdateCategoryFamilyRequest request);
        Task UpdateCategoryName(string AdminAuthToken, int CategoryId, string CategoryNameNew);
        Task<string> UpdateCoachPhoto(UpdateCoachPhotoAsAdminRequest request);
        Task<string> UpdateCoachCV(UpdateCoachCVAsAdminRequest request);
        Task<List<string>> UpdateCoachReturns(UpdateCoachReturnsAsAdminRequest request);
        Task<List<string>> UpdateCoachAttestations(UpdateCoachAttestationAsAdminRequest request);
        // Task UpdateCourseDetailsAsync(string AuthToken, int CourseId, string Name, int Category, string Description, string PhotoUrl);
        Task UpdateCourseDetailsAsync(UpdateCourseAdminRequest request);
        Task UpdateEpisodeAttachment(string AuthToken, int CourseId, int EpisodeId, string AttachmentHashId);
        Task UpdateEpisodeInCourse(string AuthToken, int CourseId, int EpisodeId, string Title, string Description, int OrdinalNumber);
        Task UpdateEpisodeMedia(string AuthToken, int CourseId, int EpisodeId);
        Task UpdateUserBillingInfo(UpdateUserBillingInfoAsAdminRequest request);
        Task UpdateUserProfile(UpdateUserProfileAsAdminRequest request);
        Task<UserSubscriptionAPIReponse> GetUserSubscriptionData(int userId);
        Task<List<CourseResponse>> GetCoursesToFlag();
        Task FlagCourses(List<CourseFlagRqs> coursesToFlag);
        Task<List<SuggestedCategoryResponse>> GetCategoriesSuggestedByUsers();
        Task RejectSuggestedCategory(int suggestedCatId, string rejectReason);
        Task AcceptSuggestedCategory(int suggestedCatId);
        Task<CourseResponse> GetCourse(int courseId);
        Task<UploadCoursePhotoResponse> UploadCoursePhoto(string PhotoBase, int CourseId);
        Task DeleteAttachment(int attachmentId);
        Task AddAttachment(int episodeId, string attachmentName, string extension, string attachmentBase64);
        Task UpdateUserPassword(int userId, string pass, string repeat);
        Task<List<ExtractUserDataResponse>> ExtractUserData(UserRoleType? role = null, DateTime? start = null, DateTime? end = null);
        Task<List<ExtractAffiliateHostDataResponse>> GetAffiliatesData(DateTime? start = null, DateTime? end= null);
        Task<UploadCoursePhotoResponse> UploadCourseBannerPhoto(string PhotoBase, int CourseId);
    }
}

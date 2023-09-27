using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface ICoachService
    {
        Task<List<EpisodeAttachment>> AddAttachmentToEpisode(string AuthToken, string AttachmentBase64, int CourseId, int EpisodeId, string Extension, string AttachmentName);
        Task<int> AddEpisodeToCourse(string AuthToken, int CourseId, string Title, string Description);
        Task<int> AddPromoEpisodeToCourse(string AuthToken, int CourseId, string Title, string Description);
        Task AssignCategoryToUser(CategoryToUserRequest request);
        Task AssignUserCategory(int userId, int categoryId);
        Task<int> CreateCategoryAsync(string name, string AuthToken);
        Task<int> CreateCourseAsync(string AuthToken, string Name, int Category, string Description, string PhotoUrl);
        Task DetachCategoryFromUser(CategoryToUserRequest request);
        Task DetachUserCategory(int userId, int categoryId);
        Task<GetCategoriesResponse> GetCategories();
        Task<GetCategoriesResponse> GetCategoriesCompleted();
        Task<List<CourseResponse>> GetCoursesForOwnerAsync(string AuthToken);
        Task<UserBasicDataResponse> GetUserBasicData(string AuthToken);
        Task RemoveAttachmentFromEpisode(string AuthToken, int CourseId, int EpisodeId, int AttachmentId);
        Task RemoveAvatar(string authToken);
        Task RemoveCourseAsync(string authToken, int CourseId);
        Task RemoveEpisodeFromCourse(string AuthToken, int CourseId, int EpisodeId);
        Task RemoveMediaFromEpisode(string AuthToken, int CourseId, int EpisodeId);
        Task SubmitCourse(string AuthToken, int CourseId);
        Task UpdateCompanyData(string AuthToken, string Name, string City, string SiretNumber, string BankAccountNumber, string RegisterAddress, string Country, string VatNumber, string ZipCode, string BICNumber);
        Task UpdateCourseDetailsAsync(string AuthToken, int CourseId, string Name, int Category, string Description, string PhotoUrl);
        Task UpdateEpisodeAttachment(string AuthToken, int CourseId, int EpisodeId, string AttachmentHashId, bool needsConverting = false);
        Task UpdateEpisodeInCourse(string AuthToken, int CourseId, int EpisodeId, string Title, string Description, int OrdinalNumber);
        Task UpdateEpisodeMedia(string AuthToken, int CourseId, int EpisodeId);
        Task<string> UpdateProfileAvatar(string AuthToken, string PhotoBase);
        public Task<string> ResetPasswordAsync(string login);
        Task UpdateUserData(string AuthToken, string Name, string Surname, int? YearOfBirth, string City, string Gender, string Bio, int UserCategory, string phoneNo, string country, string PostalCode, string address);
        Task<string> UploadPhoto(string authToken, string PhotoBase);
        Task<UploadCoursePhotoResponse> UploadCoursePhoto(string AuthToken, string PhotoBase, int CourseId);
        Task BlockCourse(int userId, int courseId, string userRole);
        Task UnBlockCourse(int userId, int courseId, string userRole);
    }
}

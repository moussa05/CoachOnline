using CoachOnline.ElasticSearch.Models;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IPlayerMedia
    {
        Task DisposeUserTokenForEpisodeMedia(int userId, int episodeId);
        Task DisposeAllExpiredTokens();
        Task<string> GetUserTokenForEpisodeMedia(int userId, int episodeId, string token);
        Task<bool> UserIsAttachmentOwner(string attachment_hash, int user_id);
        Task<CourseResponse> OpenCourse(int? userId, int courseId, bool activeSubscription);
        Task<EpisodeResponse> OpenEpisode(int? userId, int episodeId, bool activeSubscription);
        Task<string> GetAttachmentPermission(int userId, int episodeId);
        Task<ICollection<CourseResponseWithWatchedStatus>> LastOpenedCourses(int userId);
        Task<ICollection<EpisodeResponse>> LastOpenedEpisodesInCourse(int userId, int courseId);
        Task<List<CoachCategories>> GetCoachCategories(int coachId);
        Task<List<EpisodeAttachment>> GetEpisodeAttachments(int episodeId);
        Task<List<CourseResponse>> GetFlaggedCourses();
        Task<List<CourseResponse>> GetMostTrendingCourses();
        Task<List<CourseResponse>> GetSuggestedCourses();
        Task<bool> IsEpisodeAPromo(int episodeId);
        Task<ICollection<CourseResponse>> LastAddedCourses(int? userId);
        Task<int> EvalCourse(int courseId, int userId, bool isLiked);
    }
}

using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Student;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IUser
    {
        Task<User> GetUserByTokenAsync(string token);
        Task<User> GetUserByTokenAllowNullAsync(string token);
        Task<User> GetUserById(int userId);
        Task<User> GetUserByIdAllowNull(int userId);
        Task<AccountLink> UpdateUserStripeConnectedAccountLink(int userId);
        Task<User> Authenticate(string email, string secret);
        Task<User> Authenticate(string token);
        Task UpdateBasicUserData(User u, string Name, string Surname, int? YearOfBirth, string City, string Bio, string Country, string PostalCode, string Address, string gender, string phoneNo, string nick);
        Task<EndUserProfileDataResponse> GetUserProfileData(User u);
        Task<User> GetAdminByTokenAsync(string token);
        Task<bool> IsUserOwnerOfCourse(int userId, int courseId);
        Task<bool> IsUserOwnerOfEpisode(int userId, int episodeId);
        Task DeleteAccount(int userId);
        Task<User> GetUserByTokenAsync(string token, int CourseId, int EpisodeId);
        Task UpdateUserEmail(int userId, string email);
        Task<UserRoleType> ConfirmEmailUpdate(string confirmToken);
        Task<GetAuthTokenResponse> SocialLogin(string socialId, string provider, string deviceInfo, string placeInfo, string ipAddress);
        Task<GetAuthTokenResponse> RegisterSocialLogin(string socialId, string provider, string email, string firstName, string lastName, string pictrueUrl, UserRoleType role, string deviceInfo, string placeInfo, string ipAddress, string gender, int? yearOfBirth, int? professionId, int? libraryId, string affiliateLink);
        Task GenerateNick(int userId);

        Task SendEndOfDiscoveryModeEmails();


    }
}

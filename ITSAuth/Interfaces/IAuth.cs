using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITSAuth.Interfaces
{
    //interface IAuth
    //{
    //    public void CreateAccount(string id, string secret, string secretRepeated);
    //    public string GetAuthToken(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "");
    //    public void DisposeLogin(string token);
    //    public void ChangePassword(string login, string password, string oldPassword);
    //    public string ResetPassword(string login);
    //    public void ResetPasswordConfirmation(string login, string password, string passwordRepeated, string resetToken);

    //    //Task SubmitCourse(string AuthToken, int CourseId);
    //}


    public interface IAuthAsync
    {
        public Task CreateCoachAccountAsync(string id, string secret, string secretRepeated, string firstName, string lastName, string phoneNo, string affiliateLink = null);
        public Task CreateStudentAccountAsync(string id, string secret, string secretRepeated, string firstName, string lastName, string phoneNo, string affiliateLink = null);
        public Task<string> GetAuthTokenAsync(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "");
        public Task DisposeLoginAsync(string token);
        public Task ChangePasswordAsync(string Authtoken, string password, string passwordRepeat, string oldPassword);
        public Task<string> ResetPasswordAsync(string login);
        public Task ResetPasswordConfirmationAsync(string login, string password, string passwordRepeated, string resetToken);
        public Task<string> ResendEmailConfirmation(string emailAddress);
        public Task<GetAuthTokenResponse> GetAuthTokenWithUserDataAsync(string id, string secret, string deviceInfo = "", string IpAddress = "", string PlaceInfo = "");
        public Task ConfirmEmailRegistrationAsync(string emailToken);
        public Task UpdateUserData(string AuthToken, string Name, string Surname, int? YearOfBirth, string City, string Gender, string Bio, int UserCategory, string phoneNo, string country, string PostalCode, string address);
        public Task RemoveAvatar(string authToken);
        public Task<string> UploadPhoto(string authToken, string PhotoBase);
        public Task<string> UpdateProfileAvatar(string AuthToken, string PhotoBase);
        public Task UpdateCompanyData(string AuthToken, string Name, string City, string SiretNumber, string BankAccountNumber, string RegisterAddress, string Country, string VatNumber, string ZipCode, string BICNumber);
        public Task<UserBasicDataResponse> GetUserBasicData(string AuthToken);
        public Task<GetCategoriesResponse> GetCategories();
        public Task<string> CreateEmailConfirmationToken(string email);
        Task<GetCategoriesResponse> GetCategoriesForUsers();
        Task<dynamic> GetInfoAboutAffiliateHost(string affLink);
    }
}

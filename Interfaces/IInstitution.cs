using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.B2B;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IInstitution
    {
        Task<List<Profession>> GetProfessions();
        Task RegisterWithInstitution(int institutionId, int professionId, string email, string password, string repeat, string gender, int yearOfBirth, string firstName, string lastName, string phoneNo, string city, string country, string region);
        Task<LibraryBasicInfoResp> GetInstitutionInfo(string instituteLink);

    }
}

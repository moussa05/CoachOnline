using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.B2B
{

    public class LibraryConnectionsByKeyHeaderResponse
    {
        public int CurrentlyConnected { get; set; }
        public int TotalRegisteredUsers { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
        public string Key { get; set; }
        public List<LibraryConnectionsByKeyResponse> Data { get; set; }
    }

    public class LibraryConnectionsByKeyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CurrentlyConnected { get; set; }
        public int TotalRegisteredUsers { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
    }

    public class LibraryConnectionsSummarizedResponse
    {
        public int TotalRegisteredUsers { get; set; }

        public int CurrentlyConnected { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
        public List<ConnectionsByProfession> ByProfession { get; set;}
        public List<ConnectionsByBirthDate> ByYearOfBirth { get; set; }
        public List<ConnectionsByGender> ByGender { get; set; }
    }

    public class ConnectionsByProfession
    {
        public int ProfessionId { get; set; }
        public string ProfessionName { get; set; }
        public int TotalRegisteredUsers { get; set; }

        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
    }

    public class ConnectionsByBirthDate
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int FromAge { get; set; }
        public int ToAge { get; set; }
        public int TotalRegisteredUsers
        {
            get
            {
                var year = DateTime.Today.Year;
                var count = usersByBirthdate.Count(t => FromAge <= (year - t.YearOfBirth.Value) && ToAge >= (year - t.YearOfBirth.Value));
                return count;
            }
        }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<User> usersByBirthdate { get; set; } = new List<User>();
    }

    public class ConnectionsByGender
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public int TotalRegisteredUsers { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
    }

    public class ConnectionsByCity
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int TotalRegisteredUsers { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
    }

    public class ConnectionsByRegion
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public int TotalRegisteredUsers { get; set; }
        public int TotalConnections { get; set; }
        public double ConnectionsTotalTime { get; set; }
        public double ConnectrionsAverageTime { get; set; }
    }
}

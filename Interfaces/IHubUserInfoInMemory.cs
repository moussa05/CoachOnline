using CoachOnline.Hubs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IHubUserInfoInMemory
    {
        bool AddUpdate(int userId, string connectionId, int episodeId);
        void Remove(int userId, int episodeId);
        IEnumerable<HubUserInfo> GetAllUsersExceptThis(int userId, int episodeId);
        IEnumerable<HubUserInfo> GetAllByUserId(int userId);
        IEnumerable<HubUserInfo> GetAllUsers();
        HubUserInfo GetUserInfo(int userId, int episodeId);
    }
}

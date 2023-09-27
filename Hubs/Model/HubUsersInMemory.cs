using CoachOnline.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Hubs.Model
{
    public class HubUsersInMemory:IHubUserInfoInMemory
    {
        /// <summary>
        /// Tuple<int,int> userId, episodeId
        /// </summary>
        private ConcurrentDictionary<Tuple<int,int>, HubUserInfo> _onlineUser { get; set; } = new ConcurrentDictionary<Tuple<int, int>, HubUserInfo>();

        public bool AddUpdate(int userId, string connectionId, int episodeId)
        {
            var userTuple = new Tuple<int, int>(userId, episodeId);
            var userAlreadyExists = _onlineUser.ContainsKey(userTuple);

            var userInfo = new HubUserInfo
            {
                UserId = userId,
                ConnectionId = connectionId,
                EpisodeId = episodeId
            };

            _onlineUser.AddOrUpdate(userTuple, userInfo, (key, value) => userInfo);

            return userAlreadyExists;
        }

        public void Remove(int userId, int episodeId)
        {
            var userTuple = new Tuple<int, int>(userId, episodeId);
            HubUserInfo userInfo;
            _onlineUser.TryRemove(userTuple, out userInfo);
        }

        public IEnumerable<HubUserInfo> GetAllUsersExceptThis(int userId, int episodeId)
        {
            return _onlineUser.Values.Where(item => item.UserId != userId && item.EpisodeId != episodeId);
        }

        public IEnumerable<HubUserInfo> GetAllByUserId(int userId)
        {
            return _onlineUser.Values.Where(item => item.UserId == userId);
        }


        public IEnumerable<HubUserInfo> GetAllUsers()
        {
            return _onlineUser.Values;
        }

        public HubUserInfo GetUserInfo(int userId, int episodeId)
        {
            var userTuple = new Tuple<int, int>(userId, episodeId);
            HubUserInfo user;
            _onlineUser.TryGetValue(userTuple, out user);
            return user;
        }
    }
}


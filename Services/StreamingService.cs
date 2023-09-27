using AgoraIO.Media;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Statics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public interface IStream
    {
        string CreateStream(CreateStreamRqs request);
    }

    public class StreamingService: IStream
    {
        private readonly ILogger<StreamingService> _logger;
        public StreamingService(ILogger<StreamingService> logger)
        {
            _logger = logger;
        }

        public string CreateStream(CreateStreamRqs request)
        {

            string streamingToken = "";
            if (request.ValidUntil == 0)
            {
                request.ValidUntil = (uint)ConvertTime.ToUnixTimestamp(DateTime.Now.AddHours(24));
            }
            RtcTokenBuilder token = new RtcTokenBuilder();

            if (request.IsHost)
            {
                streamingToken = token.buildTokenWithUserAccount("0e407a1a36cf4012aa59801100f75ce2", "8eaa9b36c70f4ff2bbf6814159f4e38b", request.ChannelName, request.UserId, RtcTokenBuilder.Role.Role_Publisher, request.ValidUntil);

            }
            else
            {
                
                streamingToken = token.buildTokenWithUserAccount("0e407a1a36cf4012aa59801100f75ce2", "8eaa9b36c70f4ff2bbf6814159f4e38b", request.ChannelName, request.UserId, RtcTokenBuilder.Role.Role_Attendee, request.ValidUntil);

            }


            return streamingToken;
        }

    }
}

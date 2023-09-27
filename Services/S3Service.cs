using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class S3Service
    {
        private readonly ILogger<S3Service> _logger;
        private readonly IAmazonS3 _s3Client;

        public S3Service(ILogger<S3Service> logger, IAmazonS3 s3Client)
        {
            _logger = logger;
            _s3Client = s3Client;
          
        }

        private async Task UploadVideoFile(string filename)
        {

        }

        private async Task<ListBucketsResponse> ListBucketsAsync()
        {
            var response = await _s3Client.ListBucketsAsync();
            return response;
        }
    }
}

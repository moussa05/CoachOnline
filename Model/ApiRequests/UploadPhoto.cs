using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class UploadPhotoRequest
    {
        public string AuthToken { get; set; }
        public string Base64Photo { get; set; }
    }

    public class UploadCoursePhotoRequest
    {
        public string AuthToken { get; set; }
        public int CourseId { get; set; }
        public string PhotoBase64 { get; set; }
    }

    public class UploadCoursePhotoRequestAdmin
    {
        public int CourseId { get; set; }
        public string PhotoBase64 { get; set; }
    }

    public class UploadPhotoResponse
    {
        public string PhotoPath { get; set; }
    }

    public class UploadCoursePhotoResponse
    {
        public int CourseId { get; set; }
        public string PhotoPath { get; set; }
    }

    public class RemovePhotoRequest
    {
        public string AuthToken { get; set; }
        public int CourseId { get; set; }
        public int PhotoId { get; set; }
    }
}

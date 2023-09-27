using CoachOnline.ElasticSearch.Models;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CourseResponse
    {
      
        public int Id { get; set; }
        public string Name { get; set; }
        public CourseState State { get; set; }
        public CategoryAPI Category { get; set; }
        public string Description { get; set; }
        public List<EpisodeResponse> Episodes { get; set; }
        public string PhotoUrl { get; set; }
        public long Created { get; set; }
        public bool? IsFlagged { get; set; }
        public int? OrderNo { get; set; }
        public string BannerPhotoUrl { get; set; }
        public int LikesCnt { get; set; }
        public bool IsLikedByMe { get; set; }
        public CoachInfoResponse Coach { get; set; }

        public string Prerequisite { get; set; }
        public string Objectives { get; set; }
        public string PublicTargets { get; set; }
        public string CertificationQCM { get; set; }

        public virtual List<RejectionResponse> RejectionsHistory { get; set; }
    }

    public class CourseResponseWithWatchedStatus: CourseResponse
    {

        public decimal WatchedPercentage { get; set; }
        public decimal WatchedEpisodesCnt { get; set; }
        public decimal AllEpisodesCnt { get; set; }

        public virtual List<RejectionResponse> RejectionsHistory { get; set; }
    }

    public class RejectionResponse
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public long Date { get; set; }
    }

    public class CoachInfoResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public int? YearOfBirth { get; set; }
        public string PhotoUrl { get; set; }
        public string ProfilePhotoUrl
        {
            get { return $"images/{PhotoUrl}"; }
        }
        public List<CourseResponse> Courses { get; set; }
        public List<CoachCategories> UserCategories { get; set; }
    }

    public class CoachInfoDocumentResponse
    {
        public string UserCV { get; set; }
        public List<string> Returns { get; set; }
        public List<string> Diplomas { get; set; }
        // public string ReturnOne { get; set; }
        // public string ReturnTwo { get; set; }
        // public string ReturnThree { get; set; }
        // public string DiplomeOne { get; set; }
        // public string DiplomeTwo { get; set; }
        // public string DiplomeThree { get; set; }
    }

    public class CourseInfoResponse
    {
        public string Cible { get; set; }
        public string Requis { get; set; }
        public string Objectif { get; set; }
        public string CertificationQCM { get; set; }
    }

    public class EpisodeResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MediaId { get; set; }
        public List<EpisodeAttachment> Attachments { get; set; }
        public long Created { get; set; }
        public int OrdinalNumber { get; set; }
        public int CourseId { get; set; }
        public string Query { get; set; }
        public decimal LastOpenedSecond { get; set; }
        public bool NeedConversion { get; internal set; }
        public double Length { get; internal set; }
        public bool IsPromo { get; set; }
        public EpisodeState EpisodeState { get; set; }
        public string EpisodeStateStr
        {
            get { return EpisodeState.ToString(); }
        }
    }
}

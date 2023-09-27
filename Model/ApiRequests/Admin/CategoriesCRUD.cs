using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class CreateCategoryAdminRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }

        [MaxLength(64, ErrorMessage = "Max lenght of category name is 64")]

        public string CategoryName { get; set; }
    }
    public class CreateCategoryAdminResponse
    {
        public int NewCategoryId { get; set; }
    }
    public class UpdateCategoryRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
        [MaxLength(64, ErrorMessage = "Max lenght of category name is 64")]

        public string CategoryNewName { get; set; }
        public int CategoryId { get; set; }
    }
    public class RemoveCategoryRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
        public int CategoryId { get; set; }
    }

    public class AssignDismissChildRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
        public int ParentCategoryId { get; set; }
        public int ChildCategoryId { get; set; }
    }


}

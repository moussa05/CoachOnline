using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class PendingCategory
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; }
        public string RejectReason { get; set; }
        public PendingCategoryState State { get; set; }
        public bool AdultOnly { get; set; }

    }

    public enum PendingCategoryState : byte { PENDING, REJECTED, APPROVED }
}

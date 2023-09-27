using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Coach
{
    public class UpdateFieldRqs
    {
        [Required]
        public string PropertyName { get; set; }

        public dynamic PropertyValue { get; set; }
    }
}

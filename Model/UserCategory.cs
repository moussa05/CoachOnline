using System.ComponentModel.DataAnnotations;

namespace CoachOnline.Model
{
    public class UserCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
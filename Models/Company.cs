using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
    public class Company
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Website { get; set; }

        [StringLength(200)]
        public string Industry { get; set; }

        [StringLength(500)]
        public string Address { get; set; }
    }
}

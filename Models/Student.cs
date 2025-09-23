using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(100)]
        public string CollegeName { get; set; }

        [Required]
        [StringLength(50)]
        public string StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        [StringLength(10)]
        public string Year { get; set; }

        [Required]
        [Range(0, 10)]
        public decimal CGPA { get; set; }

        [StringLength(500)]
        public string Skills { get; set; }

        [StringLength(1000)]
        public string ResumePath { get; set; }

		// Approval by College
		public bool IsApproved { get; set; }
    }
}

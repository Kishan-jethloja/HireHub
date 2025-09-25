using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlacementManagementSystem.Models
{
    public enum ApplicationStatus
    {
        Pending = 0,
        Hired = 1,
        Rejected = 2
    }

	public class Application
	{
		public int Id { get; set; }

		[Required]
		public int JobPostingId { get; set; }
		[ForeignKey("JobPostingId")]
		public JobPosting JobPosting { get; set; }

		[Required]
		public string StudentUserId { get; set; }
		[ForeignKey("StudentUserId")]
		public ApplicationUser StudentUser { get; set; }

		[Required]
		[StringLength(200)]
		public string ApplicantName { get; set; }

		[Required]
		[StringLength(200)]
		[EmailAddress]
		public string ApplicantEmail { get; set; }

		[Required]
		[StringLength(50)]
		[Display(Name = "College ID")]
		public string CollegeId { get; set; }

		[Required]
		[Url]
		[StringLength(300)]
		[Display(Name = "LinkedIn URL")]
		public string LinkedInUrl { get; set; }

		[Required]
		[Url]
		[StringLength(300)]
		[Display(Name = "GitHub URL")]
		public string GithubUrl { get; set; }

		[Required]
		[StringLength(20)]
		public string Gender { get; set; }

		[Required]
		[StringLength(2000)]
		[Display(Name = "Why should we hire you?")]
		public string CoverLetter { get; set; }

		[StringLength(1000)]
		public string ResumePath { get; set; }

		[Required]
		[Display(Name = "I agree to the Terms & Conditions")]
		public bool TermsAccepted { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}



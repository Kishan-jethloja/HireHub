using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PlacementManagementSystem.Models
{
	public enum JobType
	{
		Internship = 1,
		FullTime = 2,
		PartTime = 3,
		Contract = 4
	}

    public class JobPosting : IValidatableObject
	{
		public int Id { get; set; }
		
		public string CompanyUserId { get; set; }

        [Required]
        public JobType? Type { get; set; }

		[Required]
		[StringLength(200)]
		public string CollegeName { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[StringLength(5000)]
		public string Description { get; set; }

		// Removed external form usage

        [Required(ErrorMessage = "Please enter a location")]
        [StringLength(200)]
        public string Location { get; set; }

		[Range(0, int.MaxValue)]
		[Display(Name = "Stipend/CTC (numeric)")]
		public int? Compensation { get; set; }

		[Range(0, 10)]
		[Display(Name = "Minimum CPI Required")]
		public decimal? MinimumCPI { get; set; }

		// Duration field - represents months for internships, years for full-time
		[Required(ErrorMessage = "Please enter duration.")]
		[Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0.")]
		[Display(Name = "Duration")]
		public int? Duration { get; set; }

        [Required(ErrorMessage = "Please select an application deadline")]
        [Display(Name = "Application Deadline")]
        public DateTime? ApplyByUtc { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Model-level validation so errors show in the validation summary
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == JobType.Internship)
            {
                if (!Duration.HasValue || Duration.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Please enter duration in months for an internship.",
                        new[] { nameof(Duration) }
                    );
                }
            }
            else if (Type == JobType.FullTime)
            {
                if (!Duration.HasValue || Duration.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Please enter duration in years for a full-time role.",
                        new[] { nameof(Duration) }
                    );
                }
            }
        }
	}
}

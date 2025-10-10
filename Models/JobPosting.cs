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

		// Duration fields
		// For Internship, use months; for FullTime, use years
		[Required(ErrorMessage = "Please enter duration in months for an internship.")]
		[Range(1, int.MaxValue)]
		[Display(Name = "Duration (Months)")]
		public int? DurationMonths { get; set; }

		[Required(ErrorMessage = "Please enter duration in years for a full-time role.")]
		[Range(1, int.MaxValue)]
		[Display(Name = "Duration (Years)")]
		public int? DurationYears { get; set; }

        [Required(ErrorMessage = "Please select an application deadline")]
        [Display(Name = "Application Deadline")]
        public DateTime? ApplyByUtc { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Model-level validation so errors show in the validation summary
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == JobType.Internship)
            {
                if (!DurationMonths.HasValue || DurationMonths.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Please enter duration in months for an internship.",
                        new[] { nameof(DurationMonths) }
                    );
                }
            }
            else if (Type == JobType.FullTime)
            {
                if (!DurationYears.HasValue || DurationYears.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Please enter duration in years for a full-time role.",
                        new[] { nameof(DurationYears) }
                    );
                }
            }
        }
	}
}

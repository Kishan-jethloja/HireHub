using System;
using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
	public enum JobType
	{
		Internship = 1,
		FullTime = 2,
		PartTime = 3,
		Contract = 4
	}

	public class JobPosting
	{
		public int Id { get; set; }
		
		public string CompanyUserId { get; set; }

		[Required]
		public JobType Type { get; set; }

		[Required]
		[StringLength(200)]
		public string CollegeName { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[StringLength(5000)]
		public string Description { get; set; }

		// Removed external form usage

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
		[Range(1, int.MaxValue)]
		[Display(Name = "Duration (Months)")]
		public int? DurationMonths { get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name = "Duration (Years)")]
		public int? DurationYears { get; set; }

		[Display(Name = "Application Deadline")]
		public DateTime? ApplyByUtc { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}

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
		public ApplicationUser CompanyUser { get; set; }

		[Required]
		public JobType Type { get; set; }

		[Required]
		[StringLength(200)]
		public string CollegeName { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[StringLength(2000)]
		public string Description { get; set; }

		[StringLength(1000)]
		[Url]
		[Display(Name = "Google Form URL")]
		public string GoogleFormUrl { get; set; }

		[StringLength(200)]
		public string Location { get; set; }

		[Range(0, int.MaxValue)]
		[Display(Name = "Stipend/CTC (numeric)")]
		public int? Compensation { get; set; }

		[Range(0, 10)]
		[Display(Name = "Minimum CPI Required")]
		public decimal? MinimumCPI { get; set; }

		[Display(Name = "Application Deadline")]
		public DateTime? ApplyByUtc { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}

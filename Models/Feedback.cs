using System;
using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
	public enum FeedbackTargetType
	{
		Company = 1,
		College = 2
	}

	public class Feedback
	{
		public int Id { get; set; }

		// Author info
		[Required]
		public string AuthorUserId { get; set; }
		public ApplicationUser AuthorUser { get; set; }

		// Target info
		[Required]
		public FeedbackTargetType TargetType { get; set; }
		public int? TargetCompanyId { get; set; }
		public Company TargetCompany { get; set; }
		
		// For College feedback, store plain text college name
		[StringLength(200)]
		public string TargetCollegeName { get; set; }

		// Basic details
		[Required]
		[StringLength(150)]
		public string Subject { get; set; }

		[Required]
		[StringLength(2000)]
		public string Message { get; set; }

		// Star rating 1-5
		[Range(1,5)]
		public int Rating { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}

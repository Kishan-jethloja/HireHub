using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
	public class College
	{
		public int Id { get; set; }

		[Required]
		[StringLength(200)]
		public string Name { get; set; }

		[StringLength(200)]
		public string City { get; set; }

		[StringLength(200)]
		public string State { get; set; }

		[StringLength(500)]
		public string WebsiteUrl { get; set; }

		// Owner (college account user)
		[StringLength(450)]
		public string CollegeUserId { get; set; }
	}
}



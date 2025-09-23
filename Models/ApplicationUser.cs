using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PlacementManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        public UserType UserType { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserType
    {
        Student = 1,
        Company = 2,
        College = 3
    }
}

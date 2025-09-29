using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PlacementManagementSystem.Models;

namespace PlacementManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public UserType UserType { get; set; }

        // Student specific fields
        [StringLength(50)]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; }

        [StringLength(100)]
        [Display(Name = "College Name")]
        public string CollegeName { get; set; }

        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [StringLength(10)]
        [Display(Name = "Passing Out Year")]
        public string Year { get; set; }

        [Range(0, 10)]
        [Display(Name = "CPI")]
        public decimal? CGPA { get; set; }

        [Display(Name = "Resume (PDF)")]
        public IFormFile ResumeFile { get; set; }

        // Company specific fields
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(200)]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [StringLength(200)]
        [Display(Name = "Industry")]
        public string Industry { get; set; }

        // College specific fields
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(100)]
        [Display(Name = "State")]
        public string State { get; set; }
    }
}

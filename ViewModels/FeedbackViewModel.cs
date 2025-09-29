using System;

namespace PlacementManagementSystem.ViewModels
{
    public class FeedbackViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; }
        public string JobTitle { get; set; }
        public string ApplicationStatus { get; set; }
    }
}



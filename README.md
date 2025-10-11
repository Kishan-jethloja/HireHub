# HireHub - Placement Management System

A comprehensive web application built with ASP.NET Core for managing student placements, company recruitment, and college administration.

## Features

### 🔐 User Authentication & Authorization
- **Multi-role system**: Students, Companies, and Colleges
- **Secure registration** with role-specific validation
- **Login/logout** with automatic role-based redirection
- **Profile management** for all user types

### 👨‍🎓 Student Features
- **Job Discovery**: Browse and search job opportunities
- **Application Management**: Apply to jobs and track application status
- **Hire Lock**: Once hired, students can only see their hired job
- **Deadline Handling**: View past-deadline jobs with "Deadline Passed" status
- **Real-time Chat**: Communicate with colleges
- **Announcements**: Receive updates from companies
- **Profile Management**: Update personal information and academic details

### 🏢 Company Features
- **Job Posting**: Create and manage job opportunities
- **Application Review**: View and manage student applications
- **Hiring Process**: Hire, reject, or shortlist candidates
- **Feedback System**: Receive and display student feedback with ratings
- **Public Profile**: Company information visible to students
- **Announcements**: Send updates to applicants
- **Analytics**: Track application statistics

### 🏫 College Features
- **Student Management**: View and manage registered students
- **Company Directory**: Browse registered companies
- **Student Tracking**: Monitor student applications and placements
- **No Announcements**: Clean, focused interface for student management

## Prerequisites

- .NET Core 3.1 SDK or later
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2019+ or VS Code
- Entity Framework Core tools

## Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd HireHub
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   - Open `appsettings.json`
   - Update the `DefaultConnection` string to point to your SQL Server instance

4. **Create and update the database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## User Types & Workflows

### Student Workflow
1. **Register** with academic information (Student ID, Department, Year, CGPA)
2. **Browse Jobs** from various companies
3. **Apply** to relevant positions
4. **Track Status** of applications (Applied, Shortlisted, Hired, Rejected)
5. **Chat** with colleges for guidance
6. **Receive Announcements** from companies about application updates

### Company Workflow
1. **Register** with company information (Name, Description, Website, Industry)
2. **Post Jobs** with detailed requirements and deadlines
3. **Review Applications** from interested students
4. **Manage Candidates** (Shortlist, Hire, Reject)
5. **Send Announcements** to applicants
6. **Receive Feedback** from students

### College Workflow
1. **Register** with college information (Name, Website, Location)
2. **View Students** registered in the system
3. **Browse Companies** and their job postings
4. **Monitor Placements** and student progress
5. **Chat** with students for guidance

## Key Features

### 🎯 Smart Job Management
- **Dynamic Duration Fields**: Months for internships, years for full-time positions
- **Deadline Handling**: Jobs remain visible after deadline with appropriate status
- **Location & Deadline Validation**: Required fields for better job postings
- **Job Type Support**: Internships and full-time positions

### 💬 Real-time Communication
- **SignalR Chat**: Real-time messaging between students and colleges
- **Conditional Interface**: Chat only available when college is selected
- **Message History**: Persistent chat conversations

### 📊 Application Tracking
- **Status Management**: Applied → Shortlisted → Hired/Rejected
- **Hire Lock**: Hired students cannot apply to other jobs
- **Application History**: Complete tracking of all applications

### ⭐ Feedback System
- **Student Ratings**: Rate companies after interactions
- **Average Ratings**: Display company feedback scores
- **Feedback Analytics**: Track company performance

### 📢 Announcement System
- **Company Announcements**: Send updates to job applicants
- **Student Notifications**: Receive relevant updates
- **Text Formatting**: Proper handling of line breaks and formatting

## Project Structure

```
HireHub/
├── Controllers/
│   ├── AccountController.cs          # Authentication & Registration
│   ├── StudentController.cs          # Student job management
│   ├── CompanyController.cs         # Company job posting & applications
│   ├── CollegeController.cs         # College student management
│   └── ChatController.cs            # Real-time messaging
├── Models/
│   ├── ApplicationUser.cs           # User identity model
│   ├── Student.cs                   # Student profile
│   ├── Company.cs                   # Company profile
│   ├── College.cs                   # College profile
│   ├── JobPosting.cs                # Job posting model
│   ├── Application.cs               # Job application model
│   ├── Feedback.cs                  # Company feedback model
│   ├── Announcement.cs             # Announcement model
│   └── ChatMessage.cs              # Chat message model
├── ViewModels/
│   ├── LoginViewModel.cs            # Login form
│   ├── RegisterViewModel.cs         # Registration form
│   └── JobPostingViewModel.cs       # Job creation form
├── Views/
│   ├── Student/                     # Student views
│   ├── Company/                     # Company views
│   ├── College/                    # College views
│   ├── Account/                    # Authentication views
│   └── Shared/                     # Shared layouts and partials
├── Data/
│   └── ApplicationDbContext.cs     # Entity Framework context
└── wwwroot/
    ├── css/                        # Custom styles
    └── js/                         # JavaScript functionality
```

## Database Schema

### Core Tables
- **AspNetUsers**: User authentication and basic info
- **Students**: Student profiles and academic information
- **Companies**: Company profiles and information
- **Colleges**: College profiles and information
- **JobPostings**: Job opportunities and requirements
- **Applications**: Student job applications
- **Feedback**: Company ratings and reviews
- **Announcements**: Company announcements to applicants
- **ChatMessages**: Real-time chat conversations

## Technologies Used

- **Backend**: ASP.NET Core 3.1 MVC
- **Database**: Entity Framework Core 3.1 with SQL Server
- **Authentication**: ASP.NET Core Identity
- **Real-time**: SignalR for chat functionality
- **Frontend**: Bootstrap 4, jQuery, HTML5, CSS3
- **Icons**: Font Awesome 5.15.4

## Recent Updates

### ✅ Completed Features
- **Multi-role Registration**: Conditional validation based on user type
- **Job Application System**: Complete application lifecycle
- **Hire Lock Mechanism**: Prevents hired students from applying elsewhere
- **Deadline Management**: Proper handling of job deadlines
- **Feedback System**: Company rating and review system
- **Real-time Chat**: Student-college communication
- **Announcement System**: Company-to-applicant notifications
- **Profile Management**: Comprehensive user profile system
- **Navigation Optimization**: Role-based navigation and redirects

### 🔧 Technical Improvements
- **Model Validation**: Enhanced validation with custom error messages
- **Database Optimization**: Improved queries and data handling
- **UI/UX Enhancements**: Better user interface and experience
- **Error Handling**: Comprehensive error management
- **Security**: Role-based access control and data protection

## Getting Started

1. **Register as a Student**: Create account with academic details
2. **Register as a Company**: Set up company profile and post jobs
3. **Register as a College**: Join the platform to manage students
4. **Start Using**: Explore the platform based on your role

## Support

For technical support or feature requests, please contact the development team or create an issue in the repository.

---

**HireHub** - Connecting Students, Companies, and Colleges for Better Placements! 🚀
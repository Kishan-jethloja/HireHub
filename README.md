# Placement Management System

A web application built with ASP.NET Core 3.1 for managing student placements and company recruitment.

## Features

- User authentication and authorization
- Student registration and profile management
- Company registration and profile management
- Admin panel for system management
- Role-based access control

## Prerequisites

- .NET Core 3.1 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2019 or later (or VS Code)

## Setup Instructions

1. **Clone or download the project**

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

## User Types

### Student
- Register with student-specific information (Student ID, Department, Year, CGPA)
- View and apply for job opportunities
- Track application status

### Company
- Register with company information (Company Name, Description, Website, Industry)
- Post job openings
- Review student applications

### Admin
- Manage the entire system
- Oversee placement process
- User management

## Project Structure

```
PlacementManagementSystem/
├── Controllers/
│   ├── AccountController.cs
│   └── HomeController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Student.cs
│   └── Company.cs
├── ViewModels/
│   ├── LoginViewModel.cs
│   └── RegisterViewModel.cs
├── Views/
│   ├── Account/
│   ├── Home/
│   └── Shared/
└── wwwroot/
    ├── css/
    └── js/
```

## Next Steps

This is the initial setup with login and signup functionality. Future enhancements will include:

- Job posting and management
- Application tracking
- Student profile management
- Company dashboard
- Admin panel
- Email notifications
- File upload for resumes

## Technologies Used

- ASP.NET Core 3.1 MVC
- Entity Framework Core 3.1
- ASP.NET Core Identity
- SQL Server
- Bootstrap 4
- jQuery

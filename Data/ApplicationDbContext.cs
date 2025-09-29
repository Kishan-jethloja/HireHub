using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Models;

namespace PlacementManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
		public DbSet<JobPosting> JobPostings { get; set; }
		public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Student>()
                .Property(s => s.CGPA)
                .HasColumnType("decimal(3,2)");

            builder.Entity<JobPosting>()
                .Property(j => j.MinimumCPI)
                .HasColumnType("decimal(3,2)");

			// Enforce unique StudentId
			builder.Entity<Student>()
				.HasIndex(s => s.StudentId)
				.IsUnique();

            builder.Entity<Company>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Feedback>()
                .HasOne(f => f.AuthorUser)
                .WithMany()
                .HasForeignKey(f => f.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Temporarily disabled until ApplicationId column is added
            // builder.Entity<Feedback>()
            //     .HasOne(f => f.Application)
            //     .WithMany()
            //     .HasForeignKey(f => f.ApplicationId)
            //     .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Feedback>()
                .HasOne(f => f.TargetCompany)
                .WithMany()
                .HasForeignKey(f => f.TargetCompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Feedback>()
                .HasOne(f => f.JobPosting)
                .WithMany()
                .HasForeignKey(f => f.JobPostingId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<JobPosting>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(j => j.CompanyUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobPosting>()
                .Property(j => j.CompanyUserId)
                .IsRequired();

            builder.Entity<Application>()
                .HasOne(a => a.JobPosting)
                .WithMany()
                .HasForeignKey(a => a.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Application>()
                .HasOne(a => a.StudentUser)
                .WithMany()
                .HasForeignKey(a => a.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Application>()
                .HasIndex(a => new { a.JobPostingId, a.StudentUserId })
                .IsUnique();

			// Optional: you can later relate Student.CollegeName to College.Name if normalized
        }
    }
}

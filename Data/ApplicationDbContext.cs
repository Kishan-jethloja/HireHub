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
        public DbSet<Application> Applications { get; set; }

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

            builder.Entity<Company>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Feedback>()
                .HasOne(f => f.AuthorUser)
                .WithMany()
                .HasForeignKey(f => f.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
                .HasOne(f => f.TargetCompany)
                .WithMany()
                .HasForeignKey(f => f.TargetCompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<JobPosting>()
                .HasOne(j => j.CompanyUser)
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
        }
    }
}

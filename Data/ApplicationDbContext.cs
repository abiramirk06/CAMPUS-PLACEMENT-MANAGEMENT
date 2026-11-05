
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<College> Colleges { get; set; }
        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            // College configuration
            builder.Entity<College>(entity =>
            {
                entity.HasKey(c => c.CollegeId);

                entity.HasIndex(c => c.CollegeCode)
                    .IsUnique();

                entity.HasIndex(c => c.OfficialEmail)
                    .IsUnique();

                entity.Property(c => c.CollegeName)
                    .HasMaxLength(200).IsRequired();

                entity.Property(c => c.CollegeCode)
                    .HasMaxLength(50).IsRequired();

                entity.Property(c => c.UniversityName)
                    .HasMaxLength(200);

                entity.Property(c => c.OfficialEmail)
                    .HasMaxLength(255).IsRequired();

                entity.Property(c => c.PhoneNumber)
                    .HasMaxLength(20).IsRequired();

                entity.Property(c => c.Address)
                    .HasMaxLength(500).IsRequired();

                entity.Property(c => c.Website)
                    .HasMaxLength(255);

                entity.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20).IsRequired();

                entity.HasMany(c => c.PlacementOfficers)
                    .WithOne(po => po.College)
                    .HasForeignKey(po => po.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Students)
                    .WithOne(s => s.College)
                    .HasForeignKey(s => s.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Placement Officer configuration
            builder.Entity<PlacementOfficer>(entity =>
            {
                entity.HasKey(po => po.PlacementOfficerId);

                entity.HasIndex(po => po.UserId)
                    .IsUnique();

                entity.Property(po => po.Designation)
                    .HasMaxLength(100).IsRequired();

                entity.Property(po => po.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20).IsRequired();

                entity.HasOne(po => po.User)
                    .WithOne(user => user.PlacementOfficer)
                    .HasForeignKey<PlacementOfficer>(po => po.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.ApprovedByUser)
                    .WithMany(user => user.ApprovedPlacementOfficers)
                    .HasForeignKey(po => po.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Student configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentId);

                entity.HasIndex(s => s.UserId)
                    .IsUnique();

                entity.HasIndex(s => new
                {
                    s.CollegeId,
                    s.RegisterNumber
                }).IsUnique();

                entity.Property(s => s.RegisterNumber)
                    .HasMaxLength(50).IsRequired();

                entity.Property(s => s.Department)
                    .HasMaxLength(100).IsRequired();

                entity.Property(s => s.Degree)
                    .HasMaxLength(100).IsRequired();

                entity.Property(s => s.CGPA)
                    .HasPrecision(4, 2);

                entity.Property(s => s.ResumeUrl)
                    .HasMaxLength(500);

                entity.Property(s => s.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20).IsRequired();

                entity.HasOne(s => s.User)
                    .WithOne(user => user.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.College)
                    .WithMany(c => c.Students)
                    .HasForeignKey(s => s.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.PlacementOfficer)
                    .WithMany(po => po.Students)
                    .HasForeignKey(s => s.PlacementOfficerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

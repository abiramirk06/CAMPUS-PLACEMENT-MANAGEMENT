using CampusPlacementManagement.Models;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser configuration
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
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(c => c.CollegeCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(c => c.UniversityName)
                    .HasMaxLength(200);

                entity.Property(c => c.OfficialEmail)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(c => c.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(c => c.Address)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(c => c.Website)
                    .HasMaxLength(255);

                entity.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasMany(c => c.PlacementOfficers)
                    .WithOne(po => po.College)
                    .HasForeignKey(po => po.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PlacementOfficer configuration
            builder.Entity<PlacementOfficer>(entity =>
            {
                entity.HasKey(po => po.PlacementOfficerId);

                // One ApplicationUser can have only one officer profile
                entity.HasIndex(po => po.UserId)
                    .IsUnique();

                entity.Property(po => po.Designation)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(po => po.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                // Officer account relationship
                entity.HasOne(po => po.User)
                    .WithOne(user => user.PlacementOfficer)
                    .HasForeignKey<PlacementOfficer>(po => po.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // SuperAdmin who approved the officer
                entity.HasOne(po => po.ApprovedByUser)
                    .WithMany(user => user.ApprovedPlacementOfficers)
                    .HasForeignKey(po => po.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
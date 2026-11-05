using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusPlacementManagement.Enums;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Entity<College>(entity =>
            {
                entity.HasKey(c => c.CollegeId);

                entity.HasIndex(c => c.CollegeCode)
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
            });
        }
    }
}
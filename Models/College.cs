using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models
{
    public class College
    {
        public int CollegeId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CollegeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CollegeCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? UniversityName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string OfficialEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Url]
        [MaxLength(255)]
        public string? Website { get; set; }

        [Required]
        public CollegeStatus Status { get; set; } = CollegeStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // One college can have multiple placement officers
        public ICollection<PlacementOfficer> PlacementOfficers
        { get; set; } = new List<PlacementOfficer>();

        public ICollection<Student> Students { get; set; }
    = new List<Student>();
    }
}
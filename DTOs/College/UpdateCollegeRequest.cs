using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.DTOs.College
{
    public class UpdateCollegeRequest
    {
        [Required]
        [StringLength(200)]
        public string CollegeName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CollegeCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? UniversityName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string OfficialEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Url]
        [StringLength(255)]
        public string? Website { get; set; }
    }
}
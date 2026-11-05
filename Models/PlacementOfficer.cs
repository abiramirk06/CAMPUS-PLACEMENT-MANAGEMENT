using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models
{
    public class PlacementOfficer
    {
        public int PlacementOfficerId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CollegeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public PlacementOfficerStatus Status { get; set; }
            = PlacementOfficerStatus.Pending;

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }

        public College? College { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
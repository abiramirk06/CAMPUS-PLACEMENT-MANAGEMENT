using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        // One user can have one PlacementOfficer profile
        public PlacementOfficer? PlacementOfficer { get; set; }

        // One SuperAdmin can approve multiple PlacementOfficer accounts
        public ICollection<PlacementOfficer> ApprovedPlacementOfficers
        { get; set; } = new List<PlacementOfficer>();

        public Student? Student { get; set; }
    }
}
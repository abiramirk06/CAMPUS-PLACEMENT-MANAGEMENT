using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;
    }
}
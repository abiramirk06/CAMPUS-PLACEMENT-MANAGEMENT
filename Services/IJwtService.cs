using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TABS.Data.Auth
{
    public class ClaimsTransformer : IClaimsTransformation
    {

        // State-based services
        public UserPreferenceService _userPreference { get; set; }

        /// <summary>
        /// Inject state-based services for initialization
        /// </summary>
        public ClaimsTransformer(UserPreferenceService userPreferenceService)
        {
            _userPreference = userPreferenceService;
        }

        /// <summary>
        /// Adds the user's associated role as a Claim.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns>ClaimsPrincipal</returns>
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            // string adId = Redacted
            User user = dbContext.Users.Include(u => u.Role).Where(u => u.AdID == adId).AsNoTracking().FirstOrDefault();

            if (user != null)
            {
                var identity = (ClaimsIdentity)principal.Identity;

                var claim = new Claim("role", ((RoleEnums.Roles)user.Role.PermissionLevel).ToString());
                identity.AddClaim(claim);

                // initialize services
                _userPreference.Initialize(adId);
            }

            return Task.FromResult(principal);
        }
    }
}

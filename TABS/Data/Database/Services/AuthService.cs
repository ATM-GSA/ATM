using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace TABS.Data
{
    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _auth;

        public AuthService(AuthenticationStateProvider authenticationStateProvider)
        {
            _auth = authenticationStateProvider;
        }

        #region Get Users SID
        /// <summary>
        /// Retrieve the authenticated User's SID.
        /// Returns "<UNKNOWN SID>" on failure.
        /// </summary>
        /// <returns>User's SID</returns>
        public async Task<string> GetUserSID()
        {
            string sid = "<UNKNOWN SID>";

            try
            {
                var user = await _auth.GetAuthenticationStateAsync();
                sid = user.User.Claims.Where(c => c.Type.Contains("primarysid"))
                                   .Select(c => c.Value)
                                   .FirstOrDefault();
            }
            catch
            {
                // do nothing - sid has already been defaulted
            }

            return sid;

        }
        #endregion

        #region Get Users Primary Name
        /// <summary>
        /// Retrieve the authenticated User's Primary Name.
        /// Returns "<UNKNOWN PRIMARY NAME>" on failure.
        /// </summary>
        /// <returns>User's SID</returns>
        public async Task<string> GetUsersPrimaryName()
        {
            string name = "<UNKNOWN PRIMARY NAME>";

            try
            {
                var user = await _auth.GetAuthenticationStateAsync();
                name = user.User.Identity.Name;
            }
            catch
            {
                // do nothing - sid has already been defaulted
            }

            return name;
        }
        #endregion
    }
}

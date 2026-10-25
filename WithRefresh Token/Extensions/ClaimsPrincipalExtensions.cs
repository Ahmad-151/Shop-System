using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetPersonId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(JwtRegisteredClaimNames.Sub)
                     ?? user.FindFirst(ClaimTypes.NameIdentifier);

            return claim != null && int.TryParse(claim.Value, out var id)
                ? id
                : null;
        }
    }
}
using System.Security.Claims;

namespace PKC.Web.Extensions;

public static class HttpContextExtensions
{

    public static Guid GetUserId(this HttpContext context)
    {
        var userId = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                     ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return Guid.Parse(userId);
    }
}
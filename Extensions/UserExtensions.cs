using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

public static class UserExtensions
{

	public static bool IsValidUser(HttpContext context) 
	{
		var identity = context.User.Identity;
		return identity != null && identity.IsAuthenticated;
	}

	public static bool TryGetUserId(HttpContext context, [NotNullWhen(true)] out string? userId) 
	{
		userId = null;

		if (!IsValidUser(context)) {
			return false;
		}

		var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);
		userId = claim == null ? null : claim.Value;
		
		return userId != null;
	}
}

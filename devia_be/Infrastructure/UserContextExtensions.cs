using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace devia_be.Infrastructure;

public static class UserContextExtensions
{
    public static Guid? CurrentUserId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(ClaimTypes.Name);
        return Guid.TryParse(identifier, out var userId) ? userId : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
    }

    public static bool CanAccessUser(this ClaimsPrincipal user, Guid targetUserId)
    {
        var currentUserId = user.CurrentUserId();
        return currentUserId.HasValue && (currentUserId.Value == targetUserId || user.IsAdmin());
    }

    public static ActionResult UnauthorizedOrForbidden(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true
            ? new ForbidResult()
            : new UnauthorizedResult();
    }
}

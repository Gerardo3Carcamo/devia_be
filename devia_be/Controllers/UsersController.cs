using devia_be.Contracts;
using devia_be.Data;
using devia_be.Domain;
using devia_be.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace devia_be.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = nameof(AuthRole.Admin))]
    public async Task<ActionResult<IReadOnlyCollection<UserProfileResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user => user.AuthRole == AuthRole.User)
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(user => user.ToProfileResponse()).ToArray());
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var currentUserId = User.CurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == currentUserId.Value, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user.ToProfileResponse());
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserProfileResponse>> GetById(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.CanAccessUser(userId))
        {
            return User.UnauthorizedOrForbidden();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.ToProfileResponse());
    }

    [HttpGet("{userId:guid}/apps")]
    public async Task<ActionResult<IReadOnlyCollection<AppResponse>>> GetUserApps(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.CanAccessUser(userId))
        {
            return User.UnauthorizedOrForbidden();
        }

        var apps = await dbContext.Apps
            .Where(app => app.UserId == userId)
            .OrderByDescending(app => app.LastUpdateUtc)
            .ToListAsync(cancellationToken);

        return Ok(apps.Select(app => app.ToResponse()).ToArray());
    }

    [HttpGet("{userId:guid}/deployments")]
    public async Task<ActionResult<IReadOnlyCollection<DeploymentResponse>>> GetUserDeployments(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!User.CanAccessUser(userId))
        {
            return User.UnauthorizedOrForbidden();
        }

        var deployments = await dbContext.Deployments
            .Where(deployment => deployment.UserId == userId)
            .Include(deployment => deployment.App)
            .OrderByDescending(deployment => deployment.TimestampUtc)
            .ToListAsync(cancellationToken);

        return Ok(deployments.Select(deployment => deployment.ToResponse()).ToArray());
    }

    [HttpGet("{userId:guid}/subscriptions")]
    public async Task<ActionResult<IReadOnlyCollection<SubscriptionResponse>>> GetUserSubscriptions(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!User.CanAccessUser(userId))
        {
            return User.UnauthorizedOrForbidden();
        }

        var subscriptions = await dbContext.Subscriptions
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(subscriptions.Select(subscription => subscription.ToResponse()).ToArray());
    }
}

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
public sealed class AppsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AppResponse>>> GetApps(
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var requestedUserId = userId ?? User.CurrentUserId();
        if (!requestedUserId.HasValue)
        {
            return Unauthorized();
        }

        if (!User.CanAccessUser(requestedUserId.Value))
        {
            return User.UnauthorizedOrForbidden();
        }

        var apps = await dbContext.Apps
            .Where(app => app.UserId == requestedUserId.Value)
            .OrderByDescending(app => app.LastUpdateUtc)
            .ToListAsync(cancellationToken);

        return Ok(apps.Select(app => app.ToResponse()).ToArray());
    }

    [HttpGet("{appId:guid}")]
    public async Task<ActionResult<AppResponse>> GetById(Guid appId, CancellationToken cancellationToken)
    {
        var app = await dbContext.Apps.FirstOrDefaultAsync(candidate => candidate.Id == appId, cancellationToken);
        if (app is null)
        {
            return NotFound();
        }

        if (!User.CanAccessUser(app.UserId))
        {
            return User.UnauthorizedOrForbidden();
        }

        return Ok(app.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<AppResponse>> Create([FromBody] CreateAppRequest request, CancellationToken cancellationToken)
    {
        var userId = User.CurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId.Value, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var app = new GeneratedApp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdateUtc = DateTime.UtcNow
        };

        dbContext.Apps.Add(app);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { appId = app.Id }, app.ToResponse());
    }

    [HttpPatch("{appId:guid}/status")]
    public async Task<ActionResult<AppResponse>> UpdateStatus(
        Guid appId,
        [FromBody] UpdateAppStatusRequest request,
        CancellationToken cancellationToken)
    {
        var app = await dbContext.Apps.FirstOrDefaultAsync(candidate => candidate.Id == appId, cancellationToken);
        if (app is null)
        {
            return NotFound();
        }

        if (!User.CanAccessUser(app.UserId))
        {
            return User.UnauthorizedOrForbidden();
        }

        app.Status = request.Status;
        app.LastUpdateUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(app.ToResponse());
    }
}

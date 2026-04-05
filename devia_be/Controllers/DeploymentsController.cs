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
public sealed class DeploymentsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DeploymentResponse>>> GetDeployments(
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

        var deployments = await dbContext.Deployments
            .Where(deployment => deployment.UserId == requestedUserId.Value)
            .Include(deployment => deployment.App)
            .OrderByDescending(deployment => deployment.TimestampUtc)
            .ToListAsync(cancellationToken);

        return Ok(deployments.Select(deployment => deployment.ToResponse()).ToArray());
    }

    [HttpPost("run")]
    public async Task<ActionResult<DeploymentResponse>> RunDeployment(
        [FromBody] CreateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.CurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var app = await dbContext.Apps
            .Include(candidate => candidate.User)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.AppId, cancellationToken);
        if (app is null)
        {
            return NotFound(new { message = "No se encontro la app solicitada." });
        }

        if (!User.CanAccessUser(app.UserId))
        {
            return User.UnauthorizedOrForbidden();
        }

        var deployment = new DeploymentRecord
        {
            Id = Guid.NewGuid(),
            UserId = app.UserId,
            AppId = app.Id,
            Environment = request.Environment,
            Status = DeploymentStatus.Exitoso,
            TimestampUtc = DateTime.UtcNow,
            DurationSeconds = Random.Shared.Next(48, 145)
        };

        app.LastUpdateUtc = DateTime.UtcNow;

        dbContext.Deployments.Add(deployment);
        await dbContext.SaveChangesAsync(cancellationToken);

        deployment.App = app;
        return CreatedAtAction(nameof(GetDeployments), new { userId = deployment.UserId }, deployment.ToResponse());
    }
}

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
public sealed class DashboardController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDashboardResponse>> GetMyDashboard(CancellationToken cancellationToken)
    {
        var currentUserId = User.CurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        return await BuildUserDashboard(currentUserId.Value, cancellationToken);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<UserDashboardResponse>> GetUserDashboard(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.CanAccessUser(userId))
        {
            return User.UnauthorizedOrForbidden();
        }

        return await BuildUserDashboard(userId, cancellationToken);
    }

    [HttpGet("admin/overview")]
    [Authorize(Roles = nameof(AuthRole.Admin))]
    public async Task<ActionResult<AdminOverviewResponse>> GetAdminOverview(CancellationToken cancellationToken)
    {
        var customerUsers = dbContext.Users.Where(user => user.AuthRole == AuthRole.User);
        var activeUsers = customerUsers.Where(user => user.Status == UserStatus.Activo);
        var activeApps = dbContext.Apps.Where(app => app.Status != AppStatus.Archivada);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var revenueSubscriptions = await dbContext.Subscriptions
            .Where(subscription => subscription.Status != SubscriptionStatus.Cancelada)
            .Select(subscription => subscription.PriceMonthly)
            .ToListAsync(cancellationToken);

        var response = new AdminOverviewResponse
        {
            TotalUsers = await customerUsers.CountAsync(cancellationToken),
            ActiveUsers = await activeUsers.CountAsync(cancellationToken),
            TotalApps = await dbContext.Apps.CountAsync(cancellationToken),
            ActiveApps = await activeApps.CountAsync(cancellationToken),
            DeploymentsThisMonth = await dbContext.Deployments
                .CountAsync(deployment => deployment.TimestampUtc >= monthStart && deployment.TimestampUtc < monthEnd,
                    cancellationToken),
            MonthlyRevenue = revenueSubscriptions.Sum()
        };

        return Ok(response);
    }

    [HttpGet("admin/revenue-by-plan")]
    [Authorize(Roles = nameof(AuthRole.Admin))]
    public async Task<ActionResult<IReadOnlyCollection<RevenueByPlanResponse>>> GetRevenueByPlan(
        CancellationToken cancellationToken)
    {
        var subscriptions = await dbContext.Subscriptions
            .Where(subscription => subscription.Status != SubscriptionStatus.Cancelada)
            .ToListAsync(cancellationToken);
        var rows = subscriptions
            .GroupBy(subscription => subscription.Plan)
            .Select(group => new { Plan = group.Key, Revenue = group.Sum(item => item.PriceMonthly) })
            .ToList();

        var plans = new[] { PlanTier.Starter, PlanTier.Growth, PlanTier.Scale };
        var grandTotal = rows.Sum(row => row.Revenue);

        var response = plans.Select(plan =>
        {
            var revenue = rows.FirstOrDefault(row => row.Plan == plan)?.Revenue ?? 0m;
            var share = grandTotal > 0m ? (double)(revenue / grandTotal) : 0d;
            return new RevenueByPlanResponse
            {
                Plan = plan,
                Revenue = revenue,
                Share = share
            };
        }).ToArray();

        return Ok(response);
    }

    [HttpGet("admin/user-performance")]
    [Authorize(Roles = nameof(AuthRole.Admin))]
    public async Task<ActionResult<IReadOnlyCollection<UserPerformanceResponse>>> GetUserPerformance(
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user => user.AuthRole == AuthRole.User)
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

        var appCounts = await dbContext.Apps
            .GroupBy(app => app.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.UserId, row => row.Count, cancellationToken);

        var deploymentRows = await dbContext.Deployments
            .GroupBy(deployment => deployment.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                Total = group.Count(),
                Success = group.Count(item => item.Status == DeploymentStatus.Exitoso)
            })
            .ToDictionaryAsync(row => row.UserId, row => row, cancellationToken);

        var revenueSubscriptions = await dbContext.Subscriptions
            .Where(subscription => subscription.Status != SubscriptionStatus.Cancelada)
            .ToListAsync(cancellationToken);
        var revenueRows = revenueSubscriptions
            .GroupBy(subscription => subscription.UserId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.PriceMonthly));

        var response = users.Select(user =>
        {
            deploymentRows.TryGetValue(user.Id, out var deployment);
            var totalDeployments = deployment?.Total ?? 0;
            var successRate = totalDeployments > 0 ? (double)(deployment?.Success ?? 0) / totalDeployments : 0d;

            return new UserPerformanceResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Plan = user.Plan,
                Status = user.Status,
                Apps = appCounts.TryGetValue(user.Id, out var appCount) ? appCount : 0,
                Deployments = totalDeployments,
                SuccessRate = successRate,
                MonthlyRevenue = revenueRows.TryGetValue(user.Id, out var revenue) ? revenue : 0m
            };
        }).OrderByDescending(row => row.MonthlyRevenue).ToArray();

        return Ok(response);
    }

    [HttpGet("admin/recent-deployments")]
    [Authorize(Roles = nameof(AuthRole.Admin))]
    public async Task<ActionResult<IReadOnlyCollection<DeploymentResponse>>> GetRecentDeployments(
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);
        var deployments = await dbContext.Deployments
            .Include(deployment => deployment.App)
            .OrderByDescending(deployment => deployment.TimestampUtc)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return Ok(deployments.Select(deployment => deployment.ToResponse()).ToArray());
    }

    private async Task<ActionResult<UserDashboardResponse>> BuildUserDashboard(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var apps = await dbContext.Apps
            .Where(app => app.UserId == userId)
            .OrderByDescending(app => app.LastUpdateUtc)
            .ToListAsync(cancellationToken);

        var deployments = await dbContext.Deployments
            .Where(deployment => deployment.UserId == userId)
            .Include(deployment => deployment.App)
            .OrderByDescending(deployment => deployment.TimestampUtc)
            .ToListAsync(cancellationToken);

        var subscriptions = await dbContext.Subscriptions
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var monthlyRevenue = subscriptions
            .Where(subscription => subscription.Status != SubscriptionStatus.Cancelada)
            .Sum(subscription => subscription.PriceMonthly);

        var successRate = deployments.Count > 0
            ? deployments.Count(deployment => deployment.Status == DeploymentStatus.Exitoso) / (double)deployments.Count
            : 0d;

        var response = new UserDashboardResponse
        {
            Profile = user.ToProfileResponse(),
            Apps = apps.Select(app => app.ToResponse()).ToArray(),
            Deployments = deployments.Select(deployment => deployment.ToResponse()).ToArray(),
            Subscriptions = subscriptions.Select(subscription => subscription.ToResponse()).ToArray(),
            MonthlyRevenue = monthlyRevenue,
            SuccessRate = successRate
        };

        return Ok(response);
    }
}

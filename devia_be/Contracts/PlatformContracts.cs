using System.ComponentModel.DataAnnotations;
using devia_be.Domain;

namespace devia_be.Contracts;

public sealed class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public UserStatus Status { get; set; } = UserStatus.Activo;
    public DateTime JoinedAtUtc { get; set; }
    public string AvatarInitial { get; set; } = string.Empty;
}

public sealed class AppResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AppStatus Status { get; set; } = AppStatus.Beta;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdateUtc { get; set; }
}

public sealed class DeploymentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Staging;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.EnCola;
    public DateTime TimestampUtc { get; set; }
    public int DurationSeconds { get; set; }
}

public sealed class SubscriptionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public decimal PriceMonthly { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Activa;
}

public sealed class UserDashboardResponse
{
    public UserProfileResponse Profile { get; set; } = new();
    public IReadOnlyCollection<AppResponse> Apps { get; set; } = Array.Empty<AppResponse>();
    public IReadOnlyCollection<DeploymentResponse> Deployments { get; set; } = Array.Empty<DeploymentResponse>();
    public IReadOnlyCollection<SubscriptionResponse> Subscriptions { get; set; } = Array.Empty<SubscriptionResponse>();
    public decimal MonthlyRevenue { get; set; }
    public double SuccessRate { get; set; }
}

public sealed class AdminOverviewResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalApps { get; set; }
    public int ActiveApps { get; set; }
    public int DeploymentsThisMonth { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

public sealed class RevenueByPlanResponse
{
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public decimal Revenue { get; set; }
    public double Share { get; set; }
}

public sealed class UserPerformanceResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public UserStatus Status { get; set; } = UserStatus.Activo;
    public int Apps { get; set; }
    public int Deployments { get; set; }
    public double SuccessRate { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

public sealed class CreateAppRequest
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Type { get; set; } = string.Empty;

    public AppStatus Status { get; set; } = AppStatus.Beta;
}

public sealed class UpdateAppStatusRequest
{
    [Required]
    public AppStatus Status { get; set; }
}

public sealed class CreateDeploymentRequest
{
    [Required]
    public Guid AppId { get; set; }

    [Required]
    public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Staging;
}

namespace devia_be.Domain;

public sealed class UserAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AuthRole AuthRole { get; set; } = AuthRole.User;
    public string ProfileRole { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public UserStatus Status { get; set; } = UserStatus.Activo;
    public DateTime JoinedAtUtc { get; set; }
    public string AvatarInitial { get; set; } = string.Empty;

    public ICollection<GeneratedApp> Apps { get; set; } = new List<GeneratedApp>();
    public ICollection<DeploymentRecord> Deployments { get; set; } = new List<DeploymentRecord>();
    public ICollection<SubscriptionRecord> Subscriptions { get; set; } = new List<SubscriptionRecord>();
}

public sealed class GeneratedApp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AppStatus Status { get; set; } = AppStatus.Beta;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdateUtc { get; set; }

    public ICollection<DeploymentRecord> Deployments { get; set; } = new List<DeploymentRecord>();
}

public sealed class DeploymentRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }
    public Guid AppId { get; set; }
    public GeneratedApp? App { get; set; }
    public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Staging;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.EnCola;
    public DateTime TimestampUtc { get; set; }
    public int DurationSeconds { get; set; }
}

public sealed class SubscriptionRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }
    public PlanTier Plan { get; set; } = PlanTier.Starter;
    public decimal PriceMonthly { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
}

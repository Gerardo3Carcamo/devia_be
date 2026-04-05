using devia_be.Contracts;
using devia_be.Domain;

namespace devia_be.Infrastructure;

public static class ApiMappers
{
    public static UserProfileResponse ToProfileResponse(this UserAccount user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.ProfileRole,
            Location = user.Location,
            Plan = user.Plan,
            Status = user.Status,
            JoinedAtUtc = user.JoinedAtUtc,
            AvatarInitial = user.AvatarInitial
        };
    }

    public static SessionUserResponse ToSessionResponse(this UserAccount user)
    {
        return new SessionUserResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.AuthRole.ToString().ToLowerInvariant()
        };
    }

    public static AppResponse ToResponse(this GeneratedApp app)
    {
        return new AppResponse
        {
            Id = app.Id,
            UserId = app.UserId,
            Name = app.Name,
            Type = app.Type,
            Status = app.Status,
            CreatedAtUtc = app.CreatedAtUtc,
            LastUpdateUtc = app.LastUpdateUtc
        };
    }

    public static DeploymentResponse ToResponse(this DeploymentRecord deployment)
    {
        return new DeploymentResponse
        {
            Id = deployment.Id,
            UserId = deployment.UserId,
            AppId = deployment.AppId,
            AppName = deployment.App?.Name ?? "App",
            Environment = deployment.Environment,
            Status = deployment.Status,
            TimestampUtc = deployment.TimestampUtc,
            DurationSeconds = deployment.DurationSeconds
        };
    }

    public static SubscriptionResponse ToResponse(this SubscriptionRecord subscription)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            Plan = subscription.Plan,
            PriceMonthly = subscription.PriceMonthly,
            StartedAtUtc = subscription.StartedAtUtc,
            Status = subscription.Status
        };
    }
}

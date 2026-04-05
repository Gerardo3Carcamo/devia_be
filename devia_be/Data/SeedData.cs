using System.Globalization;
using devia_be.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace devia_be.Data;

public static class SeedData
{
    private static readonly Guid AdminId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid U1 = Guid.Parse("10000000-0000-0000-0000-000000000011");
    private static readonly Guid U2 = Guid.Parse("10000000-0000-0000-0000-000000000012");
    private static readonly Guid U3 = Guid.Parse("10000000-0000-0000-0000-000000000013");
    private static readonly Guid U4 = Guid.Parse("10000000-0000-0000-0000-000000000014");

    private static readonly Guid A1 = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid A2 = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid A3 = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid A4 = Guid.Parse("20000000-0000-0000-0000-000000000004");
    private static readonly Guid A5 = Guid.Parse("20000000-0000-0000-0000-000000000005");
    private static readonly Guid A6 = Guid.Parse("20000000-0000-0000-0000-000000000006");
    private static readonly Guid A7 = Guid.Parse("20000000-0000-0000-0000-000000000007");

    public static async Task InitializeAsync(AppDbContext dbContext, IPasswordHasher<UserAccount> passwordHasher)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var users = new List<UserAccount>
        {
            new()
            {
                Id = AdminId,
                Name = "Devia Admin",
                Email = "admin@devia.app",
                AuthRole = AuthRole.Admin,
                ProfileRole = "Platform Admin",
                Location = "Monterrey, MX",
                Plan = PlanTier.Scale,
                Status = UserStatus.Activo,
                JoinedAtUtc = Utc("2025-10-01"),
                AvatarInitial = "A"
            },
            new()
            {
                Id = U1,
                Name = "Camila Ortega",
                Email = "camila@builderlabs.com",
                AuthRole = AuthRole.User,
                ProfileRole = "Founder",
                Location = "Monterrey, MX",
                Plan = PlanTier.Scale,
                Status = UserStatus.Activo,
                JoinedAtUtc = Utc("2025-11-18"),
                AvatarInitial = "C"
            },
            new()
            {
                Id = U2,
                Name = "Diego Mendez",
                Email = "diego@orbitapps.io",
                AuthRole = AuthRole.User,
                ProfileRole = "Product Manager",
                Location = "CDMX, MX",
                Plan = PlanTier.Growth,
                Status = UserStatus.Activo,
                JoinedAtUtc = Utc("2026-01-12"),
                AvatarInitial = "D"
            },
            new()
            {
                Id = U3,
                Name = "Valeria Ramos",
                Email = "valeria@clinicflow.dev",
                AuthRole = AuthRole.User,
                ProfileRole = "Operations Lead",
                Location = "Guadalajara, MX",
                Plan = PlanTier.Starter,
                Status = UserStatus.Prueba,
                JoinedAtUtc = Utc("2026-02-09"),
                AvatarInitial = "V"
            },
            new()
            {
                Id = U4,
                Name = "Sebastian Gil",
                Email = "sebastian@legacysoft.mx",
                AuthRole = AuthRole.User,
                ProfileRole = "CTO",
                Location = "Bogota, CO",
                Plan = PlanTier.Growth,
                Status = UserStatus.Inactivo,
                JoinedAtUtc = Utc("2025-10-01"),
                AvatarInitial = "S"
            }
        };

        foreach (var user in users)
        {
            var password = user.AuthRole == AuthRole.Admin ? "Admin123!" : "Demo123!";
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }

        dbContext.Users.AddRange(users);

        var apps = new[]
        {
            new GeneratedApp
            {
                Id = A1,
                UserId = U1,
                Name = "Booking Pulse",
                Type = "Reservas + pagos",
                Status = AppStatus.Produccion,
                CreatedAtUtc = Utc("2026-02-02"),
                LastUpdateUtc = Utc("2026-04-04")
            },
            new GeneratedApp
            {
                Id = A2,
                UserId = U1,
                Name = "Store Beacon",
                Type = "Inventario omnicanal",
                Status = AppStatus.Beta,
                CreatedAtUtc = Utc("2026-01-19"),
                LastUpdateUtc = Utc("2026-04-03")
            },
            new GeneratedApp
            {
                Id = A3,
                UserId = U1,
                Name = "Sales Orbit",
                Type = "Dashboard comercial",
                Status = AppStatus.Produccion,
                CreatedAtUtc = Utc("2025-12-11"),
                LastUpdateUtc = Utc("2026-04-01")
            },
            new GeneratedApp
            {
                Id = A4,
                UserId = U2,
                Name = "Workforce Grid",
                Type = "Gestion de turnos",
                Status = AppStatus.Produccion,
                CreatedAtUtc = Utc("2026-01-27"),
                LastUpdateUtc = Utc("2026-04-05")
            },
            new GeneratedApp
            {
                Id = A5,
                UserId = U2,
                Name = "Event Horizon",
                Type = "Eventos y tickets",
                Status = AppStatus.Mantenimiento,
                CreatedAtUtc = Utc("2026-02-16"),
                LastUpdateUtc = Utc("2026-04-02")
            },
            new GeneratedApp
            {
                Id = A6,
                UserId = U3,
                Name = "Clinic Flow",
                Type = "Agenda medica",
                Status = AppStatus.Beta,
                CreatedAtUtc = Utc("2026-02-12"),
                LastUpdateUtc = Utc("2026-04-04")
            },
            new GeneratedApp
            {
                Id = A7,
                UserId = U4,
                Name = "Legacy Forms",
                Type = "Flujos internos",
                Status = AppStatus.Archivada,
                CreatedAtUtc = Utc("2025-11-08"),
                LastUpdateUtc = Utc("2026-03-22")
            }
        };

        dbContext.Apps.AddRange(apps);

        var deployments = new[]
        {
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000001"), U1, A1, DeploymentEnvironment.Produccion,
                DeploymentStatus.Exitoso, "2026-04-05T10:12:00", 78),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000002"), U1, A2, DeploymentEnvironment.Staging,
                DeploymentStatus.Exitoso, "2026-04-05T08:45:00", 64),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000003"), U1, A3, DeploymentEnvironment.Produccion,
                DeploymentStatus.Fallido, "2026-04-03T21:10:00", 123),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000004"), U2, A4, DeploymentEnvironment.Produccion,
                DeploymentStatus.Exitoso, "2026-04-04T14:36:00", 80),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000005"), U2, A5, DeploymentEnvironment.Staging,
                DeploymentStatus.Exitoso, "2026-04-02T11:22:00", 59),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000006"), U3, A6, DeploymentEnvironment.Staging,
                DeploymentStatus.EnCola, "2026-04-05T11:06:00", 0),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000007"), U3, A6, DeploymentEnvironment.Staging,
                DeploymentStatus.Exitoso, "2026-04-04T17:52:00", 88),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000008"), U4, A7, DeploymentEnvironment.Produccion,
                DeploymentStatus.Fallido, "2026-03-30T09:15:00", 140),
            CreateDeployment(Guid.Parse("30000000-0000-0000-0000-000000000009"), U2, A4, DeploymentEnvironment.Produccion,
                DeploymentStatus.Exitoso, "2026-04-01T06:42:00", 74)
        };

        dbContext.Deployments.AddRange(deployments);

        var subscriptions = new[]
        {
            new SubscriptionRecord
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                UserId = U1,
                Plan = PlanTier.Scale,
                PriceMonthly = 399,
                StartedAtUtc = Utc("2025-11-18"),
                Status = SubscriptionStatus.Activa
            },
            new SubscriptionRecord
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                UserId = U2,
                Plan = PlanTier.Growth,
                PriceMonthly = 199,
                StartedAtUtc = Utc("2026-01-12"),
                Status = SubscriptionStatus.Activa
            },
            new SubscriptionRecord
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                UserId = U3,
                Plan = PlanTier.Starter,
                PriceMonthly = 49,
                StartedAtUtc = Utc("2026-02-09"),
                Status = SubscriptionStatus.Trial
            },
            new SubscriptionRecord
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                UserId = U4,
                Plan = PlanTier.Growth,
                PriceMonthly = 199,
                StartedAtUtc = Utc("2025-10-01"),
                Status = SubscriptionStatus.Cancelada
            }
        };

        dbContext.Subscriptions.AddRange(subscriptions);
        await dbContext.SaveChangesAsync();
    }

    private static DeploymentRecord CreateDeployment(
        Guid id,
        Guid userId,
        Guid appId,
        DeploymentEnvironment environment,
        DeploymentStatus status,
        string timestamp,
        int durationSeconds)
    {
        return new DeploymentRecord
        {
            Id = id,
            UserId = userId,
            AppId = appId,
            Environment = environment,
            Status = status,
            TimestampUtc = Utc(timestamp),
            DurationSeconds = durationSeconds
        };
    }

    private static DateTime Utc(string value)
    {
        var parsed = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }
}

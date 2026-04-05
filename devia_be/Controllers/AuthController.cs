using devia_be.Contracts;
using devia_be.Data;
using devia_be.Domain;
using devia_be.Infrastructure;
using devia_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace devia_be.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    AppDbContext dbContext,
    IPasswordHasher<UserAccount> passwordHasher,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Credenciales invalidas." });
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification is PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Credenciales invalidas." });
        }

        var (token, expiresAtUtc) = tokenService.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToSessionResponse()
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ese correo ya esta registrado." });
        }

        var name = request.Name.Trim();
        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = normalizedEmail,
            AuthRole = AuthRole.User,
            ProfileRole = "Owner",
            Location = request.Location.Trim(),
            Plan = request.Plan,
            Status = UserStatus.Activo,
            JoinedAtUtc = DateTime.UtcNow,
            AvatarInitial = name[..1].ToUpperInvariant()
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var subscription = new SubscriptionRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Plan = request.Plan,
            PriceMonthly = request.Plan switch
            {
                PlanTier.Growth => 199m,
                PlanTier.Scale => 399m,
                _ => 49m
            },
            StartedAtUtc = DateTime.UtcNow,
            Status = SubscriptionStatus.Activa
        };

        dbContext.Users.Add(user);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = tokenService.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToSessionResponse()
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<SessionUserResponse>> Me(CancellationToken cancellationToken)
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

        return Ok(user.ToSessionResponse());
    }
}

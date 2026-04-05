using devia_be.Domain;

namespace devia_be.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(UserAccount user);
}

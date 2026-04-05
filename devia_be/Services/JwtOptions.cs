namespace devia_be.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "devia-api";
    public string Audience { get; set; } = "devia-client";
    public string SecretKey { get; set; } = "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_32_CHARS_MIN";
    public int ExpiresHours { get; set; } = 12;
}

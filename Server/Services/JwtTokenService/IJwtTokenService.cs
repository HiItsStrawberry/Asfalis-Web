namespace asfalis.Server.Services.JwtTokenService
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, bool isMobile = false);
        Task<User> ValidateToken(string token, bool isMobile = false);
    }
}
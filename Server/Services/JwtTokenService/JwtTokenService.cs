using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace asfalis.Server.Services.JwtTokenService
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public JwtTokenService(IConfiguration config, IUserService userService)
        {
            this._config = config;
            this._userService = userService;
        }

        // A method to generate a Jwt token
        public string GenerateToken(User user, bool isMobile = false)
        {
            try
            {
                // Set the token expiry time to 10 minutes for desktop
                string isMobileUser = "false";
                var expiryDate = Helpers.GetCurrentDate(10);

                // Set the token expiry time to 30 minutes for mobile
                if (isMobile)
                {
                    isMobileUser = "true";
                    expiryDate = Helpers.GetCurrentDate(30);
                }

                // User claims
                var claims = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Gender, user.Gender.ToGender()),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("IsMobileUser", isMobileUser)
                });

                // Setup the issuer, audience and secret key
                var issuer = _config["JwtKey:Issuer"];
                var audience = _config["JwtKey:Audience"];
                var secKey = Encoding.UTF8.GetBytes(_config["JwtKey:SecretKey"]);

                // Tells the secret key for validating the credential is using HmacSha256
                var hashKey = new SigningCredentials(
                    new SymmetricSecurityKey(secKey),
                    SecurityAlgorithms.HmacSha256Signature
                );

                // Create a token descriptor based on the information above
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Issuer = issuer,
                    Audience = audience,
                    Expires = expiryDate,
                    SigningCredentials = hashKey,
                };

                // Token generation
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                //return token;
                return tokenHandler.WriteToken(token);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return string.Empty;
            }
        }


        public async Task<User> ValidateToken(string token, bool isMobile = false)
        {
            try
            {
                // Getting validation props
                var issuer = _config["JwtKey:Issuer"];
                var audience = _config["JwtKey:Audience"];
                var secKey = Encoding.UTF8.GetBytes(_config["JwtKey:SecretKey"]);

                // Validating the token with the validation parameters
                var tokenHandler = new JwtSecurityTokenHandler();

                // Validate the token based on the issuer, audience, secret key, and expiry time
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secKey),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken secToken);

                // A valid token if the validation is passed
                var jwtToken = (JwtSecurityToken)secToken;

                // Check if the token is not null and using the same encryption
                if (jwtToken != null && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    // Getting the user if the token is valid
                    var userId = jwtToken.Claims.First(x => x.Type == "nameid")?.Value;
                    var username = jwtToken.Claims.First(x => x.Type == "unique_name")?.Value;
                    return await _userService.GetUser(userId.ToInt(), username);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                if (isMobile)
                {
                    // Refresh the token if the request is sent from mobile
                    var identity = new ClaimsIdentity(token.ParseClaimsFromJwt(), "jwt");

                    var userId = identity.Claims.First(x => x.Type == "nameid")?.Value;
                    var username = identity.Claims.First(x => x.Type == "unique_name")?.Value;

                    return await _userService.GetUser(userId.ToInt(), username);
                }
            }
            // return null if the token is invalid
            return null!;
        }
    }
}

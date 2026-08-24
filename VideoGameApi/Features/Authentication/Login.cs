using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static VideoGameApi.Features.Authentication.Login;

namespace VideoGameApi.Features.Authentication
{
    public class Login
    {
        public record Command(string Email, string Password) : IRequest<Result?>;

        public record Result(
            string AccessToken,
            DateTime ExpiresAtUtc);

        public sealed class Handler(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration)
            : IRequestHandler<Command, Result?>
        {
            public async Task<Result?> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var user = await userManager.FindByEmailAsync(request.Email);

                if (user is null)
                {
                    return null;
                }

                var validPassword = await userManager.CheckPasswordAsync(
                    user,
                    request.Password);

                if (!validPassword)
                {
                    return null;
                }

                var key = configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT key is not configured.");

                var issuer = configuration["Jwt:Issuer"]
                    ?? throw new InvalidOperationException(
                        "JWT issuer is not configured.");

                var audience = configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException(
                        "JWT audience is not configured.");

                var expiresAt = DateTime.UtcNow.AddHours(1);

                var claims = new[]
                {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? request.Email),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

                var signingKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

                var credentials = new SigningCredentials(
                    signingKey,
                    SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiresAt,
                    signingCredentials: credentials);

                var accessToken = new JwtSecurityTokenHandler()
                    .WriteToken(token);

                return new Result(accessToken, expiresAt);
            }
        }
    }
    [ApiController]
    [Route("api/auth")]
    public sealed class LoginController(ISender sender) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<Result>> Login(
            Command command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);

            if (result is null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            return Ok(result);
        }
    }
}
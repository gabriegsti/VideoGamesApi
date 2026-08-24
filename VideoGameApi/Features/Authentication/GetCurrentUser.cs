using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static VideoGameApi.Features.Authentication.GetCurrentUser;

namespace VideoGameApi.Features.Authentication
{
    public class GetCurrentUser
    {
        public record Query : IRequest<Response?>;

        public record Response(
            string Id,
            string? Email,
            string? UserName);

        public sealed class Handler(
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor)
            : IRequestHandler<Query, Response?>
        {
            public async Task<Response?> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var userId = httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return null;
                }

                var user = await userManager.FindByIdAsync(userId);

                return user is null
                    ? null
                    : new Response(
                        user.Id,
                        user.Email,
                        user.UserName);
            }
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/auth")]
    public sealed class GetCurrentUserController(ISender sender) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<ActionResult<Response>> Me(
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new Query(),
                cancellationToken);

            return result is null
                ? Unauthorized()
                : Ok(result);
        }
    }
}

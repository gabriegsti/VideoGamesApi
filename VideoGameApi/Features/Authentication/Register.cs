using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static VideoGameApi.Features.Authentication.Register;

namespace VideoGameApi.Features.Authentication
{
    public class Register
    {
        public record Command(string UserName, string Email, string Password) : IRequest<Result>;

        public record Result(
            bool Succeeded,
            string? UserId,
            IEnumerable<string> Errors);

        public sealed class Handler(
            UserManager<IdentityUser> userManager)
            : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var user = new IdentityUser
                {
                    UserName = request.UserName,
                    Email = request.Email
                };

                var result = await userManager.CreateAsync(
                    user,
                    request.Password);

                if (!result.Succeeded)
                {
                    return new Result(
                        false,
                        null,
                        result.Errors.Select(error => error.Description));
                }

                return new Result(true, user.Id, []);
            }
        }
    }

    [ApiController]
    [Route("api/auth")]
    public sealed class RegisterController(ISender sender) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult> Register(
            Command command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors
                });
            }

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    userId = result.UserId,
                    email = command.Email
                });
        }
    }
}
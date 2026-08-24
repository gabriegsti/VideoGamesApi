using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameApi.Data;

namespace VideoGameApi.Features.UserGames;

public class GetMyLibrary
{
    public record Query(
        bool? Owned = null,
        bool? Played = null) : IRequest<Response?>;

    public record Response(
        IReadOnlyList<Game> Games);

    public record Game(
        int VideoGameId,
        string Title,
        string Genre,
        int ReleaseYear,
        bool IsOwned,
        bool IsPlayed,
        DateTime AddedAt);

    public sealed class Handler(
        VideoGameDBContext context,
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

            var query = context.UserGames
                .AsNoTracking()
                .Where(userGame => userGame.UserId == userId);

            if (request.Owned.HasValue)
            {
                query = query.Where(userGame =>
                    userGame.IsOwned == request.Owned.Value);
            }

            if (request.Played.HasValue)
            {
                query = query.Where(userGame =>
                    userGame.IsPlayed == request.Played.Value);
            }

            var games = await query
                .OrderBy(userGame => userGame.VideoGame.Title)
                .Select(userGame => new Game(
                    userGame.VideoGameId,
                    userGame.VideoGame.Title,
                    userGame.VideoGame.Genre,
                    userGame.VideoGame.ReleaseYear,
                    userGame.IsOwned,
                    userGame.IsPlayed,
                    userGame.AddedAt))
                .ToListAsync(cancellationToken);

            return new Response(games);
        }
    }
}

[Authorize]
[ApiController]
[Route("api/me/library")]
public sealed class GetMyLibraryController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetMyLibrary.Response>> GetLibrary(
        [FromQuery] bool? owned,
        [FromQuery] bool? played,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetMyLibrary.Query(owned, played),
            cancellationToken);

        return result is null
            ? Unauthorized()
            : Ok(result);
    }
}

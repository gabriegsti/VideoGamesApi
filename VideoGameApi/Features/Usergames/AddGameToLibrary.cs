using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameApi.Data;
using VideoGameApi.Entities;
using static VideoGameApi.Features.UserGames.AddGameToLibrary;

namespace VideoGameApi.Features.UserGames;

public static class AddGameToLibrary
{
    public record Command(
        int VideoGameId,
        bool IsOwned,
        bool IsPlayed) : IRequest<Result>;

    public record Result(
        Response? Data,
        string? Error);

    public record Response(
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
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return new Result(null, "Unauthorized");
            }

            var videoGame = await context.VideoGames
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    game => game.Id == request.VideoGameId,
                    cancellationToken);

            if (videoGame is null)
            {
                return new Result(null, "GameNotFound");
            }

            var alreadyExists = await context.UserGames
                .AnyAsync(
                    userGame =>
                        userGame.UserId == userId &&
                        userGame.VideoGameId == request.VideoGameId,
                    cancellationToken);

            if (alreadyExists)
            {
                return new Result(null, "AlreadyInLibrary");
            }

            var userGame = new UserGame
            {
                UserId = userId,
                VideoGameId = videoGame.Id,
                IsOwned = request.IsOwned,
                IsPlayed = request.IsPlayed,
                AddedAt = DateTime.UtcNow
            };

            context.UserGames.Add(userGame);

            await context.SaveChangesAsync(cancellationToken);

            var response = new Response(
                videoGame.Id,
                videoGame.Title,
                videoGame.Genre,
                videoGame.ReleaseYear,
                userGame.IsOwned,
                userGame.IsPlayed,
                userGame.AddedAt);

            return new Result(response, null);
        }
    }
}

[Authorize]
[ApiController]
[Route("api/me/library")]
public sealed class AddGameToLibraryController(ISender sender)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Response>> Add(
        Command command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Error switch
        {
            "Unauthorized" => Unauthorized(),

            "GameNotFound" => NotFound(new
            {
                message = "The specified video game does not exist."
            }),

            "AlreadyInLibrary" => Conflict(new
            {
                message = "This game is already in the library."
            }),

            _ => Created(
                $"/api/me/library/{result.Data!.VideoGameId}",
                result.Data)
        };
    }
}


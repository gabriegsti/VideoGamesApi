using MediatR;
using Microsoft.AspNetCore.Mvc;
using VideoGameApi.Data;
using VideoGameApi.Entities;
using static VideoGameApi.Features.VideoGames.CreateGame;

namespace VideoGameApi.Features.VideoGames
{
    public class CreateGame
    {
        public record Command(string Title, string Genre, int ReleaseYear) : IRequest<Response>;
        public record Response(int Id, string Title, string Genre, int ReleaseYear);
        public class Handler(VideoGameDBContext context) : IRequestHandler<Command, Response>
        {
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var videoGame = new VideoGame
                {
                    Title = request.Title,
                    Genre = request.Genre,
                    ReleaseYear = request.ReleaseYear
                };
                context.VideoGames.Add(videoGame);

                await context.SaveChangesAsync(cancellationToken);

                return new Response(videoGame.Id,
                    videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
            }
        }

    }

    [ApiController]
    [Route("api/games")]
    public class CreateGameController(ISender sender) : Controller
    {
        [HttpPost]
        public async Task<ActionResult<Response>> CreateGame(Command command, CancellationToken cancellationToken)
        {
            var response = await sender.Send(command, cancellationToken);
            return Created($"/api/games/{response.Id}", response);
        }
    }
}

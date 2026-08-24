using MediatR;
using Microsoft.AspNetCore.Mvc;
using VideoGameApi.Data;
using static VideoGameApi.Features.VideoGames.UpdateGame;

namespace VideoGameApi.Features.VideoGames
{
    public class UpdateGame
    {
        public record Command(int Id, string Title, string Genre, int ReleaseYear) : IRequest<Response>;
        public record Response(int Id, string Title, string Genre, int ReleaseYear);
        public class Handler(VideoGameDBContext context) : IRequestHandler<Command, Response>
        {
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var videoGame = await context.VideoGames.FindAsync(new object[] { request.Id }, cancellationToken);
                if (videoGame == null)
                {
                    return null;
                }
                videoGame.Title = request.Title;
                videoGame.Genre = request.Genre;
                videoGame.ReleaseYear = request.ReleaseYear;
                await context.SaveChangesAsync(cancellationToken);
                return new Response(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
            }
        }
    }

    [ApiController]
    [Route("api/games")]
    public class UpdateGameController(ISender sender) : Controller
    {
        [HttpPut("{id}")]
        public async Task<ActionResult<Response>> UpdateGame(int id, Command command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            var response = await sender.Send(command, cancellationToken);

            if(response == null)
            {
                return NotFound("Video game with given Id not found.");
            }

            return Ok(response);
        }
    }
}

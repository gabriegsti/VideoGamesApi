using MediatR;
using Microsoft.AspNetCore.Mvc;
using VideoGameApi.Data;
using static VideoGameApi.Features.VideoGames.DeleteGame;

namespace VideoGameApi.Features.VideoGames
{
    public class DeleteGame
    {
        public record Command(int id) : IRequest<bool>;
        public record Response(int id, string Title, string genre, int ReleaseYear);

        public class Handler(VideoGameDBContext context) : IRequestHandler<Command, bool>
        {
            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var videoGame = await context.VideoGames.FindAsync(new object[] { request.id }, cancellationToken);
                if (videoGame == null)
                {
                    return false;
                }
                context.VideoGames.Remove(videoGame);
                await context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }

    }

    [ApiController]
    [Route("api/games")]
    public class DeleteGameController(ISender sender) : Controller
    {
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGame(int id, CancellationToken cancellationToken)
        {
            var command = new Command(id);
            var result = await sender.Send(command, cancellationToken);
            if (!result)
            {
                return NotFound("Video game with given Id not found.");
            }
            return NoContent();
        }
    }
}

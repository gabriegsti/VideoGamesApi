using MediatR;
using VideoGameApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using static VideoGameApi.Features.VideoGames.GetAllGames;


namespace VideoGameApi.Features.VideoGames
{
    public class GetAllGames
    {
        public record Query : IRequest<IEnumerable<Response>>;

        public record Response(int Id, string Title, string Genre, int ReleaseYear);

        public class Handler(VideoGameDBContext context) : IRequestHandler<Query, IEnumerable<Response>>
        {
            public async Task<IEnumerable<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var videoGames = await context.VideoGames.ToListAsync(cancellationToken);

                return videoGames.Select(vg => new Response(vg.Id, vg.Title, vg.Genre, vg.ReleaseYear));
            }
        }
    }

    [ApiController]
    [Route("api/games")]
    public class GetAllGamesController(ISender sender) : Controller
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Response>>> GetAllGames(CancellationToken cancellationToken)
        {
           var response = await sender.Send(new GetAllGames.Query(), cancellationToken);
           return Ok(response);
        }
    }
}

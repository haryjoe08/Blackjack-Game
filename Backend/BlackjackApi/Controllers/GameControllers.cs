using BlackjackApi.Dtos;
using BlackjackApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackApi.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly GameSessionService _session;

        public GameController(GameSessionService session)
        {
            _session = session;
        }


        [HttpPost("new")]
        public ActionResult<NewGameResponseDto> NewGame([FromBody] NewGameRequest req) =>
            ToActionResult(_session.NewGame(req));


        [HttpGet("{gameId}/resume")]
        public ActionResult<GameStateDto> Resume(string gameId) =>
            ToActionResult(_session.ResumeSession(gameId));

        [HttpPost("{gameId}/bet")]
        public ActionResult<GameStateDto> PlaceBet(string gameId, [FromBody] BetRequest req) =>
            ToActionResult(_session.HandleBet(gameId, req));

        [HttpPost("{gameId}/deal")]
        public ActionResult<GameStateDto> Deal(string gameId) =>
            ToActionResult(_session.Deal(gameId));

        [HttpPost("{gameId}/action")]
        public ActionResult<GameStateDto> PerformAction(string gameId, [FromBody] ActionRequest req) =>
            ToActionResult(_session.HandleAction(gameId, req));

        private ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
        {
            if (result.NotFound) return NotFound(new { message = result.Error });
            if (!result.Success) return BadRequest(new { message = result.Error });
            return Ok(result.Data);
        }
    }
}
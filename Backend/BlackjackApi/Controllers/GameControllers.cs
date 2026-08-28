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
        
        
        /// POST /api/game/new
      
        [HttpPost("new")]
        public ActionResult<GameStateDto> NewGame([FromBody] NewGameRequest req)
        {
            try
            {
                GameStateDto state = _session.NewGame(req, out string gameId);

                return Ok(new { gameId, state });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

   
        /// GET /api/game/{gameId}/resume

        [HttpGet("{gameId}/resume")]
        public ActionResult<GameStateDto> Resume(string gameId)
        {
            try
            {
                GameStateDto state = _session.ResumeSession(gameId);
                return Ok(state);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

 
        /// POST /api/game/{gameId}/bet
        
        [HttpPost("{gameId}/bet")]
        public ActionResult<GameStateDto> PlaceBet(string gameId, [FromBody] BetRequest req)
        {
            try
            {
                GameStateDto state = _session.HandleBet(gameId, req);
                return Ok(state);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// POST /api/game/{gameId}/deal
        [HttpPost("{gameId}/deal")]
        public ActionResult<GameStateDto> Deal(string gameId)
        {
            try
            {
                GameStateDto state = _session.Deal(gameId);
                return Ok(state);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/game/{gameId}/action
        /// Performs a player action.
        /// </summary>
        [HttpPost("{gameId}/action")]
        public ActionResult<GameStateDto> PerformAction(string gameId, [FromBody] ActionRequest req)
        {
            try
            {
                GameStateDto state = _session.HandleAction(gameId, req);
                return Ok(state);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
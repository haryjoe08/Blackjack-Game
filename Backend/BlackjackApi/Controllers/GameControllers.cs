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

        /// <summary>
        /// Constructor injection of the game session service.
        /// </summary>
        public GameController(GameSessionService session)
        {
            _session = session;
        }

        /// <summary>
        /// POST /api/game/new
        /// 
        /// Starts a new Blackjack game session.
        /// </summary>
        [HttpPost("new")]
        public ActionResult<GameStateDto> NewGame(
            [FromBody] NewGameRequest req)
        {
            try
            {
                GameStateDto state = _session.NewGame(req);

                return Ok(state);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET /api/game/resume
        /// 
        /// Resumes the current Blackjack game session.
        /// </summary>
        [HttpGet("resume")]
        public ActionResult<GameStateDto> Resume()
        {
            try
            {
                GameStateDto state = _session.ResumeSession();

                return Ok(state);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// POST /api/game/bet
        /// 
        /// Places a bet for the current Blackjack game.
        /// </summary>
        [HttpPost("bet")]
        public ActionResult<GameStateDto> PlaceBet(
            [FromBody] BetRequest req)
        {
            try
            {
                GameStateDto state = _session.HandleBet(req);

                return Ok(state);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// POST /api/game/deal
        /// 
        /// Deals the initial cards and starts the Blackjack round.
        /// </summary>
        [HttpPost("deal")]
        public ActionResult<GameStateDto> Deal()
        {
            try
            {
                GameStateDto state = _session.Deal();

                return Ok(state);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// POST /api/game/action
        /// 
        /// Performs a player action such as Hit, Stand,
        /// Double, Split, or Insurance.
        /// </summary>
        [HttpPost("action")]
        public ActionResult<GameStateDto> PerformAction(
            [FromBody] ActionRequest req)
        {
            try
            {
                GameStateDto state = _session.HandleAction(req);

                return Ok(state);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
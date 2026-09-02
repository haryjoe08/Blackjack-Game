using BlackjackApi.Engine;
using BlackjackApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using BlackjackApi.Controllers;
using BlackjackApi.Dtos;
using BlackjackApi.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackApi.Tests
{
    class GameControllerTest
    {
        private GameController _controller;
        private GameSessionService _service;
        private GameEngine _engine;
        private Mock<ILogger<GameSessionService>> _logger;

        [SetUp]
        public void SetUp()
        {
            _engine = new GameEngine();
            _logger = new Mock<ILogger<GameSessionService>>();

            _service = new GameSessionService(
                _engine,
                _logger.Object);

            _controller = new GameController(_service);
        }

        #region NewGame

        [Test]
        public void New_ValidRequest_ReturnsOk()
        {
            var req = new NewGameRequest(
                "Player 1",
                1000,
                10,
                100);

            var result = _controller.NewGame(req);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public void NewGame_InvalidRequest_ReturnsBadRequest()
        {
            var req = new NewGameRequest(
                "",
                1000,
                10,
                100);

            var result = _controller.NewGame(req);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        #endregion

        #region PlaceBet
        [Test]
        public void PlaceBet_ValidBet_ReturnsOk()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            var req = new BetRequest(50);

            var result = _controller.PlaceBet(gameId, req);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public void PlaceBet_InvalidBet_ReturnsBadRequest()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            var req = new BetRequest(5);

            var result = _controller.PlaceBet(gameId, req);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public void PlaceBet_InvalidGameId_ReturnsNotFound()
        {
            var req = new BetRequest(50);

            var result = _controller.PlaceBet(
                "invalid-game-id",
                req);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        #endregion

        #region  Deal

        [Test]
        public void Deal_ValidGameWithBet_ReturnsOk()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));

            var result = _controller.Deal(gameId);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public void Deal_InvalidGameId_ReturnsNotFound()
        {
            var result = _controller.Deal("invalid-game-id");

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }
        #endregion

        #region PerformAction

        [Test]
        public void PerformAction_ValidAction_ReturnsOk()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);

            var req = new ActionRequest(ActionType.Stand, 0);

            var result = _controller.PerformAction(gameId, req);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public void PerformAction_InvalidGameId_ReturnsNotFound()
        {
            var req = new ActionRequest(ActionType.Stand, 0);

            var result = _controller.PerformAction(
                "invalid-game-id",
                req);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public void PerformAction_InvalidHandIndex_ReturnsBadRequest()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            var req = new ActionRequest(ActionType.Stand, 99);

            var result = _controller.PerformAction(gameId, req);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
        #endregion

    }
}
using BlackjackApi.Engine;
using BlackjackApi.Dtos;
using BlackjackApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using BlackjackApi.Models.Enums;
using BlackjackApi.Models;


namespace BlackjackApi.Tests
{
    public class GameServiceTest
    {
        private GameSessionService _service;
        private GameEngine _engine;
        private Mock<ILogger<GameSessionService>> _logger;

        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine();
            _logger = new Mock<ILogger<GameSessionService>>();

            _service = new GameSessionService(
                _engine,
                _logger.Object);
        }

        #region NewGame

        [TestCase("")]
        [TestCase(" ")]
        public void NewGame_InvalidName_ReturnsFailure(string name)
        {
            var req = new NewGameRequest(
                name,
                1000,
                10,
                100);

            var result = _service.NewGame(req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Player name cannot be empty."));
        }

        [Test]
        public void NewGame_InvalidMaxBet_ReturnsFailure()
        {
            var req = new NewGameRequest("hary", 1000, 1000, 100);
            var result = _service.NewGame(req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Maximum bet must be greater than or equal to the minimum bet."));
        }

        [Test]
        public void NewGame_InvalidStartingBalance_ReturnsFailure()
        {
            var req = new NewGameRequest("hary", 0, 1000, 100);
            var result = _service.NewGame(req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Starting balance must be greater than 0."));
        }

        [Test]
        public void NewGame_InvalidMinBet_ReturnsFailure()
        {
            var req = new NewGameRequest("hary", 1000, 0, 100);
            var result = _service.NewGame(req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Minimum bet must be greater than 0."));
        }

        [Test]
        public void NewGame_ValidRequest_ReturnsSuccess()
        {
            var req = new NewGameRequest(
                "hary",
                1000,
                10,
                100);

            var result = _service.NewGame(req);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        }

        #endregion

        #region ResumeGame

        [Test]
        public void ResumeSession_InvalidGameId_ReturnsNotFound()
        {
            var result = _service.ResumeSession("invalid-game-id");

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.True);
            Assert.That(result.Error, Is.EqualTo(
                "Game session not found or has ended."));
        }


        [Test]
        public void ResumeSession_ExistingSessionWithoutActiveRound_ReturnsSuccess()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var result = _service.ResumeSession(gameId);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.LastMessage, Is.EqualTo(
                "Session found. Please place a bet."));
        }

        #endregion

        #region HandleBet

        [Test]
        public void HandleBet_InvalidGameId_ReturnsNotFound()
        {
            var req = new BetRequest(50);

            var result = _service.HandleBet(
                "invalid-game-id",
                req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.True);
            Assert.That(result.Error, Is.EqualTo(
                "Game session not found or has ended."));
        }

        [Test]
        public void HandleBet_InvalidBet_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var req = new BetRequest(5);

            var result = _service.HandleBet(gameId, req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Invalid bet (outside the min/max limit or insufficient balance)."));
        }

        [Test]
        public void HandleBet_ValidBet_ReturnsSuccess()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var req = new BetRequest(50);

            var result = _service.HandleBet(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        }

        #endregion

        #region Deal

        [Test]
        public void Deal_InvalidGameId_ReturnsNotFound()
        {
            var result = _service.Deal("invalid-game-id");

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.True);
            Assert.That(result.Error, Is.EqualTo(
                "Game session not found or has ended."));
        }

        [Test]
        public void Deal_WithoutBet_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var result = _service.Deal(gameId);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Please place a bet before dealing."));
        }

        [Test]
        public void Deal_WithValidBet_ReturnsSuccess()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(
                gameId,
                new BetRequest(50));

            var result = _service.Deal(gameId);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.LastMessage, Is.EqualTo(
                "Cards dealt."));
        }

        #endregion

        #region HandleAction

        [Test]
        public void HandleAction_InvalidGameId_ReturnsNotFound()
        {
            var req = new ActionRequest(
                ActionType.Hit, 0);

            var result = _service.HandleAction(
                "invalid-game-id",
                req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.True);
            Assert.That(result.Error, Is.EqualTo(
                "Game session not found or has ended."));
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void HandleAction_InvalidHandIndex_ReturnsFailure(
            int handIndex)
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var req = new ActionRequest(
                ActionType.Hit,
                handIndex);

            var result = _service.HandleAction(
                gameId,
                req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Invalid hand index."));
        }

        [Test]
        public void HandleAction_NegativeHandIndex_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            var req = new ActionRequest(
                ActionType.Hit, -1);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "Invalid hand index."));
        }

        [Test]
        public void HandleAction_InactiveHand_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest(
                    "Player 1",
                    1000,
                    10,
                    100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(
                gameId,
                new BetRequest(50));

            _service.Deal(gameId);
        }

        [Test]
        public void HandleAction_WrongHandIndexx_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            var session = _service.GetSession(gameId)!;

            session.Player.Hands.Add(new Hand());

            var req = new ActionRequest(ActionType.Hit, 1);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "It is not this hand's turn."));
        }
        [Test]
        public void HandleAction_WrongHandIndex_ReturnsFailure()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            var session = _service.GetSession(gameId)!;

            session.Player.Hands.Add(new Hand());
            session.ActiveHandIndex = 0;

            var req = new ActionRequest(ActionType.Hit, 1);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.False);
            Assert.That(result.NotFound, Is.False);
            Assert.That(result.Error, Is.EqualTo(
                "It is not this hand's turn."));
        }
        
        [Test]
        public void HandleAction_FinishedHand_ReturnsSuccessWithErrorMessage()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);

            var session = _service.GetSession(gameId)!;
            session.Player.Hands[0].IsFinished = true;

            var req = new ActionRequest(ActionType.Hit, 0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        }
        
        [Test]
        public void HandleAction_FinishedHand_MovesToNextUnfinishedHand()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);

            var session = _service.GetSession(gameId)!;

            session.Player.Hands.Add(new Hand());
            session.ActiveHandIndex = 0;

            var req = new ActionRequest(ActionType.Stand, 0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(session.ActiveHandIndex, Is.EqualTo(1));
        }

        
        [Test]
        public void HandleAction_AllHandsBusted_RevealsDealerHand()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);

            var session = _service.GetSession(gameId)!;

            var hand = session.Player.Hands[0];

            hand.Cards.Clear();
            hand.Cards.Add(new Card(Rank.King, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Ten, Suit.Diamond));
            hand.Cards.Add(new Card(Rank.Five, Suit.Clubs));

            var req = new ActionRequest(ActionType.Stand, 0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        }


        [Test]
        public void HandleAction_FinishedHandNotBusted_PlaysDealerTurn()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);
            
            var session = _service.GetSession(gameId)!;

            var hand = session.Player.Hands[0];

            hand.Cards.Clear();
            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Diamond));

            var req = new ActionRequest(ActionType.Stand, 0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(session.Player.CurrentBet, Is.EqualTo(0));
            Assert.That(result.Data, Is.Not.Null);
        }

        [Test]
        public void HandleAction_DoubleDownBust_RevealsDealerHand()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));
            _service.Deal(gameId);
            
            var session = _service.GetSession(gameId)!;

            var hand = session.Player.Hands[0];

            hand.Cards.Clear();
            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Ten, Suit.Diamond));

            session.Deck.Cards.Push(
                new Card(Rank.Five, Suit.Clubs));

            var req = new ActionRequest(
                ActionType.DoubleDown,
                0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(hand.IsFinished, Is.True);
            Assert.That(
                session.Player.CurrentBet,
                Is.EqualTo(0));
        }

        [Test]
        public void HandleAction_Split_ReturnsSuccess()
        {
            var newGame = _service.NewGame(
                new NewGameRequest("Player 1", 1000, 10, 100));

            var gameId = newGame.Data!.GameId;

            _service.HandleBet(gameId, new BetRequest(50));

            var session = _service.GetSession(gameId)!;

            var hand = session.Player.Hands[0];

            hand.Cards.Clear();
            hand.Cards.Add(new Card(Rank.Eight, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Eight, Suit.Diamond));

            var req = new ActionRequest(ActionType.Split, 0);

            var result = _service.HandleAction(gameId, req);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(session.Player.Hands.Count, Is.EqualTo(2));
        }

        #endregion
    }
}
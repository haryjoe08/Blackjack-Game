using System.Collections.Concurrent;
using BlackjackApi.Dtos;
using BlackjackApi.Engine;
using BlackjackApi.Models;
using BlackjackApi.Models.Enums;

namespace BlackjackApi.Services
{
    public class GameSessionService
    {
        private readonly GameEngine _engine;
        private readonly ConcurrentDictionary<string, GameSessionData> _sessions = new();
        public event Action<string>? OnGameLog;
        private readonly ILogger<GameSessionService> _logger;

        public GameSessionService(GameEngine engine, ILogger<GameSessionService> logger)
        {
            _engine = engine;
            OnGameLog += (message) => Console.WriteLine($"[LOG GAME] {message}");
            _logger = logger;
        }

        public ServiceResult<NewGameResponseDto> NewGame(NewGameRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                _logger.LogWarning("NewGame rejected: player name was empty.");
                return ServiceResult<NewGameResponseDto>.Fail(
                    "Player name cannot be empty.");
            }

            if (req.StartingBalance <= 0)
            {
                _logger.LogWarning(
                    "NewGame rejected for {PlayerName}: invalid starting balance {StartingBalance}.",
                    req.Name, req.StartingBalance);
                return ServiceResult<NewGameResponseDto>.Fail(
                    "Starting balance must be greater than 0.");
            }

            if (req.MinBet <= 0)
            {
                _logger.LogWarning(
                    "NewGame rejected for {PlayerName}: invalid min bet {MinBet}.",
                    req.Name, req.MinBet);
                return ServiceResult<NewGameResponseDto>.Fail(
                    "Minimum bet must be greater than 0.");
            }

            if (req.MaxBet < req.MinBet)
            {
                _logger.LogWarning(
                    "NewGame rejected for {PlayerName}: MaxBet {MaxBet} < MinBet {MinBet}.",
                    req.Name, req.MaxBet, req.MinBet);
                return ServiceResult<NewGameResponseDto>.Fail(
                    "Maximum bet must be greater than or equal to the minimum bet.");
            }

            var player = new Player(req.Name, req.StartingBalance);
            var table = new Table(1, req.MinBet, req.MaxBet);
            var deck = _engine.CreateStandardDeck();
            var dealer = new Dealer();

            var session = new GameSessionData(player, table, deck, dealer)
            {
                LastMessage = "New game started.",
                ActiveHandIndex = 0
            };

            _sessions[session.GameId] = session;

            Log($"New session created for [{req.Name}] | GameId: {session.GameId}");
            _logger.LogInformation(
               "New session created for player {PlayerName}. GameId: {GameId}",
               req.Name,
               session.GameId);

            return ServiceResult<NewGameResponseDto>.Ok(
                new NewGameResponseDto(session.GameId, BuildState(session)));
        }

        public ServiceResult<GameStateDto> ResumeSession(string gameId)
        {
            var session = GetSession(gameId);

            if (session == null)
            {
                _logger.LogWarning(
                    "ResumeSession failed: GameId {GameId} not found or has ended.",
                    gameId);
                return ServiceResult<GameStateDto>.NotFoundResult(
                    "Game session not found or has ended.");
            }

            lock (session)
            {
                bool hasActiveRound =
                    _engine.ResumeGame(session.Player, session.Dealer);

                session.LastMessage = hasActiveRound
                    ? "Game resumed from the previous round."
                    : "Session found. Please place a bet.";

                Log(
                    $"Session resumed for [{session.Player.Name}] | GameId: {gameId} | ActiveRound: {hasActiveRound}");
                _logger.LogInformation(
                    "Session resumed for player {PlayerName} on GameId {GameId}. HasActiveRound: {HasActiveRound}",
                    session.Player.Name,
                    gameId,
                    hasActiveRound);

                return ServiceResult<GameStateDto>.Ok(BuildState(session));
            }
        }

        public ServiceResult<GameStateDto> HandleBet(
            string gameId,
            BetRequest req)
        {
            var session = GetSession(gameId);

            if (session == null)
            {
                _logger.LogWarning(
                    "HandleBet failed: GameId {GameId} not found or has ended.",
                    gameId);
                return ServiceResult<GameStateDto>.NotFoundResult(
                    "Game session not found or has ended.");
            }

            lock (session)
            {
                bool accepted =
                    _engine.PlaceBet(
                        session.Player,
                        session.Table,
                        req.Amount);

                if (!accepted)
                {
                    _logger.LogWarning(
                        "Bet rejected for player {PlayerName} on GameId {GameId}. Amount: {BetAmount}, Balance: {Balance}, Limits: [{MinBet}-{MaxBet}]",
                        session.Player.Name,
                        gameId,
                        req.Amount,
                        session.Player.Balance,
                        session.Table.MinBet,
                        session.Table.MaxBet);
                    return ServiceResult<GameStateDto>.Fail(
                        "Invalid bet (outside the min/max limit or insufficient balance).");
                }

                session.LastMessage =
                    $"Bet of {req.Amount} chips accepted.";

                Log(
                    $"Player [{session.Player.Name}] placed a bet: {req.Amount} chips.");
                _logger.LogInformation(
                    "Player {PlayerName} placed a bet of {BetAmount} on GameId {GameId}. Remaining Balance: {Balance}",
                    session.Player.Name,
                    req.Amount,
                    gameId,
                    session.Player.Balance
                );

                return ServiceResult<GameStateDto>.Ok(BuildState(session));
            }
        }

        public ServiceResult<GameStateDto> Deal(string gameId)
        {
            var session = GetSession(gameId);

            if (session == null)
            {
                _logger.LogWarning(
                    "Deal failed: GameId {GameId} not found or has ended.",
                    gameId);
                return ServiceResult<GameStateDto>.NotFoundResult(
                    "Game session not found or has ended.");
            }

            lock (session)
            {
                if (session.Player.CurrentBet <= 0)
                {
                    _logger.LogWarning(
                        "Deal rejected for player {PlayerName} on GameId {GameId}: no bet placed.",
                        session.Player.Name,
                        gameId);
                    return ServiceResult<GameStateDto>.Fail(
                        "Please place a bet before dealing.");
                }

                if (_engine.RemainingCard(session.Deck) < 15)
                {
                    session.Deck = _engine.CreateStandardDeck();
                    _logger.LogInformation(
                        "Deck reshuffled for GameId {GameId} (low remaining cards).",
                        gameId);
                }

                session.Dealer = new Dealer();

                _engine.StartGame(
                    session.Player,
                    session.Deck,
                    session.Dealer);

                session.LastMessage = "Cards dealt.";
                session.ActiveHandIndex = 0;

                Log(
                    $"Cards dealt for [{session.Player.Name}] | GameId: {gameId} | Bet: {session.Player.CurrentBet}");
                _logger.LogInformation(
                    "Cards dealt for player {PlayerName} on GameId {GameId}. Bet: {BetAmount}",
                    session.Player.Name,
                    gameId,
                    session.Player.CurrentBet);

                if (_engine.IsHandBlackjack(session.Player.Hands[0]))
                {
                    _engine.RevealDealerHand(session.Dealer);

                    session.LastMessage =
                        _engine.EvaluateWinner(
                            session.Player,
                            session.Dealer);

                    Log(
                        $"Blackjack! Player [{session.Player.Name}] | GameId: {gameId} | Result: {session.LastMessage}");
                    _logger.LogInformation(
                        "Player {PlayerName} got a natural blackjack on GameId {GameId}. Result: {Result}",
                        session.Player.Name,
                        gameId,
                        session.LastMessage);

                    session.Player.CurrentBet = 0;
                }
                

                return ServiceResult<GameStateDto>.Ok(BuildState(session));
            }
        }

        public ServiceResult<GameStateDto> HandleAction(
            string gameId,
            ActionRequest req)
        {
            var session = GetSession(gameId);

            if (session == null)
            {
                _logger.LogWarning(
                    "HandleAction failed: GameId {GameId} not found or has ended.",
                    gameId);
                return ServiceResult<GameStateDto>.NotFoundResult(
                    "Game session not found or has ended.");
            }

            lock (session)
            {
                if (req.HandIndex < 0 ||
                    req.HandIndex >= session.Player.Hands.Count)
                {
                    _logger.LogWarning(
                        "HandleAction rejected for GameId {GameId}: invalid hand index {HandIndex}.",
                        gameId,
                        req.HandIndex);
                    return ServiceResult<GameStateDto>.Fail(
                        "Invalid hand index.");
                }

                if (req.HandIndex != session.ActiveHandIndex)
                {
                    _logger.LogWarning(
                        "HandleAction rejected for GameId {GameId}: hand {HandIndex} is not active (active is {ActiveHandIndex}).",
                        gameId,
                        req.HandIndex,
                        session.ActiveHandIndex);
                    return ServiceResult<GameStateDto>.Fail(
                        "It is not this hand's turn.");
                }

                Hand hand = session.Player.Hands[req.HandIndex];

                var result =
                    _engine.PerformAction(
                        session.Player,
                        req.Action,
                        session.Deck,
                        hand,
                        session.Dealer);

                session.LastMessage = result.Message;

                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Action {Action} failed for player {PlayerName} on GameId {GameId}, hand {HandIndex}: {Message}",
                        req.Action,
                        session.Player.Name,
                        gameId,
                        req.HandIndex,
                        result.Message);
                    return ServiceResult<GameStateDto>.Ok(
                        BuildState(session));
                }

                if (req.Action == ActionType.Split)
                {
                    _logger.LogInformation(
                        "Player {PlayerName} split hand {HandIndex} on GameId {GameId}.",
                        session.Player.Name,
                        req.HandIndex,
                        gameId);
                    return ServiceResult<GameStateDto>.Ok(
                        BuildState(session));
                }

                if (req.Action == ActionType.Insurance &&
                    !_engine.IsHandFinished(hand))
                {
                    _logger.LogInformation(
                        "Player {PlayerName} took insurance on GameId {GameId}.",
                        session.Player.Name,
                        gameId);
                    return ServiceResult<GameStateDto>.Ok(
                        BuildState(session));
                }

                if (_engine.IsHandFinished(hand))
                {
                    int nextIndex =
                        FindNextUnfinishedHandIndex(
                            session.Player,
                            session.ActiveHandIndex);

                    if (nextIndex != -1)
                    {
                        session.ActiveHandIndex = nextIndex;
                        _logger.LogInformation(
                            "Hand {HandIndex} finished for GameId {GameId}; moving to next hand {NextIndex}.",
                            req.HandIndex,
                            gameId,
                            nextIndex);
                    }
                    else
                    {
                        bool allBusted =
                            session.Player.Hands.All(
                                _engine.IsHandBusted);

                        if (allBusted)
                        {
                            _engine.RevealDealerHand(
                                session.Dealer);
                            _logger.LogInformation(
                                "All hands busted for player {PlayerName} on GameId {GameId}; revealing dealer hand.",
                                session.Player.Name,
                                gameId);
                        }
                        else
                        {
                            _engine.PlayDealerTurn(
                                session.Dealer,
                                session.Deck);
                            _logger.LogInformation(
                                "Dealer turn played for GameId {GameId}.",
                                gameId);
                        }

                        var roundMessage =
                            _engine.EvaluateWinner(
                                session.Player,
                                session.Dealer);

                        session.LastMessage =
                            req.Action == ActionType.Insurance
                                ? $"{result.Message} | {roundMessage}"
                                : roundMessage;

                        session.Player.CurrentBet = 0;

                        _logger.LogInformation(
                            "Round finished for player {PlayerName} on GameId {GameId}. Result: {Result}",
                            session.Player.Name,
                            gameId,
                            session.LastMessage);
                    }
                }

                Log(
                    $"Player [{session.Player.Name}] performed action: {req.Action} -> {result.Message}");
                _logger.LogInformation(
                    "Player {PlayerName} performed action {Action} on GameId {GameId}, hand {HandIndex}. Message: {Message}",
                    session.Player.Name,
                    req.Action,
                    gameId,
                    req.HandIndex,
                    result.Message);

                return ServiceResult<GameStateDto>.Ok(
                    BuildState(session));
            }
        }

        public GameSessionData? GetSession(string gameId)
        {
            _sessions.TryGetValue(gameId, out var session);
            return session;
        }

        private int FindNextUnfinishedHandIndex(
            Player player,
            int afterIndex)
        {
            for (int i = afterIndex + 1;
                 i < player.Hands.Count;
                 i++)
            {
                if (!_engine.IsHandFinished(
                        player.Hands[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Log(string message)
        {
            OnGameLog?.Invoke(message);
        }

        private GameStateDto BuildState(
            GameSessionData session)
        {
            bool isGameOver =
                _engine.GetTotalBalance(session.Player) <
                session.Table.MinBet;

            return new GameStateDto(
                session.Player.PlayerId,
                session.Player.Name,
                _engine.GetTotalBalance(session.Player),
                session.Player.CurrentBet,
                session.Player.Hands
                    .Select(h => HandDto.From(_engine, h))
                    .ToList(),
                session.ActiveHandIndex,
                DealerDto.From(
                    _engine,
                    session.Dealer),
                _engine.RemainingCard(session.Deck),
                session.Table.MinBet,
                session.Table.MaxBet,
                isGameOver,
                CanOfferInsurance(session),
                session.LastMessage);
        }

        private bool CanOfferInsurance(
            GameSessionData session)
        {
            if (session.ActiveHandIndex < 0 ||
                session.ActiveHandIndex >=
                session.Player.Hands.Count)
            {
                return false;
            }

            var hand =
                session.Player.Hands[session.ActiveHandIndex];

            if (hand.Cards.Count != 2 ||
                hand.IsFinished ||
                hand.InsuranceTaken)
            {
                return false;
            }

            return session.Dealer.Hand.Cards.Count > 0 &&
                   session.Dealer.Hand.Cards[0].Rank == Rank.Ace;
        }
    }
}
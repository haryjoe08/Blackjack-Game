using System.Collections.Concurrent;
using BlackjackApi.Dtos;
using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Services
{
    public class GameSessionData
    {
        public string GameId { get; set; } = Guid.NewGuid().ToString();
        public Player Player { get; set; }
        public Table Table { get; set; }
        public Deck Deck { get; set; }
        public Dealer Dealer { get; set; }
        public string LastMessage { get; set; } = "Game baru dimulai.";
        public int ActiveHandIndex { get; set; } = 0;

        public GameSessionData(Player player, Table table, Deck deck, Dealer dealer)
        {
            Player = player;
            Table = table;
            Deck = deck;
            Dealer = dealer;
        }
    }

    public class GameSessionService
    {
        private readonly GameEngine _engine;
        private readonly ConcurrentDictionary<string, GameSessionData> _sessions = new();
        public event Action<string>? OnGameLog;

        public GameSessionService(GameEngine engine)
        {
            _engine = engine;
            OnGameLog += (message) => Console.WriteLine($"[LOG GAME] {message}");
        }

        public GameStateDto NewGame(NewGameRequest req, out string gameId)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                throw new ArgumentException("Nama pemain tidak boleh kosong.");
            }

            if (req.StartingBalance <= 0)
            {
                throw new ArgumentException("Saldo awal harus lebih dari 0.");
            }

            if (req.MinBet <= 0)
            {
                throw new ArgumentException("Min bet harus lebih dari 0.");
            }

            if (req.MaxBet < req.MinBet)
            {
                throw new ArgumentException("Max bet harus lebih besar atau sama dengan min bet.");
            }

            var player = new Player(req.Name, req.StartingBalance);
            var table = new Table(1, req.MinBet, req.MaxBet);
            var deck = _engine.CreateStandardDeck();
            var dealer = new Dealer();

            var session = new GameSessionData(player, table, deck, dealer)
            {
                LastMessage = "Game baru dimulai.",
                ActiveHandIndex = 0
            };

            _sessions[session.GameId] = session;
            gameId = session.GameId;

            Log($"Session baru dibuat untuk [{req.Name}] | GameId: {gameId}");
            return BuildState(session);
        }

        public GameStateDto ResumeSession(string gameId)
        {
            var session = GetSession(gameId);

            lock (session)
            {
                bool hasActiveRound = _engine.ResumeGame(session.Player, session.Dealer);

                session.LastMessage = hasActiveRound
                    ? "Game dilanjutkan dari ronde sebelumnya."
                    : "Sesi ditemukan, silakan pasang taruhan.";

                return BuildState(session);
            }
        }

        public GameStateDto HandleBet(string gameId, BetRequest req)
        {
            var session = GetSession(gameId);

            lock (session)
            {
                bool accepted = _engine.PlaceBet(session.Player, session.Table, req.Amount);

                if (!accepted)
                {
                    throw new ArgumentException("Taruhan tidak valid (di luar min/max, atau saldo kurang).");
                }

                session.LastMessage = $"Taruhan {req.Amount} chip diterima.";
                
                Log($"Player [{session.Player.Name}] memasang bet: {req.Amount} chip.");
                return BuildState(session);
            }
        }

        public GameStateDto Deal(string gameId)
        {
            var session = GetSession(gameId);

            lock (session)
            {
                if (session.Player.CurrentBet <= 0)
                {
                    throw new InvalidOperationException("Pasang taruhan dulu sebelum deal.");
                }

                if (_engine.RemainingCard(session.Deck) < 15)
                {
                    session.Deck = _engine.CreateStandardDeck();
                }

                session.Dealer = new Dealer();

                _engine.StartGame(session.Player, session.Deck, session.Dealer);

                session.LastMessage = "Kartu dibagikan.";
                session.ActiveHandIndex = 0;

                if (_engine.IsHandBlackjack(session.Player.Hands[0]))
                {
                    _engine.RevealDealerHand(session.Dealer);

                    session.LastMessage = _engine.EvaluateWinner(session.Player, session.Table, session.Dealer);
                    session.Player.CurrentBet = 0;
                }

                Log($"Kartu dibagikan untuk Player [{session.Player.Name}].");
                return BuildState(session);
            }
        }

        public GameStateDto HandleAction(string gameId, ActionRequest req)
        {
            var session = GetSession(gameId);

            lock (session)
            {
                if (req.HandIndex < 0 || req.HandIndex >= session.Player.Hands.Count)
                {
                    throw new ArgumentException("Index tangan tidak valid.");
                }

                if (req.HandIndex != session.ActiveHandIndex)
                {
                    throw new InvalidOperationException("Bukan giliran tangan ini.");
                }

                Hand hand = session.Player.Hands[req.HandIndex];

                var result = _engine.PerformAction(session.Player, req.Action, session.Deck, hand, session.Dealer);

                session.LastMessage = result.Message;

                if (!result.Success)
                {
                    return BuildState(session);
                }

                if (req.Action == ActionType.Split)
                {
                    return BuildState(session); 
                }

                if (req.Action == ActionType.Insurance && !_engine.IsHandFinished(hand))
                {
                    return BuildState(session);
                }

                if (_engine.IsHandFinished(hand))
                {
                    int nextIndex = FindNextUnfinishedHandIndex(session.Player, session.ActiveHandIndex);

                    if (nextIndex != -1)
                    {
                        session.ActiveHandIndex = nextIndex;
                    }
                    else
                    {
                        bool allBusted = session.Player.Hands.All(_engine.IsHandBusted);

                        if (allBusted)
                        {
                            _engine.RevealDealerHand(session.Dealer);
                        }
                        else
                        {
                            _engine.PlayDealerTurn(session.Dealer, session.Deck);
                        }

                        session.LastMessage = _engine.EvaluateWinner(session.Player, session.Table, session.Dealer);
                        session.Player.CurrentBet = 0;
                    }
                }

                Log($"Player [{session.Player.Name}] melalukan aksi: {req.Action} -> {result.Message}"); 
                return BuildState(session);
            }
        }

        private GameSessionData GetSession(string gameId)
        {
            if (!_sessions.TryGetValue(gameId, out var session))
            {
                throw new InvalidOperationException("Session game tidak ditemukan atau telah berakhir.");
            }

            return session;
        }

        private int FindNextUnfinishedHandIndex(Player p, int afterIndex)
        {
            for (int i = afterIndex + 1; i < p.Hands.Count; i++)
            {
                if (!_engine.IsHandFinished(p.Hands[i]))
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

        private GameStateDto BuildState(GameSessionData session)
        {
            bool isGameOver = _engine.GetTotalBalance(session.Player) < session.Table.MinBet;

            return new GameStateDto(
                session.Player.PlayerId,
                session.Player.Name,
                _engine.GetTotalBalance(session.Player),
                session.Player.CurrentBet,
                session.Player.Hands.Select(h => HandDto.From(_engine, h)).ToList(),
                session.ActiveHandIndex,
                DealerDto.From(_engine, session.Dealer),
                _engine.RemainingCard(session.Deck),
                session.Table.MinBet,
                session.Table.MaxBet,
                isGameOver,
                CanOfferInsurance(session),
                session.LastMessage);
        }

        private bool CanOfferInsurance(GameSessionData session)
        {
            if (session.ActiveHandIndex < 0 || session.ActiveHandIndex >= session.Player.Hands.Count) return false;

            var hand = session.Player.Hands[session.ActiveHandIndex];
            if (hand.Cards.Count != 2 || hand.IsFinished || hand.InsuranceTaken) return false;

            return session.Dealer.Hand.Cards.Count > 0 && session.Dealer.Hand.Cards[0].Rank == Rank.Ace;
        }
    }
}
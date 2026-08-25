using BlackjackApi.Dtos;
using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Services
{
    /// <summary>
    /// Owns EVERY piece of session state now that GameEngine is stateless
    /// (Option B) - current player/table/deck/dealer, whose hand is active,
    /// the last message, and even the diagram-native GameEngine fields
    /// (Turn, _cardInHands, _players, OnCheckCardPlayer/Dealer) that used to
    /// live on GameEngine itself. Those last few were never actually read
    /// anywhere beyond basic bookkeeping even before this refactor - they're
    /// kept here unchanged (not deleted) purely so nothing behaves
    /// differently, just relocated since a stateless GameEngine has nowhere
    /// left to put them.
    ///
    /// Also the orchestrator: decides the order GameEngine's (now fully
    /// parameterized) methods get called in for a full round.
    /// </summary>
    public class GameSessionService
    {
        // ---------------------------------------------------------------
        // Demo-scope simplification: one in-memory session shared by every
        // request (no auth, no per-user sessions).
        // ---------------------------------------------------------------
        private readonly GameEngine _engine = new();
        private Player? _player;
        private Table? _table;
        private Deck? _deck;
        private Dealer _dealer = new();
        private string? _lastMessage;
        private int _activeHandIndex;

        // Diagram-native GameEngine fields, relocated here now that
        // GameEngine can't hold any state at all - see class note above.
        private readonly List<Player> _players = new();
        private readonly Dictionary<Player, List<Hand>> _cardInHands = new();
        public int Turn { get; private set; }
        public Action<int, ChipType>? OnCheckCardPlayer;
        public Action<int, ChipType>? OnCheckCardDealer;

        public void NextTurn(Player p) => Turn++;
        public bool HasPlayer() => _players.Count > 0;

        public GameStateDto NewGame(NewGameRequest req)
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

            _players.Clear();
            _cardInHands.Clear();
            Turn = 0;

            _player = new Player(req.Name, req.StartingBalance);
            _table = new Table(1, req.MinBet, req.MaxBet);
            _deck = _engine.CreateStandardDeck();
            _dealer = new Dealer();
            _lastMessage = "Game baru dimulai.";
            _activeHandIndex = 0;

            _players.Add(_player);
            _cardInHands[_player] = _player.Hands;

            return BuildState();
        }

        public GameStateDto ResumeSession()
        {
            if (_player == null || _table == null || _deck == null)
            {
                throw new InvalidOperationException(
                    "Belum ada game yang bisa di-resume.");
            }

            bool hasActiveRound = _engine.ResumeGame(
                _player,
                _table);

            _lastMessage = hasActiveRound
                ? "Game dilanjutkan dari ronde sebelumnya."
                : "Sesi ditemukan, silakan pasang taruhan.";

            return BuildState();
        }

        public GameStateDto HandleBet(BetRequest req)
        {
            if (_player == null || _table == null || _deck == null)
            {
                throw new InvalidOperationException(
                    "Panggil NewGame dulu.");
            }

            bool accepted = _engine.PlaceBet(
                _player,
                _table,
                req.Amount);

            if (!accepted)
            {
                throw new ArgumentException(
                    "Taruhan tidak valid (di luar min/max, atau saldo kurang).");
            }

            _lastMessage = $"Taruhan {req.Amount} chip diterima.";

            return BuildState();
        }

        public GameStateDto Deal()
        {
            if (_player == null || _table == null || _deck == null)
            {
                throw new InvalidOperationException(
                    "Panggil NewGame dulu.");
            }

            if (_player.CurrentBet <= 0)
            {
                throw new InvalidOperationException(
                    "Pasang taruhan dulu sebelum deal.");
            }

            if (_engine.RemainingCard(_deck) < 15)
            {
                _deck = _engine.CreateStandardDeck();
            }

            _dealer = new Dealer();

            _engine.StartGame(
                _player,
                _deck,
                _table,
                _dealer);

            _cardInHands[_player] = _player.Hands;
            _lastMessage = "Kartu dibagikan.";
            _activeHandIndex = 0;

            if (_engine.IsHandBlackjack(_player.Hands[0]))
            {
                _engine.RevealDealerHand(_dealer);

                _lastMessage = _engine.EvaluateWinner(
                    _player,
                    _table,
                    _dealer);
            }

            return BuildState();
        }

        public GameStateDto HandleAction(ActionRequest req)
        {
            if (_player == null || _table == null || _deck == null)
            {
                throw new InvalidOperationException(
                    "Panggil NewGame dulu.");
            }

            if (req.HandIndex < 0 ||
                req.HandIndex >= _player.Hands.Count)
            {
                throw new ArgumentException(
                    "Index tangan tidak valid.");
            }

            if (req.HandIndex != _activeHandIndex)
            {
                throw new InvalidOperationException(
                    "Bukan giliran tangan ini.");
            }

            Hand hand = _player.Hands[req.HandIndex];

            var result = _engine.PerformAction(
                _player,
                req.Action,
                _deck,
                hand,
                _dealer);

            _lastMessage = result.Message;

            if (req.Action == ActionType.Split)
            {
                return BuildState();
            }

            if (_engine.IsHandFinished(hand))
            {
                int nextIndex = FindNextUnfinishedHandIndex(
                    _player,
                    _activeHandIndex);

                if (nextIndex != -1)
                {
                    _activeHandIndex = nextIndex;
                }
                else
                {
                    bool allBusted = _player.Hands.All(
                        _engine.IsHandBusted);

                    if (allBusted)
                    {
                        _engine.RevealDealerHand(_dealer);
                    }
                    else
                    {
                        _engine.PlayDealerTurn(
                            _dealer,
                            _deck);
                    }

                    _lastMessage = _engine.EvaluateWinner(
                        _player,
                        _table,
                        _dealer);
                }
            }

            return BuildState();
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

        public GameStateDto GetState()
        {
            if (_player == null)
            {
                throw new InvalidOperationException(
                    "Belum ada game aktif.");
            }

            return BuildState();
        }

        private GameStateDto BuildState()
        {
            if (_player == null || _table == null)
            {
                throw new InvalidOperationException("Game belum dimulai. Panggil NewGame dulu.");
            }

            bool isGameOver = _engine.GetTotalBalance(_player) < _table.MinBet;

            return new GameStateDto(
                _player.PlayerId,
                _player.Name,
                _engine.GetTotalBalance(_player),
                _player.CurrentBet,
                _player.Hands.Select(h => HandDto.From(_engine, h)).ToList(),
                _activeHandIndex,
                DealerDto.From(_engine, _dealer),
                _engine?.RemainingCard(_deck) ?? 0,
                _table.Rounds.Count,
                _table.MinBet,
                _table.MaxBet,
                isGameOver,
                _lastMessage);
        }
    }
}
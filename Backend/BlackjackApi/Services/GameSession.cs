using BlackjackApi.Dtos;
using BlackjackApi.Engine;
using BlackjackApi.Models;

namespace BlackjackApi.Services
{

    public class GameSessionService
    {
        private readonly GameEngine _engine = new();
        private Player? _player;
        private Table? _table;
        private Deck? _deck;
        private Dealer? _dealer;
        private string? _lastMessage;
        private int _activeHandIndex;

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


            _player = new Player(req.Name, req.StartingBalance);
            _table = new Table(1, req.MinBet, req.MaxBet);
            _deck = _engine.CreateStandardDeck();
            _dealer = new Dealer();
            _lastMessage = "Game baru dimulai.";
            _activeHandIndex = 0;
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
            if (_player == null || _table == null || _deck == null || _dealer == null)
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
            
            if (!result.Success)
            {
                return BuildState();
            }

            if (req.Action == ActionType.Split || req.Action == ActionType.Insurance)
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

        private GameStateDto BuildState()
        {
            if (_player == null || _dealer == null || _table == null || _deck == null)
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
                _engine.RemainingCard(_deck),
                _table.Rounds.Count,
                _table.MinBet,
                _table.MaxBet,
                isGameOver,
                CanOfferInsurance(),
                _lastMessage);
        }
        private bool CanOfferInsurance()
        {
            if (_player == null || _dealer == null) return false;
            if (_activeHandIndex < 0 || _activeHandIndex >= _player.Hands.Count) return false;

            var hand = _player.Hands[_activeHandIndex];
            if (hand.Cards.Count != 2 || hand.IsFinished || hand.InsuranceTaken) return false;

            return _dealer.Hand.Cards.Count > 0 && _dealer.Hand.Cards[0].Rank == Rank.Ace;
        }
    }
}
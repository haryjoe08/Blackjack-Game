using BlackjackApi.Interfaces;
using BlackjackApi.Models;

namespace BlackjackApi.Engine
{
    /// <summary>
    /// GameEngine implements IGameAction, IBetting and IDeck.
    ///
    /// STATELESS BY DESIGN (Option B): GameEngine holds NO instance state at
    /// all - no "current player/deck/table", no dealer field, nothing that
    /// persists between calls. Every method takes everything it needs as a
    /// parameter and mutates/reads only what's passed in. All session state
    /// (which player, which table, which deck, whose turn) lives entirely in
    /// GameSessionService instead.
    ///
    /// DEVIATION FROM DIAGRAM: the three interfaces' method signatures were
    /// corrected (not just worked around) to carry the parameters they
    /// actually need - see IGameAction/IBetting/IDeck for the specifics of
    /// each. An earlier version of this class kept the diagram's exact
    /// signatures and made the under-specified ones throw
    /// NotSupportedException, with a second "real" overload doing the actual
    /// work. That's gone now: since the interfaces themselves were fixed,
    /// each method here directly and fully implements its interface member -
    /// no throwing stubs, no duplicate overloads.
    /// </summary>
    public class GameEngine : IGameAction, IBetting, IDeck
    {
        private const int MaxScore = 21;

        /// <summary>
        /// DEVIATION FROM DIAGRAM: diagram had two ambiguous fields,
        /// OnCheckCardPlayer/OnCheckCardDealer (both Action&lt;int, ChipType&gt;),
        /// with no further context anywhere on what "checking a card" has to
        /// do with a chip amount and chip type - they were also never invoked
        /// anywhere. Re-purposed as ONE event whose name actually matches its
        /// parameter types: fires whenever any player's chip balance changes
        /// (bet placed, payout won, refund, etc.). The "dealer" variant was
        /// dropped - Dealer never holds chips/a Balance in this domain, so a
        /// second event for it would have nothing to report.
        /// </summary>
        public event Action<int, ChipType>? OnChipsChanged;

        // ---------- Deck operations ----------

        public Deck CreateStandardDeck()
        {
            var deck = new Deck();
            var ranks = Enum.GetValues<Rank>(); // all 13 named members, even though some share a value
            var suits = Enum.GetValues<Suit>();

            var freshCards = new List<Card>();
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    freshCards.Add(new Card(rank, suit));
                }
            }

            var rng = new Random();
            foreach (var card in freshCards.OrderBy(_ => rng.Next()))
            {
                deck.Cards.Push(card);
            }

            return deck;
        }

        public Card DrawCard(Deck deck)
        {
            if (deck.Cards.Count == 0)
            {
                throw new InvalidOperationException("Deck kosong, tidak ada kartu tersisa untuk diambil.");
            }
            return deck.Cards.Pop();
        }

        // ---------- Hand calculations ----------

        public int GetHandScore(Hand h)
        {
            int score = h.Cards.Sum(c => c.Value);
            int aceCount = h.Cards.Count(c => c.Rank == Rank.Ace);

            while (score > MaxScore && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        public bool IsHandBusted(Hand h) => GetHandScore(h) > MaxScore;

        public bool IsHandBlackjack(Hand h) => h.Cards.Count == 2 && GetHandScore(h) == 21;

        public bool IsHandSoft(Hand h)
        {
            int score = h.Cards.Sum(c => c.Value);
            int aceCount = h.Cards.Count(c => c.Rank == Rank.Ace);

            while (score > MaxScore && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return aceCount > 0;
        }

        public bool IsHandFinished(Hand h) => IsHandBusted(h) || h.IsFinished;

        // ---------- Player calculations ----------

        public int GetTotalBalance(Player p) => p.Balance.Sum(kv => (int)kv.Key * kv.Value);

        // ---------- Game lifecycle ----------

        /// <summary>
        /// DEVIATION FROM DIAGRAM: takes an explicit Dealer parameter (the
        /// diagram signature only has Player/Deck/Table). A stateless engine
        /// has nowhere else to keep "the dealer" - the caller creates a fresh
        /// Dealer and passes it in, and this method deals into it directly.
        /// </summary>
        public void StartGame(Player p, Deck deck, Table table, Dealer dealer)
        {
            p.Hands.Clear();
            p.Hands.Add(new Hand());

            var round = new GameRound { RoundNumber = table.Rounds.Count + 1 };
            table.Rounds.Add(round);

            // Standard opening deal: player, dealer, player, dealer (dealer's 2nd card hidden).
            p.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            p.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            dealer.HoleCardHidden = true;
        }

        /// <summary>
        /// DEVIATION FROM DIAGRAM: return type fixed from the diagram's typo
        /// "boo" to "bool". Stateless version: everything needed to decide
        /// "is there an active round for this player" is derivable purely
        /// from table.Rounds, so no engine-side bookkeeping is needed at all.
        /// </summary>
        public bool ResumeGame(Player p, Table table)
        {
            var lastRound = table.Rounds.LastOrDefault();
            if (lastRound == null || lastRound.ResultsByPlayerId.ContainsKey(p.PlayerId))
            {
                return false; // no round, or last round already resolved
            }
            return true;
        }

        public void PlayDealerTurn(Dealer dealer, Deck deck)
        {
            dealer.HoleCardHidden = false;

            // Standard rule: dealer hits until at least 17 (stands on soft 17).
            while (GetHandScore(dealer.Hand) < 17)
            {
                dealer.Hand.Cards.Add(DrawCard(deck));
            }
        }

        public void RevealDealerHand(Dealer dealer)
        {
            dealer.HoleCardHidden = false;
        }

        // ---------- Card actions ----------

        /// <summary>
        /// Implements IGameAction directly - the interface signature now
        /// includes Dealer (see IGameAction's deviation note), so there's no
        /// longer any need for a throwing/limited stub plus a "real" overload.
        /// This is the one and only PerformAction.
        /// </summary>
        public ActionResult PerformAction(Player p, ActionType action, Deck deck, Hand h, Dealer dealer)
        {
            // Guard: an already-finished hand (busted, or the player already
            // stood/doubled/surrendered on it) can't take any more actions.
            if (IsHandFinished(h) && action != ActionType.Insurance)
            {
                return FailResult("Tangan ini sudah selesai, tidak bisa diambil aksi lagi.");
            }

            switch (action)
            {
                case ActionType.Hit:
                    h.Cards.Add(DrawCard(deck));
                    if (IsHandBusted(h))
                    {
                        return SuccessResult("Kartu diambil. Bust!");
                    }
                    return SuccessResult("Kartu diambil.");

                case ActionType.Stand:
                    h.IsFinished = true;
                    return SuccessResult("Pemain stand.");

                case ActionType.DoubleDown:
                    if (h.Cards.Count != 2)
                    {
                        return FailResult("Double down hanya boleh dilakukan di 2 kartu pertama.");
                    }
                    if (GetTotalBalance(p) < p.CurrentBet)
                    {
                        return FailResult("Saldo tidak cukup untuk double down.");
                    }
                    RemoveChips(ChipType.White, p.CurrentBet, p);
                    p.CurrentBet *= 2;
                    h.Cards.Add(DrawCard(deck));
                    h.IsFinished = true;
                    if (IsHandBusted(h))
                    {
                        return SuccessResult("Double down. Bust!");
                    }
                    return SuccessResult("Double down selesai.");

                case ActionType.Split:
                    if (h.Cards.Count != 2 || h.Cards[0].Value != h.Cards[1].Value)
                    {
                        return FailResult("Split hanya boleh jika 2 kartu punya nilai sama.");
                    }
                    if (GetTotalBalance(p) < p.CurrentBet)
                    {
                        return FailResult("Saldo tidak cukup untuk split.");
                    }
                    RemoveChips(ChipType.White, p.CurrentBet, p);
                    var secondCard = h.Cards[1];
                    h.Cards.RemoveAt(1);
                    var newHand = new Hand();
                    newHand.Cards.Add(secondCard);
                    h.Cards.Add(DrawCard(deck));
                    newHand.Cards.Add(DrawCard(deck));
                    p.Hands.Add(newHand);
                    return SuccessResult("Split berhasil, tangan baru dibuat.");

                case ActionType.Surrender:
                    h.IsFinished = true;
                    h.IsSurrendered = true;
                    AddChips(ChipType.White, p.CurrentBet / 2, p);
                    return SuccessResult("Menyerah, setengah taruhan dikembalikan.");

                case ActionType.Insurance:
                    int insuranceCost = p.CurrentBet / 2;
                    if (GetTotalBalance(p) < insuranceCost)
                    {
                        return FailResult("Saldo tidak cukup untuk insurance.");
                    }
                    RemoveChips(ChipType.White, insuranceCost, p);
                    if (dealer.Hand.Cards.Count == 2 && GetHandScore(dealer.Hand) == 21)
                    {
                        AddChips(ChipType.White, insuranceCost * 3, p);
                        return SuccessResult("Insurance dibayar, dealer blackjack.");
                    }
                    return SuccessResult("Insurance diambil, dealer bukan blackjack.");

                default:
                    return FailResult("Aksi tidak dikenal.");
            }
        }

        /// <summary>
        /// DEVIATION FROM DIAGRAM: no longer takes Hand/ActionType/HandResult
        /// parameters. It used to compute HandScore/IsBusted/IsBlackjack here
        /// (via GetHandScore/IsHandBusted/IsHandBlackjack) and stash them on
        /// ActionResult - but nothing ever read those fields (GameSessionService
        /// only reads .Message), and BuildState() recomputes the exact same
        /// values moments later for HandDto anyway. Simplified down to what's
        /// actually needed: a message and a success flag.
        /// </summary>
   

        private static ActionResult SuccessResult(string message)
        {
            return new ActionResult
            {
                Success = true,
                Message = message
            };
        } private static ActionResult FailResult(string message)
        {
            return new ActionResult
            {
                Success = false,
                Message = message
            };
        }

        // ---------- Winner evaluation ----------

        /// <summary>
        /// DEVIATION FROM DIAGRAM: takes an explicit Dealer parameter. Not
        /// tied to any interface, so it was always free to add what it needs
        /// - same idea as PlaceBet/RemainingCard/PerformAction now that their
        /// interfaces were corrected too.
        /// </summary>
        public string EvaluateWinner(Player p, Table table, Dealer dealer)
        {
            var round = table.Rounds.LastOrDefault();
            var messages = new List<string>();

            foreach (var hand in p.Hands)
            {
                HandResult result;

                if (hand.IsSurrendered)
                {
                    result = HandResult.Surrender;
                }
                else if (IsHandBusted(hand))
                {
                    result = HandResult.Lose;
                }
                else if (IsHandBlackjack(hand) && !IsHandBlackjack(dealer.Hand))
                {
                    result = HandResult.BlackJack;
                }
                else if (IsHandBusted(dealer.Hand))
                {
                    result = HandResult.Win;
                }
                else if (GetHandScore(hand) > GetHandScore(dealer.Hand))
                {
                    result = HandResult.Win;
                }
                else if (GetHandScore(hand) < GetHandScore(dealer.Hand))
                {
                    result = HandResult.Lose;
                }
                else
                {
                    result = HandResult.Push;
                }

                messages.Add(ResultHand(p, result));
                round?.ResultsByPlayerId.TryAdd(p.PlayerId, result);
            }

            return string.Join(" | ", messages);
        }

        public string ResultHand(Player p, HandResult handResults)
        {
            switch (handResults)
            {
                case HandResult.Win:
                    AddChips(ChipType.White, p.CurrentBet * 2, p);
                    return $"{p.Name} menang, mendapat {p.CurrentBet * 2} chip.";
                case HandResult.BlackJack:
                    AddChips(ChipType.White, (int)(p.CurrentBet * 2.5), p);
                    return $"{p.Name} Blackjack! Mendapat {(int)(p.CurrentBet * 2.5)} chip.";
                case HandResult.Push:
                    AddChips(ChipType.White, p.CurrentBet, p);
                    return $"{p.Name} seri (push), taruhan dikembalikan.";
                case HandResult.Surrender:
                    return $"{p.Name} menyerah pada ronde ini.";
                case HandResult.Lose:
                default:
                    return $"{p.Name} kalah, kehilangan {p.CurrentBet} chip.";
            }
        }

        // ---------- IDeck ----------

        public void ShuffleDeck(Deck deck)
        {
            var rng = new Random();
            var shuffled = deck.Cards.OrderBy(_ => rng.Next()).ToList();
            deck.Cards.Clear();
            foreach (var card in shuffled)
            {
                deck.Cards.Push(card);
            }
        }

        /// <summary>
        /// Implements IDeck directly - the interface signature now includes
        /// Deck (see IDeck's deviation note), so no throwing stub needed.
        /// </summary>
        public int RemainingCard(Deck deck) => deck.Cards.Count;

        // ---------- IBetting ----------

        /// <summary>
        /// Implements IBetting directly - the interface signature now
        /// includes Player and Table (see IBetting's deviation note), so no
        /// throwing stub needed.
        /// </summary>
        public bool PlaceBet(Player p, Table table, int amount)
        {
            if (!IsValidBet(table, amount))
            {
                return false;
            }
            if (GetTotalBalance(p) < amount)
            {
                return false;
            }

            RemoveChips(ChipType.White, amount, p);
            p.CurrentBet = amount;
            return true;
        }

        public bool IsValidBet(Table table, int amount) => amount >= table.MinBet && amount <= table.MaxBet;

        // ---------- Chips ----------

        /// <summary>
        /// DEVIATION FROM DIAGRAM: Player parameter is required (not
        /// optional/defaulted) now that there's no "current player" fallback
        /// to fall back to - a stateless engine simply has nowhere else to
        /// get a player from.
        /// </summary>
        private int AddChips(ChipType type, int totalChips, Player player)
        {
            player.Balance.TryGetValue(type, out var current);
            player.Balance[type] = current + totalChips;
            OnChipsChanged?.Invoke(totalChips, type);
            return player.Balance[type];
        }

        private int RemoveChips(ChipType type, int totalChips, Player player)
        {
            player.Balance.TryGetValue(type, out var current);
            var updated = Math.Max(0, current - totalChips);
            player.Balance[type] = updated;
            OnChipsChanged?.Invoke(-totalChips, type);
            return updated;
        }
    }
}

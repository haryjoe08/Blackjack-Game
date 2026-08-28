using BlackjackApi.Interfaces;
using BlackjackApi.Models;

namespace BlackjackApi.Engine
{
    public class GameEngine : IGameAction, IBetting, IDeck
    {
        private const int MaxScore = 21;

        // ---------- Deck operations ----------

        public Deck CreateStandardDeck()
        {
            var deck = new Deck();
            var ranks = Enum.GetValues<Rank>(); 
            var suits = Enum.GetValues<Suit>();

            var freshCards = new List<Card>();
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    freshCards.Add(new Card(rank, suit));
                }
            }
            foreach (var card in freshCards)
            {
                deck.Cards.Push(card);
            }
            ShuffleDeck(deck);

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
        public void StartGame(Player p, Deck deck, Dealer dealer)
        {
            p.Hands.Clear();
            p.Hands.Add(new Hand());
            p.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            p.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            dealer.HoleCardHidden = true;
        }
        
        public bool ResumeGame(Player p, Dealer dealer)
        {
            bool roundInProgress = dealer.HoleCardHidden && p.Hands.Any(h => h.Cards.Count > 0);
            return roundInProgress;
        }

        public void PlayDealerTurn(Dealer dealer, Deck deck)
        {
            RevealDealerHand(dealer);

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

        public ActionResult PerformAction(Player p, ActionType action, Deck deck, Hand h, Dealer dealer)
        {
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
                    if (h.Cards.Count != 2 || h.IsFinished)
                    {
                        return FailResult("Insurance cuma bisa diambil sebelum aksi lain di tangan ini.");
                    }

                    if (h.InsuranceTaken)
                    {
                        return FailResult("Insurance sudah diambil untuk tangan ini.");
                    }

                    if (dealer.Hand.Cards.Count == 0 || dealer.Hand.Cards[0].Rank != Rank.Ace)
                    {
                        return FailResult("Insurance cuma tersedia kalau kartu terbuka dealer itu As.");
                    }

                    int insuranceCost = p.CurrentBet / 2;
                    if (GetTotalBalance(p) < insuranceCost)
                    {
                        return FailResult("Saldo tidak cukup untuk insurance.");
                    }

                    h.InsuranceTaken = true;
                    RemoveChips(ChipType.White, insuranceCost, p);
                    if (dealer.Hand.Cards.Count == 2 && GetHandScore(dealer.Hand) == 21)
                    {
                        AddChips(ChipType.White, insuranceCost * 3, p);
                        h.IsFinished = true;
                        return SuccessResult("Insurance dibayar, dealer blackjack.");
                    }
                    
                    return SuccessResult("Insurance diambil, dealer bukan blackjack.");
                default:
                    return FailResult("Aksi tidak dikenal.");
            }
        }

        private static ActionResult SuccessResult(string message)
        {
            return new ActionResult
            {
                Success = true,
                Message = message
            };
        }

        private static ActionResult FailResult(string message)
        {
            return new ActionResult
            {
                Success = false,
                Message = message
            };
        }

        // ---------- Winner evaluation ----------

        public string EvaluateWinner(Player p, Table table, Dealer dealer)
        {
          
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
        
        public int RemainingCard(Deck deck) => deck.Cards.Count;

        // ---------- IBetting ----------

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
        
        private int AddChips(ChipType type, int totalChips, Player player)
        {
            player.Balance.TryGetValue(type, out var current);
            player.Balance[type] = current + totalChips;
            return player.Balance[type];
        }

        private int RemoveChips(ChipType type, int totalChips, Player player)
        {
            player.Balance.TryGetValue(type, out var current);
            var updated = Math.Max(0, current - totalChips);
            player.Balance[type] = updated;
            return updated;
        }
    }
}
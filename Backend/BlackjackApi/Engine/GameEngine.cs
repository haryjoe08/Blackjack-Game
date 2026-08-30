using BlackjackApi.Models.Interfaces;
using BlackjackApi.Models.Enums;
using BlackjackApi.Models;

namespace BlackjackApi.Engine
{
    public class GameEngine : IGameAction, IBetting, IDeck
    {
        private const int MaxScore = 21;

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

        public int GetHandScore(Hand hand)
        {
            int score = hand.Cards.Sum(c => c.Value);
            int aceCount = hand.Cards.Count(c => c.Rank == Rank.Ace);

            while (score > MaxScore && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        public bool IsHandBusted(Hand hand) => GetHandScore(hand) > MaxScore;

        public bool IsHandBlackjack(Hand hand) => hand.Cards.Count == 2 && GetHandScore(hand) == 21;

        public bool IsHandSoft(Hand hand)
        {
            int score = hand.Cards.Sum(c => c.Value);
            int aceCount = hand.Cards.Count(c => c.Rank == Rank.Ace);

            while (score > MaxScore && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return aceCount > 0;
        }

        public bool IsHandFinished(Hand hand) => IsHandBusted(hand) || hand.IsFinished;

        public int GetTotalBalance(Player player) => player.Balance.Sum(kv => (int)kv.Key * kv.Value);

        public void StartGame(Player player, Deck deck, Dealer dealer)
        {
            player.Hands.Clear();
            player.Hands.Add(new Hand());
            player.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            player.Hands[0].Cards.Add(DrawCard(deck));
            dealer.Hand.Cards.Add(DrawCard(deck));
            dealer.HoleCardHidden = true;
        }

        public bool ResumeGame(Player player, Dealer dealer)
        {
            bool roundInProgress = dealer.HoleCardHidden && player.Hands.Any(hand => hand.Cards.Count > 0);
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

        public GameActionResult PerformAction(Player player, ActionType action, Deck deck, Hand hand, Dealer dealer)
        {
            if (IsHandFinished(hand) && action != ActionType.Insurance)
            {
                return GameActionResult.Failure("Tangan ini sudah selesai, tidak bisa diambil aksi lagi.");
            }

            switch (action)
            {
                case ActionType.Hit:
                    hand.Cards.Add(DrawCard(deck));
                    if (IsHandBusted(hand))
                    {
                        return GameActionResult.Success("Kartu diambil. Bust!");
                    }

                    return GameActionResult.Success("Kartu diambil.");

                case ActionType.Stand:
                    hand.IsFinished = true;
                    return GameActionResult.Success("Pemain stand.");

                case ActionType.DoubleDown:
                    if (hand.Cards.Count != 2)
                    {
                        return GameActionResult.Failure("Double down hanya boleh dilakukan di 2 kartu pertama.");
                    }

                    if (GetTotalBalance(player) < player.CurrentBet)
                    {
                        return GameActionResult.Failure("Saldo tidak cukup untuk double down.");
                    }

                    RemoveChips(ChipType.White, player.CurrentBet, player);
                    player.CurrentBet *= 2;
                    hand.Cards.Add(DrawCard(deck));
                    hand.IsFinished = true;

                    if (IsHandBusted(hand))
                    {
                        return GameActionResult.Success("Double down. Bust!");
                    }

                    return GameActionResult.Success("Double down selesai.");

                case ActionType.Split:
                    if (hand.Cards.Count != 2 || hand.Cards[0].Value != hand.Cards[1].Value)
                    {
                        return GameActionResult.Failure("Split hanya boleh jika 2 kartu punya nilai sama.");
                    }

                    if (GetTotalBalance(player) < player.CurrentBet)
                    {
                        return GameActionResult.Failure("Saldo tidak cukup untuk split.");
                    }

                    RemoveChips(ChipType.White, player.CurrentBet, player);
                    var secondCard = hand.Cards[1];
                    hand.Cards.RemoveAt(1);
                    var newHand = new Hand();
                    newHand.Cards.Add(secondCard);
                    hand.Cards.Add(DrawCard(deck));
                    newHand.Cards.Add(DrawCard(deck));
                    player.Hands.Add(newHand);

                    return GameActionResult.Success("Split berhasil, tangan baru dibuat.");

                case ActionType.Surrender:
                    hand.IsFinished = true;
                    hand.IsSurrendered = true;
                    AddChips(ChipType.White, player.CurrentBet / 2, player);

                    return GameActionResult.Success("Menyerah, setengah taruhan dikembalikan.");

                case ActionType.Insurance:
                    if (hand.Cards.Count != 2 || hand.IsFinished)
                    {
                        return GameActionResult.Failure("Insurance cuma bisa diambil sebelum aksi lain di tangan ini.");
                    }

                    if (hand.InsuranceTaken)
                    {
                        return GameActionResult.Failure("Insurance sudah diambil untuk tangan ini.");
                    }

                    if (dealer.Hand.Cards.Count == 0 || dealer.Hand.Cards[0].Rank != Rank.Ace)
                    {
                        return GameActionResult.Failure("Insurance cuma tersedia kalau kartu terbuka dealer itu As.");
                    }

                    int insuranceCost = player.CurrentBet / 2;
                    if (GetTotalBalance(player) < insuranceCost)
                    {
                        return GameActionResult.Failure("Saldo tidak cukup untuk insurance.");
                    }

                    hand.InsuranceTaken = true;
                    RemoveChips(ChipType.White, insuranceCost, player);

                    if (dealer.Hand.Cards.Count == 2 && GetHandScore(dealer.Hand) == 21)
                    {
                        AddChips(ChipType.White, insuranceCost * 3, player);
                        hand.IsFinished = true;
                        return GameActionResult.Success("Insurance dibayar, dealer blackjack.");
                    }

                    return GameActionResult.Success("Insurance diambil, dealer bukan blackjack.");

                default:
                    return GameActionResult.Failure("Aksi tidak dikenal.");
            }
        }

        public string EvaluateWinner(Player player, Dealer dealer)
        {
            var messages = new List<string>();

            foreach (var hand in player.Hands)
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

                messages.Add(ResultHand(player, result));
            }

            return string.Join(" | ", messages);
        }

        public string ResultHand(Player player, HandResult handResults)
        {
            switch (handResults)
            {
                case HandResult.Win:
                    AddChips(ChipType.White, player.CurrentBet * 2, player);
                    return $"{player.Name} menang, mendapat {player.CurrentBet * 2} chip.";
                case HandResult.BlackJack:
                    AddChips(ChipType.White, (int)(player.CurrentBet * 2.5), player);
                    return $"{player.Name} Blackjack! Mendapat {(int)(player.CurrentBet * 2.5)} chip.";
                case HandResult.Push:
                    AddChips(ChipType.White, player.CurrentBet, player);
                    return $"{player.Name} seri (push), taruhan dikembalikan.";
                case HandResult.Surrender:
                    return $"{player.Name} menyerah pada ronde ini.";
                case HandResult.Lose:
                default:
                    return $"{player.Name} kalah, kehilangan {player.CurrentBet} chip.";
            }
        }

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

        public bool PlaceBet(Player player, Table table, int amount)
        {
            if (!IsValidBet(table, amount))
            {
                return false;
            }

            if (GetTotalBalance(player) < amount)
            {
                return false;
            }

            RemoveChips(ChipType.White, amount, player);
            player.CurrentBet = amount;
            return true;
        }

        public bool IsValidBet(Table table, int amount) => amount >= table.MinBet && amount <= table.MaxBet;

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
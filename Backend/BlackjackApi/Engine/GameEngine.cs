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
                throw new InvalidOperationException("The deck is empty. No cards left to draw.");
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

        public bool IsHandBlackjack(Hand hand) =>
            hand.Cards.Count == 2 && GetHandScore(hand) == 21;

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

        public bool IsHandFinished(Hand hand) =>
            IsHandBusted(hand) || hand.IsFinished;

        public int GetTotalBalance(Player player) =>
            player.Balance.Sum(kv => (int)kv.Key * kv.Value);

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
            bool roundInProgress =
                dealer.HoleCardHidden &&
                player.Hands.Any(hand => hand.Cards.Count > 0);

            return roundInProgress;
        }

        public void PlayDealerTurn(Dealer dealer, Deck deck)
        {
            while (GetHandScore(dealer.Hand) < 17)
            {
                dealer.Hand.Cards.Add(DrawCard(deck));
            }

            RevealDealerHand(dealer);
        }

        public void RevealDealerHand(Dealer dealer)
        {
            dealer.HoleCardHidden = false;
        }

        public GameActionResult PerformAction(
            Player player,
            ActionType action,
            Deck deck,
            Hand hand,
            Dealer dealer)
        {
            if (IsHandFinished(hand) && action != ActionType.Insurance)
            {
                return GameActionResult.Failure(
                    "This hand is already finished. No further actions can be taken.");
            }

            switch (action)
            {
                case ActionType.Hit:
                    hand.Cards.Add(DrawCard(deck));

                    if (IsHandBusted(hand))
                    {
                        return GameActionResult.Success("Card drawn. Bust!");
                    }

                    return GameActionResult.Success("Card drawn.");

                case ActionType.Stand:
                    hand.IsFinished = true;
                    return GameActionResult.Success("Player stands.");

                case ActionType.DoubleDown:
                    if (hand.Cards.Count != 2)
                    {
                        return GameActionResult.Failure(
                            "Double down is only allowed on the first 2 cards.");
                    }

                    if (GetTotalBalance(player) < player.CurrentBet)
                    {
                        return GameActionResult.Failure(
                            "Not enough balance for double down.");
                    }

                    RemoveChips(
                        ChipType.White,
                        player.CurrentBet,
                        player);

                    player.CurrentBet *= 2;
                    hand.Cards.Add(DrawCard(deck));
                    hand.IsFinished = true;

                    if (IsHandBusted(hand))
                    {
                        return GameActionResult.Success("Double down. Bust!");
                    }

                    return GameActionResult.Success("Double down completed.");

                case ActionType.Split:
                    if (hand.Cards.Count != 2 ||
                        hand.Cards[0].Value != hand.Cards[1].Value)
                    {
                        return GameActionResult.Failure(
                            "Split is only allowed when the 2 cards have the same value.");
                    }

                    if (GetTotalBalance(player) < player.CurrentBet)
                    {
                        return GameActionResult.Failure(
                            "Not enough balance for split.");
                    }

                    RemoveChips(
                        ChipType.White,
                        player.CurrentBet,
                        player);

                    var secondCard = hand.Cards[1];
                    hand.Cards.RemoveAt(1);

                    var newHand = new Hand();
                    newHand.Cards.Add(secondCard);

                    hand.Cards.Add(DrawCard(deck));
                    newHand.Cards.Add(DrawCard(deck));

                    player.Hands.Add(newHand);

                    return GameActionResult.Success(
                        "Split successful. A new hand has been created.");

                case ActionType.Surrender:
                    hand.IsFinished = true;
                    hand.IsSurrendered = true;

                    AddChips(
                        ChipType.White,
                        player.CurrentBet / 2,
                        player);

                    return GameActionResult.Success(
                        "Surrendered. Half of the bet has been returned.");

                case ActionType.Insurance:
                    if (hand.Cards.Count != 2 || hand.IsFinished)
                    {
                        return GameActionResult.Failure(
                            "Insurance can only be taken before any other action on this hand.");
                    }

                    if (hand.InsuranceTaken)
                    {
                        return GameActionResult.Failure(
                            "Insurance has already been taken for this hand.");
                    }

                    if (dealer.Hand.Cards.Count == 0 ||
                        dealer.Hand.Cards[0].Rank != Rank.Ace)
                    {
                        return GameActionResult.Failure(
                            "Insurance is only available when the dealer's face-up card is an Ace.");
                    }

                    int insuranceCost = player.CurrentBet / 2;

                    if (GetTotalBalance(player) < insuranceCost)
                    {
                        return GameActionResult.Failure(
                            "Not enough balance for insurance.");
                    }

                    hand.InsuranceTaken = true;

                    RemoveChips(
                        ChipType.White,
                        insuranceCost,
                        player);

                    if (dealer.Hand.Cards.Count == 2 &&
                        GetHandScore(dealer.Hand) == 21)
                    {
                        AddChips(
                            ChipType.White,
                            insuranceCost * 3,
                            player);

                        hand.IsFinished = true;

                        return GameActionResult.Success(
                            $"Insurance paid {insuranceCost * 3} chips. Dealer has blackjack.");
                    }

                    return GameActionResult.Success(
                        $"Insurance taken. Dealer does not have blackjack. Lost {insuranceCost} chips.");

                default:
                    return GameActionResult.Failure("Unknown action.");
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
                else if (IsHandBlackjack(hand) &&
                         !IsHandBlackjack(dealer.Hand))
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
                    AddChips(
                        ChipType.White,
                        player.CurrentBet * 2,
                        player);

                    return $"{player.Name} wins, receiving {player.CurrentBet * 2} chips.";

                case HandResult.BlackJack:
                    AddChips(
                        ChipType.White,
                        (int)(player.CurrentBet * 2.5),
                        player);

                    return $"{player.Name} has Blackjack! Receives {(int)(player.CurrentBet * 2.5)} chips.";

                case HandResult.Push:
                    AddChips(
                        ChipType.White,
                        player.CurrentBet,
                        player);

                    return $"{player.Name} pushes. Bet returned.";

                case HandResult.Surrender:
                    return $"{player.Name} surrendered this round.";

                case HandResult.Lose:
                default:
                    return $"{player.Name} loses, losing {player.CurrentBet} chips.";
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

        public int RemainingCard(Deck deck) =>
            deck.Cards.Count;

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

            RemoveChips(
                ChipType.White,
                amount,
                player);

            player.CurrentBet = amount;

            return true;
        }

        public bool IsValidBet(Table table, int amount) =>
            amount >= table.MinBet && amount <= table.MaxBet;

        private int AddChips(
            ChipType type,
            int totalChips,
            Player player)
        {
            player.Balance.TryGetValue(type, out var current);
            player.Balance[type] = current + totalChips;

            return player.Balance[type];
        }

        private int RemoveChips(
            ChipType type,
            int totalChips,
            Player player)
        {
            player.Balance.TryGetValue(type, out var current);

            var updated = Math.Max(0, current - totalChips);
            player.Balance[type] = updated;

            return updated;
        }
    }
}
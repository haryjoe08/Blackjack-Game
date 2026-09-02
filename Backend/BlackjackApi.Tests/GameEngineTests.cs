using BlackjackApi.Engine;
using BlackjackApi.Models;
using BlackjackApi.Models.Enums;

namespace BlackjackApi.Tests
{
    [TestFixture]
    public class GameEngineTest
    {
        private GameEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new GameEngine();
        }

        #region GetHandScore

        [TestCase(Rank.Ace, Rank.King, 21)]
        [TestCase(Rank.Ace, Rank.Eight, 19)]
        [TestCase(Rank.Ten, Rank.Seven, 17)]
        [TestCase(Rank.Ten, Rank.Five, 15)]
        public void GetHandScore_TwoCards_ReturnExpectedScore(
            Rank firstRank,
            Rank secondRank,
            int expected)
        {
            var hand = new Hand();
            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));

            var result = _engine.GetHandScore(hand);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected hand score to be {expected}, but was {result}");
        }

        #endregion

        #region IsHandBlackjack

        [TestCase(Rank.Ace, Rank.King, true)]
        [TestCase(Rank.Ace, Rank.Ten, true)]
        [TestCase(Rank.King, Rank.Queen, false)]
        [TestCase(Rank.Ten, Rank.Nine, false)]
        public void IsHandBlackjack_TwoCards_ReturnExpectedResult(
            Rank firstRank,
            Rank secondRank,
            bool expected)
        {
            var hand = new Hand();
            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));

            var result = _engine.IsHandBlackjack(hand);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected IsHandBlackjack to return {expected}, but was {result}");
        }

        #endregion

        #region IsHandBusted

        [TestCase(Rank.Jack, Rank.Nine, Rank.Three, true)]
        [TestCase(Rank.Ten, Rank.Seven, Rank.Four, false)]
        [TestCase(Rank.Ace, Rank.Eight, Rank.Seven, false)]
        public void IsHandBusted_ThreeCards_ReturnExpectedResult(
            Rank firstRank,
            Rank secondRank,
            Rank thirdRank,
            bool expected)
        {
            var hand = new Hand();
            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));
            hand.Cards.Add(new Card(thirdRank, Suit.Clubs));

            var result = _engine.IsHandBusted(hand);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected IsHandBusted to return {expected}, but was {result}");
        }

        #endregion

        #region IsHandSoft

        [TestCase(Rank.Ace, Rank.Eight, Rank.Seven, false)]
        [TestCase(Rank.Ace, Rank.Eight, Rank.Two, true)]
        [TestCase(Rank.Ace, Rank.Two, Rank.Five, true)]
        [TestCase(Rank.Ten, Rank.Eight, Rank.Two, false)]
        [TestCase(Rank.Ace, Rank.King, Rank.Five, false)]
        public void IsHandSoft_Cards_ReturnExpectedResult(
             Rank firstRank,
             Rank secondRank,
             Rank thirdRank,
             bool expected)
        {
            var hand = new Hand();
            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));
            hand.Cards.Add(new Card(thirdRank, Suit.Clubs));

            var result = _engine.IsHandSoft(hand);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected IsHandSoft to return {expected}, but was {result}");
        }
        #endregion

        #region IsHandFinished

        [TestCase(Rank.Ten, Rank.Seven, Rank.Five, false, true)]
        [TestCase(Rank.Ten, Rank.Seven, Rank.Four, true, true)]
        [TestCase(Rank.Ten, Rank.Seven, Rank.Four, false, false)]
        public void IsHandFinished_HandState_ReturnExpectedResult(
            Rank firstRank,
            Rank secondRank,
            Rank thirdRank,
            bool isFinished,
            bool expected)
        {
            var hand = new Hand
            {
                IsFinished = isFinished
            };

            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));
            hand.Cards.Add(new Card(thirdRank, Suit.Clubs));

            var result = _engine.IsHandFinished(hand);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected IsHandFinished to return {expected}, but was {result}");
        }

        #endregion

        #region GetTotalBalance

        [TestCase(ChipType.White, 10, 10)]
        [TestCase(ChipType.Red, 10, 50)]
        [TestCase(ChipType.Blue, 10, 100)]
        public void GetTotalBalance_SingleChipType_ReturnExpectedBalance(
          ChipType chipType,
          int quantity,
          int expected)
        {
            var player = new Player("Player 1", 0);
            player.Balance[chipType] = quantity;

            var result = _engine.GetTotalBalance(player);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected total balance to be {expected}, but was {result}");
        }

        [Test]
        public void GetTotalBalance_MultipleChipTypes_ReturnExpectedBalance()
        {
            var player = new Player("Player 1", 0);

            player.Balance[ChipType.White] = 10;
            player.Balance[ChipType.Red] = 10;
            player.Balance[ChipType.Blue] = 5;

            var result = _engine.GetTotalBalance(player);
            Assert.That(
                result,
                Is.EqualTo(110),
                $"Total balance should equal the combined value of all chips : {result}");
        }

        #endregion

        #region GetRemainingCards

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(52)]
        public void RemainingCard_Deck_ReturnsExpectedCount(int cardCount)
        {
            var deck = new Deck();

            for (int i = 0; i < cardCount; i++)
            {
                deck.Cards.Push(new Card(Rank.Ace, Suit.Heart));
            }

            var result = _engine.RemainingCard(deck);

            Assert.That(
                result,
                Is.EqualTo(cardCount),
                $"Expected remaining cards to be {cardCount}, but was {result}");
        }

        #endregion

        #region CreateStandardDeck
        [Test]
        public void CreateStandardDeck_ReturnsStandardDeck()
        {
            var deck = _engine.CreateStandardDeck();

            Assert.That(deck, Is.Not.Null);
            Assert.That(deck.Cards.Count, Is.EqualTo(52));
        }


        #endregion

        #region ShuffleDeck
        [Test]
        public void ShuffleDeck_Deck_ChangesCardOrder()
        {
            var deck = _engine.CreateStandardDeck();

            var originalCards = deck.Cards
                .Select(card => $"{card.Rank}_{card.Suit}")
                .ToList();

            _engine.ShuffleDeck(deck);

            var shuffledCards = deck.Cards
                .Select(card => $"{card.Rank}_{card.Suit}")
                .ToList();

            Assert.That(
                shuffledCards,
                Is.Not.EqualTo(originalCards),
                "Shuffled deck should have a different card order");
        }

        #endregion

        #region IsValidBet

        [TestCase(10, 100, 10, true)]
        [TestCase(10, 100, 50, true)]
        [TestCase(10, 100, 100, true)]
        [TestCase(10, 100, 5, false)]
        [TestCase(10, 100, 101, false)]
        public void IsValidBet_BetAmount_ReturnExpectedResult(
            int minBet,
            int maxBet,
            int amount,
            bool expected)
        {
            var table = new Table(1, minBet, maxBet);

            var result = _engine.IsValidBet(table, amount);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected IsValidBet to return {expected} for bet amount {amount}");
        }

        #endregion

        #region PlaceBet

        [TestCase(10, 100, 50, 100, true)]
        [TestCase(10, 100, 10, 10, true)]
        [TestCase(10, 100, 100, 100, true)]
        [TestCase(10, 100, 101, 100, false)]
        [TestCase(10, 100, 150, 100, false)]
        public void PlaceBet_BetAmount_ReturnExpectedResult(
            int minBet,
            int maxBet,
            int amount,
            int balance,
            bool expected)
        {
            var player = new Player("Player 1", balance);
            var table = new Table(1, minBet, maxBet);

            var result = _engine.PlaceBet(player, table, amount);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected PlaceBet to return {expected} for bet amount {amount}");
        }

        [Test]
        public void PlaceBet_ValidAmount_UpdatesCurrentBetAndBalance()
        {
            var player = new Player("Player 1", 100);
            var table = new Table(1, 10, 100);

            var result = _engine.PlaceBet(player, table, 50);

            Assert.That(result, Is.True);
            Assert.That(player.CurrentBet, Is.EqualTo(50));
            Assert.That(_engine.GetTotalBalance(player), Is.EqualTo(50));
        }
        #endregion

        #region RevealDealerHand
        [Test]
        public void RevealDealerHand_Dealer_ReturnsHoleCardVisible()
        {
            var dealer = new Dealer();
            dealer.HoleCardHidden = true;

            _engine.RevealDealerHand(dealer);

            Assert.That(dealer.HoleCardHidden, Is.False);
        }
        #endregion

        #region ResumeGame
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void ResumeGame_GameState_ReturnsExpectedResult(
            bool holeCardHidden,
            bool hasCards,
            bool expected)
        {
            var player = new Player("Player 1", 100);
            var dealer = new Dealer();

            dealer.HoleCardHidden = holeCardHidden;

            if (hasCards)
            {
                player.Hands[0].Cards.Add(
                    new Card(Rank.Ace, Suit.Heart));
            }

            var result = _engine.ResumeGame(player, dealer);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected ResumeGame to return {expected}");
        }
        #endregion

        #region StartGame

        [Test]
        public void StartGame_PlayerAndDealer_DealTwoCardsEach()
        {
            var player = new Player("Player 1", 100);
            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();

            var initialCards = deck.Cards.Count;

            _engine.StartGame(player, deck, dealer);

            Assert.That(player.Hands[0].Cards.Count, Is.EqualTo(2));
            Assert.That(dealer.Hand.Cards.Count, Is.EqualTo(2));
            Assert.That(dealer.HoleCardHidden, Is.True);
            Assert.That(deck.Cards.Count, Is.EqualTo(initialCards - 4));
        }

        #endregion

        #region PlayDealerTurn

        [Test]
        public void PlayDealerTurn_DealerBelow17_DrawsUntilAtLeast17AndReveals()
        {
            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();

            dealer.Hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            dealer.Hand.Cards.Add(new Card(Rank.Five, Suit.Spades));
            dealer.HoleCardHidden = true;

            _engine.PlayDealerTurn(dealer, deck);

            Assert.That(_engine.GetHandScore(dealer.Hand), Is.GreaterThanOrEqualTo(17));
            Assert.That(dealer.HoleCardHidden, Is.False);
        }
        #endregion

        #region ResultHand

        [TestCase(HandResult.Win, "Player 1 wins, receiving 200 chips.")]
        [TestCase(HandResult.BlackJack, "Player 1 has Blackjack! Receives 250 chips.")]
        [TestCase(HandResult.Push, "Player 1 pushes. Bet returned.")]
        [TestCase(HandResult.Surrender, "Player 1 surrendered this round.")]
        [TestCase(HandResult.Lose, "Player 1 loses, losing 100 chips.")]
        public void ResultHand_HandResult_ReturnsExpectedMessage(
            HandResult handResult,
            string expected)
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var result = _engine.ResultHand(player, handResult);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected ResultHand to return: {expected}");
        }

        [TestCase(HandResult.Win, 200)]
        [TestCase(HandResult.BlackJack, 250)]
        [TestCase(HandResult.Push, 100)]
        [TestCase(HandResult.Surrender, 0)]
        [TestCase(HandResult.Lose, 0)]
        public void ResultHand_HandResult_UpdatesBalanceCorrectly(
        HandResult handResult,
        int expectedPayout)
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            _engine.ResultHand(player, handResult);

            Assert.That(
                _engine.GetTotalBalance(player),
                Is.EqualTo(1000 + expectedPayout),
                $"Expected balance to be {1000 + expectedPayout}");
        }

        #endregion

        #region EvaluateWinner

        [Test]
        public void EvaluateWinner_PlayerSurrendered_ReturnsSurrenderMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(new Card(Rank.Ten, Suit.Heart));
            player.Hands[0].Cards.Add(new Card(Rank.Seven, Suit.Spades));
            player.Hands[0].IsSurrendered = true;

            dealer.Hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            dealer.Hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo("Player 1 surrendered this round."));
        }

        [Test]
        public void EvaluateWinner_PlayerBusted_ReturnsLoseMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(new Card(Rank.King, Suit.Heart));
            player.Hands[0].Cards.Add(new Card(Rank.Nine, Suit.Spades));
            player.Hands[0].Cards.Add(new Card(Rank.Five, Suit.Clubs));

            dealer.Hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            dealer.Hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo("Player 1 loses, losing 100 chips."));
        }

        [Test]
        public void EvaluateWinner_PlayerBlackjack_ReturnsBlackjackMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(new Card(Rank.Ace, Suit.Heart));
            player.Hands[0].Cards.Add(new Card(Rank.King, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            dealer.Hand.Cards.Add(new Card(Rank.Nine, Suit.Spades));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo("Player 1 has Blackjack! Receives 250 chips."));
        }

        [Test]
        public void EvaluateWinner_DealerBusted_ReturnsWinMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(new Card(Rank.Ten, Suit.Heart));
            player.Hands[0].Cards.Add(new Card(Rank.Seven, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.King, Suit.Heart));
            dealer.Hand.Cards.Add(new Card(Rank.Nine, Suit.Spades));
            dealer.Hand.Cards.Add(new Card(Rank.Five, Suit.Clubs));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo("Player 1 wins, receiving 200 chips."));
        }

        [TestCase(Rank.Ten, Rank.Seven, Rank.Ten, Rank.Six,
            "Player 1 wins, receiving 200 chips.")]
        [TestCase(Rank.Ten, Rank.Six, Rank.Ten, Rank.Seven,
            "Player 1 loses, losing 100 chips.")]
        [TestCase(Rank.Ten, Rank.Seven, Rank.Ten, Rank.Seven,
            "Player 1 pushes. Bet returned.")]
        public void EvaluateWinner_PlayerAndDealerScores_ReturnExpectedResult(
            Rank playerFirstRank,
            Rank playerSecondRank,
            Rank dealerFirstRank,
            Rank dealerSecondRank,
            string expected)
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(
                new Card(playerFirstRank, Suit.Heart));

            player.Hands[0].Cards.Add(
                new Card(playerSecondRank, Suit.Spades));

            dealer.Hand.Cards.Add(
                new Card(dealerFirstRank, Suit.Heart));

            dealer.Hand.Cards.Add(
                new Card(dealerSecondRank, Suit.Spades));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo(expected),
                $"Expected EvaluateWinner to return: {expected}");
        }
        [Test]
        public void EvaluateWinner_BothPlayerAndDealerBlackjack_ReturnsPushMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();

            player.Hands[0].Cards.Add(new Card(Rank.Ace, Suit.Heart));
            player.Hands[0].Cards.Add(new Card(Rank.King, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ace, Suit.Diamond));
            dealer.Hand.Cards.Add(new Card(Rank.Queen, Suit.Clubs));

            var result = _engine.EvaluateWinner(player, dealer);

            Assert.That(
                result,
                Is.EqualTo("Player 1 pushes. Bet returned."));
        }

        #endregion

        #region PerformAction

        [Test]
        public void PerformAction_Hit_DrawsCard()
        {
            var player = new Player("Player 1", 1000);
            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();

            var hand = player.Hands[0];
            hand.Cards.Add(new Card(Rank.Two, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Three, Suit.Spades));

            var initialCount = hand.Cards.Count;

            var result = _engine.PerformAction(
                player,
                ActionType.Hit,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(hand.Cards.Count, Is.EqualTo(initialCount + 1));
        }

        [Test]
        public void PerformAction_Hit_WhenBust_ReturnsBustMessage()
        {
            var player = new Player("Player 1", 1000);
            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();

            var hand = player.Hands[0];
            hand.Cards.Add(new Card(Rank.King, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Nine, Suit.Spades));
            hand.Cards.Add(new Card(Rank.Two, Suit.Clubs));

            var result = _engine.PerformAction(
                player,
                ActionType.Hit,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Card drawn. Bust!"));
        }

        [Test]
        public void PerformAction_Stand_FinishesHand()
        {
            var player = new Player("Player 1", 1000);
            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();

            var hand = player.Hands[0];
            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Stand,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Player stands."));
            Assert.That(hand.IsFinished, Is.True);
        }

        [Test]
        public void PerformAction_DoubleDownWithMoreThanTwoCards_ReturnsFailure()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Two, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Three, Suit.Spades));
            hand.Cards.Add(new Card(Rank.Four, Suit.Clubs));

            var result = _engine.PerformAction(
                player,
                ActionType.DoubleDown,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Double down is only allowed on the first 2 cards."));
        }

        [Test]
        public void PerformAction_DoubleDownWithInsufficientBalance_ReturnsFailure()
        {
            var player = new Player("Player 1", 50);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Two, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Three, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.DoubleDown,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Not enough balance for double down."));
        }
        [Test]
        public void PerformAction_DoubleDownWithValidHand_DoublesBetAndFinishesHand()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = new Deck();

            deck.Cards.Push(new Card(Rank.Two, Suit.Heart));

            var hand = player.Hands[0];
            hand.Cards.Add(new Card(Rank.Five, Suit.Spades));
            hand.Cards.Add(new Card(Rank.Five, Suit.Clubs));

            var result = _engine.PerformAction(
                player,
                ActionType.DoubleDown,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Double down completed."));
            Assert.That(player.CurrentBet, Is.EqualTo(200));
            Assert.That(hand.Cards.Count, Is.EqualTo(3));
            Assert.That(hand.IsFinished, Is.True);
        }
        [Test]
        public void PerformAction_DoubleDownThatBusts_ReturnsBustMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = new Deck();

            deck.Cards.Push(new Card(Rank.Ten, Suit.Heart));

            var hand = player.Hands[0];
            hand.Cards.Add(new Card(Rank.Ten, Suit.Spades));
            hand.Cards.Add(new Card(Rank.Six, Suit.Clubs));

            var result = _engine.PerformAction(
                player,
                ActionType.DoubleDown,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Double down. Bust!"));
            Assert.That(player.CurrentBet, Is.EqualTo(200));
            Assert.That(hand.IsFinished, Is.True);
        }
        [Test]
        public void PerformAction_SplitWithInvalidCards_ReturnsFailure()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Split,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Split is only allowed when the 2 cards have the same value."));
        }

        [Test]
        public void PerformAction_SplitWithInsufficientBalance_ReturnsFailure()
        {
            var player = new Player("Player 1", 50);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Eight, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Eight, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Split,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Not enough balance for split."));
        }

        [Test]
        public void PerformAction_SplitWithValidHand_CreatesNewHand()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = new Deck();

            deck.Cards.Push(new Card(Rank.Two, Suit.Clubs));
            deck.Cards.Push(new Card(Rank.Three, Suit.Diamond));

            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Eight, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Eight, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Split,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Split successful. A new hand has been created."));

            Assert.That(player.Hands.Count, Is.EqualTo(2));

            Assert.That(hand.Cards.Count, Is.EqualTo(2));
            Assert.That(player.Hands[1].Cards.Count, Is.EqualTo(2));

            Assert.That(hand.Cards[0].Rank, Is.EqualTo(Rank.Eight));
            Assert.That(player.Hands[1].Cards[0].Rank, Is.EqualTo(Rank.Eight));
        }

        [TestCase(Rank.Ten, Rank.Jack)]
        [TestCase(Rank.Queen, Rank.King)]
        public void PerformAction_SplitWithSameValueCards_CreatesNewHand(
            Rank firstRank,
            Rank secondRank)
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = new Deck();

            deck.Cards.Push(new Card(Rank.Two, Suit.Clubs));
            deck.Cards.Push(new Card(Rank.Three, Suit.Diamond));

            var hand = player.Hands[0];

            hand.Cards.Add(new Card(firstRank, Suit.Heart));
            hand.Cards.Add(new Card(secondRank, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Split,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(player.Hands.Count, Is.EqualTo(2));
        }

        [Test]
        public void PerformAction_Surrender_FinishesHandAndReturnsHalfBet()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            var result = _engine.PerformAction(
                player,
                ActionType.Surrender,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo("Surrendered. Half of the bet has been returned."));

            Assert.That(hand.IsFinished, Is.True);
            Assert.That(hand.IsSurrendered, Is.True);
            Assert.That(_engine.GetTotalBalance(player), Is.EqualTo(1050));
        }

        [Test]
        public void PerformAction_FinishedHand_ReturnsFailure()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));
            hand.IsFinished = true;

            var result = _engine.PerformAction(
                player,
                ActionType.Hit,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "This hand is already finished. No further actions can be taken."));
        }


        [Test]
        public void PerformAction_InsuranceAlreadyTaken_ReturnsFailure()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));
            hand.InsuranceTaken = true;

            dealer.Hand.Cards.Add(new Card(Rank.Ace, Suit.Diamond));

            var result = _engine.PerformAction(
                player,
                ActionType.Insurance,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Insurance has already been taken for this hand."));
        }

        [Test]
        public void PerformAction_InsuranceWhenDealerUpCardIsNotAce_ReturnsFailure()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ten, Suit.Diamond));

            var result = _engine.PerformAction(
                player,
                ActionType.Insurance,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Insurance is only available when the dealer's face-up card is an Ace."));
        }

        [Test]
        public void PerformAction_InsuranceWithInsufficientBalance_ReturnsFailure()
        {
            var player = new Player("Player 1", 40);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ace, Suit.Diamond));

            var result = _engine.PerformAction(
                player,
                ActionType.Insurance,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Message,
                Is.EqualTo("Not enough balance for insurance."));
        }

        [Test]
        public void PerformAction_InsuranceWithDealerNotBlackjack_ReturnsInsuranceMessage()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ace, Suit.Diamond));
            dealer.Hand.Cards.Add(new Card(Rank.Nine, Suit.Clubs));

            var result = _engine.PerformAction(
                player,
                ActionType.Insurance,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Insurance taken. Dealer does not have blackjack. Lost 50 chips."));

            Assert.That(hand.InsuranceTaken, Is.True);
            Assert.That(hand.IsFinished, Is.False);

            Assert.That(
                _engine.GetTotalBalance(player),
                Is.EqualTo(950));
        }

        [Test]
        public void PerformAction_InsuranceWithDealerBlackjack_PaysInsurance()
        {
            var player = new Player("Player 1", 1000);
            player.CurrentBet = 100;

            var dealer = new Dealer();
            var deck = _engine.CreateStandardDeck();
            var hand = player.Hands[0];

            hand.Cards.Add(new Card(Rank.Ten, Suit.Heart));
            hand.Cards.Add(new Card(Rank.Seven, Suit.Spades));

            dealer.Hand.Cards.Add(new Card(Rank.Ace, Suit.Diamond));
            dealer.Hand.Cards.Add(new Card(Rank.King, Suit.Clubs));

            var initialBalance = _engine.GetTotalBalance(player);

            var result = _engine.PerformAction(
                player,
                ActionType.Insurance,
                deck,
                hand,
                dealer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Message,
                Is.EqualTo(
                    "Insurance paid 150 chips. Dealer has blackjack."));

            Assert.That(hand.InsuranceTaken, Is.True);
            Assert.That(hand.IsFinished, Is.True);

            Assert.That(
                _engine.GetTotalBalance(player),
                Is.EqualTo(initialBalance + 100));
        }

        #endregion
    }
}
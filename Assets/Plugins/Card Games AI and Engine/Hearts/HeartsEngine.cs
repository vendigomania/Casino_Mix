using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game Hearts. Rules:</para>
    /// <para>The deck is dealt evenly among the players.
    /// At the start, each player passes 3 cards to the player on the left, next round to the next player and so on.
    /// After card passing, the player with the two of clubs starts.</para>
    /// <para>Players take clockwise turns in placing one card on the table each trick.
    /// The card has to be of the same suit as the one who started the trick.
    /// A trick ends when all players have placed a card, and the player who placed the highest card of the original suit collects the cards and starts the next trick.</para>
    /// <para>Hearts cannot be used to start a trick until at least one heart is played.
    /// At the end of the round, players get score based on the cards they collected.</para>
    /// <para>Each heart collected is +1 point, and the queen of spades is +13 points.
    /// If one player collected all of the aforementioned cards, they get -26 points or other players get +26 points instead, depending which benefits them most.</para>
    /// <para>When a player reaches a score of 100, the player with the lowest score wins.</para>
    /// </summary>
    public static class HeartsEngine
    {
        /// <summary>
        /// Enumeration to specify the current state of the game.
        /// </summary>
        public enum GameState
        {
            /// <summary>
            /// The game is not active. Execute ResetGame()-method to start it.
            /// </summary>
            inactive = 0,

            cardPassing = 1,

            normalTurn = 2
        }

        /// <summary>
        /// Enumeration to specify the type of a player.
        /// </summary>
        public enum PlayerType
        {
            /// <summary>
            /// Local player. The input is given through the LocalMove()-method.
            /// </summary>
            local = 0,

            /// <summary>
            /// AI. The move is made automatically by the HeartsAI class.
            /// </summary>
            AI = 1,

            /// <summary>
            /// Custom. The input is given through the CustomMove()-method. Can be used, for example, to input the moves of a remote player.
            /// </summary>
            custom = 2
        }

        private static PlayerType[] _seats;
        /// <summary>
        /// Number of players. From 3 to 6
        /// </summary>
        public static PlayerType[] seats
        {
            get
            {
                return _seats;
            }
            set
            {
                if (value != null && value.Length < 3)
                {
                    _seats = new PlayerType[3] { PlayerType.local, PlayerType.AI, PlayerType.AI };
                }
                else if (value != null && value.Length > 6)
                {
                    _seats = new PlayerType[6] { value[0], value[1], value[2], value[3], value[4], value[5] };
                }
                else
                {
                    _seats = value;
                }
            }
        }

        private static int[] score;
        /// <summary>
        /// Array of each player's score.
        /// </summary>
        public static int[] Score
        {
            get
            {
                if (score == null)
                    ResetGame();
                return score;
            }
        }

        private static List<Card>[] hands;
        /// <summary>
        /// Array of lists of cards in each player's hand.
        /// </summary>
        public static List<Card>[] Hands
        {
            get
            {
                if (hands == null)
                    ResetGame();
                return hands;
            }
        }

        private static List<Card>[] collected;
        /// <summary>
        /// Array of lists of cards each player has collected.
        /// </summary>
        public static List<Card>[] Collected
        {
            get
            {
                if (collected == null)
                    ResetGame();
                return collected;
            }
        }

        private static List<Card> table;
        /// <summary>
        /// List of cards already on the table for the current trick.
        /// </summary>
        public static List<Card> Table
        {
            get
            {
                if (table == null)
                    ResetGame();
                return table;
            }
        }

        private static int currentPlayer;
        /// <summary>
        /// The number of the current player. Ranges from 1 to length of 'seats'.
        /// </summary>
        public static int CurrentPlayer
        {
            get
            {
                if (currentPlayer == 0)
                    ResetGame();
                return currentPlayer;
            }
        }

        private static int passDifference;
        /// <summary>
        /// The difference in player number for who to pass cards. Clockwise.
        /// If PassDifference is 0, no cards are passed.
        /// </summary>
        public static int PassDifference
        {
            get
            {
                return passDifference;
            }
        }

        private static GameState _gameState = GameState.cardPassing;
        /// <summary>
        /// The current state of the game. As a GameState enumeration.
        /// </summary>
        public static GameState gameState
        {
            get
            {
                return _gameState;
            }
        }

        /// <summary>
        /// time to wait between certain actions in milliseconds
        /// </summary>
        public static int waitPeriod = 1000;

        //private variables
        private static Thread AIThread;

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame()
        {
            StopAI();

            hands = new List<Card>[seats.Length];
            collected = new List<Card>[seats.Length];
            table = new List<Card>();
            score = new int[seats.Length];
            Deck deck = new Deck();
            int startingCards = 52 / seats.Length;
            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                collected[i] = new List<Card>();
                for (int c = 0; c < startingCards; ++c)
                {
                    Card card = deck.Deal();
                    if ((seats.Length == 3 || seats.Length > 4) && card.suit == Suit.diamond && card.value == 2)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 4 && card.suit == Suit.club && card.value == 3)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 5 && card.suit == Suit.diamond && card.value == 3)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 5 && card.suit == Suit.spade && card.value == 2)
                    {
                        --c;
                        continue;
                    }

                    hands[i].Add(card);
                }

                hands[i] = Utils.SortCards(hands[i], true);
            }

            _gameState = GameState.cardPassing;
            passDifference = 1;
            currentPlayer = 1;

            if (seats[0] == PlayerType.AI)
                AITurn();

        }

        /// <summary>
        /// Resets all variables related to the current round and stops the AI if needed.
        /// </summary>
        public static void ResetRound()
        {
            hands = new List<Card>[seats.Length];
            collected = new List<Card>[seats.Length];
            table = new List<Card>();
            Deck deck = new Deck();
            int startingCards = 52 / seats.Length;

            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                collected[i] = new List<Card>();
                for (int c = 0; c < startingCards; ++c)
                {
                    Card card = deck.Deal();
                    if ((seats.Length == 3 || seats.Length > 4) && card.suit == Suit.diamond && card.value == 2)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 4 && card.suit == Suit.club && card.value == 3)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 5 && card.suit == Suit.diamond && card.value == 3)
                    {
                        --c;
                        continue;
                    }
                    else if (seats.Length > 5 && card.suit == Suit.spade && card.value == 2)
                    {
                        --c;
                        continue;
                    }

                    hands[i].Add(card);
                }

                hands[i] = Utils.SortCards(hands[i], true);
            }

            _gameState = GameState.cardPassing;
            passDifference++;
            if (passDifference == seats.Length)
            {
                passDifference = 0;
                _gameState = GameState.normalTurn;
                for (int i = 0; i < hands.Length; ++i)
                {
                    bool found = false;
                    for (int k = 0; k < hands[i].Count; ++k)
                    {
                        if (hands[i][k].suit == Suit.club && hands[i][k].value == 2)
                        {
                            currentPlayer = i + 1;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }
            }
            else
            {
                currentPlayer = 1;
            }

            if ((AIThread == null || !AIThread.IsAlive) && seats[currentPlayer - 1] == PlayerType.AI)
                AITurn();
            else if ((AIThread != null && AIThread.IsAlive) && seats[currentPlayer - 1] == PlayerType.AI)
                ExecuteAIMove();

        }

        /// <summary>
        /// This method can be used to stop the AI. This will abort the whole AI thread, meaning that the AI will not do a move. Should not be needed by the user.
        /// </summary>
        public static void StopAI()
        {
            if (AIThread != null)
            {
                if (AIThread.IsAlive)
                    AIThread.Abort();
                AIThread = null;
            }
        }

        /// <summary>
        /// Helper method used to run the AI thread asynchronously.
        /// </summary>
        /// <param name="_t">The thread to run asynchronously</param>
        private static async void WaitForJobDone(Thread _t)
        {
            while (_t.IsAlive)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a local player.
        /// </summary>
        /// <param name="card">The used hand card.</param>
        public static void LocalMove(Card card)
        {
            if ((AIThread == null || !AIThread.IsAlive) && PossibleMoves().Contains(card) && seats[currentPlayer-1] == PlayerType.local && (_gameState != GameState.inactive))
            {
                ApplyTurn(card);
                if (seats[currentPlayer-1] == PlayerType.AI)
                {
                    AITurn();
                }
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="card">The used hand card.</param>
        public static void CustomMove(Card card)
        {
            if ((AIThread == null || !AIThread.IsAlive) && PossibleMoves().Contains(card) && seats[currentPlayer-1] == PlayerType.custom && (_gameState != GameState.inactive))
            {
                ApplyTurn(card);
                if (seats[currentPlayer-1] == PlayerType.AI)
                {
                    AITurn();
                }
            }
        }

        /// <summary>
        /// Gives the HeartsAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {

            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            Card aiCard = HeartsAI.MakeMove(hands[currentPlayer - 1], collected, table, _gameState);

            System.TimeSpan calcTime = System.DateTime.UtcNow - startTime;
            if (calcTime.TotalMilliseconds < waitPeriod)
                Thread.Sleep(waitPeriod - (int)(calcTime.TotalMilliseconds));


            ApplyTurn(aiCard);
        }

        /// <summary>
        /// Cards are being dealt to players.
        /// </summary>
        private static void DealerTurn()
        {
            if (_gameState == GameState.cardPassing)
            {
                int targetPlayer = currentPlayer + passDifference;
                if (targetPlayer > collected.Length)
                    targetPlayer -= collected.Length;

                if (collected[targetPlayer - 1].Count >= 3)
                {
                    Thread.Sleep(waitPeriod);

                    targetPlayer++;
                    currentPlayer++;
                }
                if (currentPlayer > collected.Length)
                    currentPlayer -= collected.Length;
                if (targetPlayer > collected.Length)
                    targetPlayer -= collected.Length;
                if (collected[targetPlayer - 1].Count >= 3)
                {
                    for (int i = 0; i < seats.Length; ++i)
                    {
                        hands[i].AddRange(collected[i]);
                        collected[i].Clear();
                        hands[i] = Utils.SortCards(hands[i], true);
                    }
                    _gameState = GameState.normalTurn;
                    for (int i = 0; i < hands.Length; ++i)
                    {
                        bool found = false;
                        for (int k = 0; k < hands[i].Count; ++k)
                        {
                            if (hands[i][k].suit == Suit.club && hands[i][k].value == 2)
                            {
                                currentPlayer = i + 1;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            break;
                    }
                }
            }
            else if (_gameState == GameState.normalTurn)
            {
                if (table.Count >= seats.Length)
                {
                    Thread.Sleep(waitPeriod);

                    int highestIndex = 0;
                    int highestValue = table[0].value;
                    if (highestValue == 1)
                        highestValue = 14;
                    for (int i = 1; i < table.Count; ++i)
                    {
                        if (table[i].suit == table[0].suit && table[i].value > highestValue)
                        {
                            highestIndex = i;
                            highestValue = table[i].value;
                        }
                        else if (table[i].suit == table[0].suit && table[i].value == 1)
                        {
                            highestIndex = i;
                            highestValue = 14;
                            break;
                        }
                    }

                    currentPlayer = currentPlayer + highestIndex + 1;
                    if (currentPlayer > seats.Length)
                        currentPlayer -= seats.Length;
                    collected[currentPlayer - 1].AddRange(table);
                    table.Clear();

                }
                else
                {
                    currentPlayer++;
                    if (currentPlayer > collected.Length)
                        currentPlayer -= collected.Length;
                }
            }

            //checking for round end
            if (_gameState == GameState.normalTurn && hands[currentPlayer - 1].Count == 0)
            {
                //counting score
                for (int i = 0; i < collected.Length; ++i)
                {
                    int newscore = 0;
                    for (int c = 0; c < collected[i].Count; ++c)
                    {
                        if (collected[i][c].suit == Suit.heart)
                            newscore++;
                        else if (collected[i][c].suit == Suit.spade && collected[i][c].value == 12)
                            newscore += 13;
                    }

                    //blowout
                    if (newscore == 26)
                    {
                        int[] possiblescore = Utils.IndependentCopyArray(score);
                        for (int k = 0; k < possiblescore.Length; ++k)
                        {
                            if (k != i)
                                possiblescore[k] += 26;
                        }
                        if (Utils.MaxInt(possiblescore) >= 100 && possiblescore[i] != Utils.MinInt(possiblescore))
                        {
                            //Player would not win. Deducting 26 from own score instead.
                            score[i] -= 26;
                        }
                        else
                        {
                            for (int k = 0; k < score.Length; ++k)
                            {
                                if (k != i)
                                    score[k] += 26;
                            }
                        }

                        break;
                    }
                    else
                    {
                        score[i] += newscore;
                    }
                }

                //checking for game end
                bool end = false;
                for (int i = 0; i < score.Length; ++i)
                {
                    if (score[i] >= 100)
                    {
                        end = true;
                        break;
                    }
                }

                if (!end)
                {
                    ResetRound();
                }
                else
                {
                    _gameState = GameState.inactive;
                }
            }

            //switching turns
            if (_gameState != GameState.inactive)
            {
                if (seats[currentPlayer - 1] == PlayerType.AI)
                {
                    ExecuteAIMove();
                }
                else
                {
                    if (AIThread != null && AIThread.IsAlive)
                        StopAI();
                }
            }
        }

        /// <summary>
        /// Runs 'DealerTurn' method in another thread.
        /// </summary>
        private static void InitiateDealerTurn()
        {
            if (AIThread != null && AIThread.IsAlive)
                StopAI();

            AIThread = new Thread(new ThreadStart(DealerTurn));
            AIThread.Start();
            WaitForJobDone(AIThread);
        }

        /// <summary>
        /// Runs the ExecuteAIMove-method in another thread to avoid keeping the main thread busy.
        /// </summary>
        private static void AITurn()
        {
            if (AIThread != null && AIThread.IsAlive)
                StopAI();

            AIThread = new Thread(new ThreadStart(ExecuteAIMove));
            AIThread.Start();
            WaitForJobDone(AIThread);

        }

        private static bool HeartPlayed(List<Card>[] collected, List<Card> table)
        {
            for (int i = 0; i < table.Count; ++i)
            {
                if (table[i].suit == Suit.heart)
                    return true;
            }
            for (int i = 0; i < collected.Length; ++i)
            {
                for (int c = 0; c < collected[i].Count; ++c)
                {
                    if (collected[i][c].suit == Suit.heart)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all possible moves for the given situation.
        /// </summary>
        /// <param name="hand">List of cards in the player's hand.</param>
        /// <param name="collected">Lists of cards collected by all players.</param>
        /// <param name="table">List of cards currently on the table.</param>
        /// <param name="gs">State of the game. I.e. card passing or normal turn.</param>
        /// <returns>List of cards that can be legitimately played from the player's hand.</returns>
        public static List<Card> PossibleMoves(List<Card> hand, List<Card>[] collected, List<Card> table, GameState gs)
        {
            if (gs == GameState.cardPassing)
            {
                return hand;
            }
            else if (gs == GameState.normalTurn)
            {
                if (table.Count == 0)
                {
                    if (HeartPlayed(collected, table))
                        return hand;


                    List<Card> playableCards = new List<Card>();
                    for (int i = 0; i < hand.Count; ++i)
                    {
                        if (hand[i].suit != Suit.heart)
                            playableCards.Add(hand[i]);
                        if (hand[i].suit == Suit.club && hand[i].value == 2)
                            return new List<Card>() { hand[i] };
                    }

                    if (playableCards.Count == 0)
                        return hand;
                    else
                        return playableCards;
                }
                else
                {
                    List<Card> playableCards = new List<Card>();
                    for (int i = 0; i < hand.Count; ++i)
                    {
                        if (hand[i].suit == table[0].suit)
                            playableCards.Add(hand[i]);
                    }

                    if (playableCards.Count == 0)
                    {

                        if (table[0].suit == Suit.club && table[0].value == 2)
                        {
                            playableCards = new List<Card>();
                            for (int i = 0; i < hand.Count; ++i)
                            {
                                if (hand[i].suit != Suit.heart && (hand[i].suit != Suit.spade || hand[i].value != 12))
                                    playableCards.Add(hand[i]);
                            }
                            return playableCards;
                        }

                        return hand;
                    }
                    else
                        return playableCards;

                }
            }

            return new List<Card>();
        }

        /// <summary>
        /// Finds all possible moves for the current player.
        /// </summary>
        /// <returns>List of cards that can be legitimately played from the current player's hand.</returns>
        public static List<Card> PossibleMoves()
        {
            return PossibleMoves(hands[currentPlayer - 1], collected, table, _gameState);
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="card">Card used from the hand.</param>
        private static void ApplyTurn(Card card)
        {
            hands[currentPlayer-1].Remove(card);

            if (_gameState == GameState.cardPassing)
            {
                int targetPlayer = currentPlayer + passDifference;
                if (targetPlayer > collected.Length)
                    targetPlayer -= collected.Length;
                collected[targetPlayer-1].Add(card);

                if (AIThread != null && AIThread.IsAlive)
                    DealerTurn();
                else
                    InitiateDealerTurn();
            }
            else if (_gameState == GameState.normalTurn)
            {
                table.Add(card);

                if (AIThread != null && AIThread.IsAlive)
                    DealerTurn();
                else
                    InitiateDealerTurn();

            }
        }
    }
}
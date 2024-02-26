using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game Casino. Rules:</para>
    /// <para>Each player is dealt four cards, as well as the table.
    /// Players take turns to either use a card to collect cards from the table, or throw the card to the table.</para>
    /// <para>To collect cards, the sum of the cards you collect must match the card you are using from your hand.
    /// I.e. you can collect a six and a three from the table using a nine.
    /// Players are also allowed to take multiples of the used hand card.
    /// I.e. you can collect two sixes and two threes from the table using a nine.
    /// It is not allowed to play a card but leave collectable cards on the table.</para>
    /// <para>Some cards have special values when used from the hand (but not when they are on the table):
    /// Two of spades has the value of 15.
    /// Ten of diamonds has the value of 16.
    /// Each ace has the value of 14.</para>
    /// <para>When players are out of cards, each player are dealt 4 new cards.
    /// This is repeated until the deck is empty, at which point the player who collected cards last gets the rest from the table.</para>
    /// <para>Points are calculated at the end of each round:
    /// 2 points for collecting most spades,
    /// 1 point for collecting most cards,
    /// 2 points for collecting ten of diamonds,
    /// 1 point for collecting two of spades or any ace.
    /// The first player to get 16 points wins.</para>
    /// </summary>
    public static class CasinoEngine
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

            /// <summary>
            /// Cards are being dealt.
            /// </summary>
            cardDealing = 1,

            /// <summary>
            /// It is a player's turn.
            /// </summary>
            normalTurn = 2,

            /// <summary>
            /// The game is between turns.
            /// </summary>
            endOfTurn = 3
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
            /// AI. The move is made automatically by the AI.
            /// </summary>
            AI = 1,

            /// <summary>
            /// Custom. The input is given through the CustomMove()-method. Can be used, for example, to input the moves of a remote player.
            /// </summary>
            custom = 2
        }

        /// <summary>
        /// A struct to represent moves in Casino. A move consists of a hand card and possible list of cards collected from the table.
        /// </summary>
        public struct CasinoMove
        {
            public CasinoMove(Card _handCard, List<Card> _tableCards)
            {
                handCard = _handCard;
                tableCards = _tableCards;
            }

            public CasinoMove(Card _handCard, Card[] _tableCards)
            {
                handCard = _handCard;
                tableCards = new List<Card>();
                for (int i = 0; i < _tableCards.Length; ++i)
                {
                    tableCards.Add(_tableCards[i]);
                }
            }

            public readonly Card handCard;
            public readonly List<Card> tableCards;
        }

        private static PlayerType[] _seats;
        /// <summary>
        /// Number of players. From 2 to 4
        /// </summary>
        public static PlayerType[] seats
        {
            get
            {
                return _seats;
            }
            set
            {
                if (value != null && value.Length < 2)
                {
                    _seats = new PlayerType[2] { PlayerType.local, PlayerType.AI };
                }
                else if (value != null && value.Length > 4)
                {
                    _seats = new PlayerType[4] { value[0], value[1], value[2], value[3] };
                }
                else
                {
                    _seats = value;
                }
            }
        }

        private static int[] score;
        /// <summary>
        /// Each player's score.
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
        /// List of cards on each player's hand.
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
        /// List of cards collected by each player.
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
        /// List of cards on the table.
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
        /// The current player. Ranges from 1 to length of 'seats'.
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

        private static Card usedCard;
        /// <summary>
        /// The hand card that was just used by a player.
        /// </summary>
        public static Card UsedCard
        {
            get
            {
                if (usedCard == null)
                    ResetGame();
                return usedCard;
            }
        }

        private static List<Card> takenCards;
        /// <summary>
        /// The cards just collected from the table by a player.
        /// </summary>
        public static List<Card> TakenCards
        {
            get
            {
                if (takenCards == null)
                    ResetGame();
                return takenCards;
            }
        }

        private static Deck deck;
        /// <summary>
        /// Number of cards left in the deck.
        /// </summary>
        public static int cardsInDeck
        {
            get
            {
                if (deck != null)
                    return deck.Cards.Count;

                return 0;
            }
        }

        private static GameState _gameState = GameState.cardDealing;
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
        private static int spadesExtraPoints = 0;
        private static int cardsExtraPoints = 0;
        private static int mostRecentPlayer = 1;

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame()
        {
            StopAI();

            hands = new List<Card>[seats.Length];
            collected = new List<Card>[seats.Length];
            score = new int[seats.Length];
            table = new List<Card>();
            mostRecentPlayer = 1;
            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                collected[i] = new List<Card>();
                score[i] = 0;
            }
            deck = new Deck();
            spadesExtraPoints = 0;
            cardsExtraPoints = 0;

            _gameState = GameState.cardDealing;
            currentPlayer = 1;

            InitiateDealerTurn();

        }

        /// <summary>
        /// Resets all variables related to the current round and stops the AI if needed.
        /// </summary>
        public static void ResetRound()
        {
            hands = new List<Card>[seats.Length];
            collected = new List<Card>[seats.Length];
            table = new List<Card>();
            deck = new Deck();
            mostRecentPlayer = 1;

            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                collected[i] = new List<Card>();
            }

            _gameState = GameState.cardDealing;
            
            if ((AIThread == null || !AIThread.IsAlive) && seats[currentPlayer - 1] == PlayerType.AI)
                InitiateDealerTurn();
            else if ((AIThread != null && AIThread.IsAlive) && seats[currentPlayer - 1] == PlayerType.AI)
                DealerTurn();

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
        /// <param name="tableCards">List of possibly collected table cards.</param>
        public static void LocalMove(Card card, List<Card> tableCards)
        {
            tableCards = Utils.SortCards(tableCards);

            bool validMove = false;
            List<CasinoMove> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i].handCard == card && possibleMoves[i].tableCards.Count == tableCards.Count)
                {
                    validMove = true;
                    for (int k = 0; k < possibleMoves[i].tableCards.Count; ++k)
                    {
                        if (possibleMoves[i].tableCards[k] != tableCards[k])
                        {
                            validMove = false;
                            break;
                        }    
                    }

                    if (validMove)
                        break;
                }
            }

            if ((AIThread == null || !AIThread.IsAlive) && validMove && seats[currentPlayer - 1] == PlayerType.local && _gameState == GameState.normalTurn)
            {
                ApplyTurn(card, tableCards);
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="card">The used hand card.</param>
        /// <param name="tableCards">List of possibly collected table cards.</param>
        public static void CustomMove(Card card, List<Card> tableCards)
        {
            tableCards = Utils.SortCards(tableCards);

            bool validMove = false;
            List<CasinoMove> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i].handCard == card && possibleMoves[i].tableCards.Count == tableCards.Count)
                {
                    validMove = true;
                    for (int k = 0; k < possibleMoves[i].tableCards.Count; ++k)
                    {
                        if (possibleMoves[i].tableCards[k] != tableCards[k])
                        {
                            validMove = false;
                            break;
                        }    
                    }

                    if (validMove)
                        break;
                }
            }

            if ((AIThread == null || !AIThread.IsAlive) && validMove && seats[currentPlayer - 1] == PlayerType.custom && _gameState == GameState.normalTurn)
            {
                ApplyTurn(card, tableCards);
            }
        }

        /// <summary>
        /// Gives the CasinoAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            CasinoMove aiMove = CasinoAI.MakeMove(hands[currentPlayer-1], table, collected);

            System.TimeSpan calcTime = System.DateTime.UtcNow - startTime;
            if (calcTime.TotalMilliseconds < waitPeriod)
                Thread.Sleep(waitPeriod - (int)(calcTime.TotalMilliseconds));

            ApplyTurn(aiMove.handCard, aiMove.tableCards);
        }

        /// <summary>
        /// Cards are being dealt to players.
        /// </summary>
        private static void DealerTurn()
        {
            if (_gameState == GameState.cardDealing)
            {
                //players
                for (int i = currentPlayer; i < seats.Length + currentPlayer; ++i)
                {
                    Thread.Sleep(waitPeriod/2);
                    hands[(i-1)%seats.Length].Add(deck.Deal());
                    hands[(i - 1) % seats.Length].Add(deck.Deal());
                }

                //table
                Thread.Sleep(waitPeriod/2);
                table.Add(deck.Deal());
                table.Add(deck.Deal());

                //players
                for (int i = currentPlayer; i < seats.Length + currentPlayer; ++i)
                {
                    Thread.Sleep(waitPeriod/2);
                    hands[(i - 1) % seats.Length].Add(deck.Deal());
                    hands[(i - 1) % seats.Length].Add(deck.Deal());
                }

                //table
                Thread.Sleep(waitPeriod/2);
                table.Add(deck.Deal());
                table.Add(deck.Deal());

                _gameState = GameState.normalTurn;

                if (seats[currentPlayer - 1] == PlayerType.AI)
                {
                    ExecuteAIMove();
                }
            }
            else if (_gameState == GameState.normalTurn || _gameState == GameState.endOfTurn)
            {
                _gameState = GameState.cardDealing;
                //round end
                if (deck.Cards.Count == 0)
                {
                    currentPlayer--;
                    if (currentPlayer < 1)
                        currentPlayer += seats.Length;

                    //giving table cards to the player that most recently took something from the table
                    collected[mostRecentPlayer - 1].AddRange(table);
                    table = new List<Card>();

                    //counting score   
                    //spades
                    int[] spades = new int[collected.Length];
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        spades[i] = 0;
                        for (int k = 0; k < collected[i].Count; ++k)
                        {
                            if (collected[i][k].suit == Suit.spade)
                                spades[i]++;
                        }
                    }

                    int highestSpades = Utils.MaxInt(spades);
                    int ind = -1;
                    for (int i = 0; i < spades.Length; ++i)
                    {
                        if (spades[i] == highestSpades)
                        {
                            if (ind >= 0)
                            {
                                //tie
                                spadesExtraPoints += 2;
                                ind = -1;
                                break;
                            }
                            ind = i;
                        }
                    }
                    if (ind >= 0)
                    {
                        score[ind] += 2 + spadesExtraPoints;
                        spadesExtraPoints = 0;
                    }

                    //cards
                    int highestCardCount = 0;
                    int highestCardIndex = -1;
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        if (collected[i].Count > highestCardCount)
                        {
                            highestCardCount = collected[i].Count;
                            highestCardIndex = i;
                        }
                        else if (collected[i].Count == highestCardCount)
                        {
                            highestCardIndex = -1;
                        }
                    }
                    if (highestCardIndex >= 0)
                    {
                        score[highestCardIndex] += 1 + cardsExtraPoints;
                    }
                    else
                    {
                        cardsExtraPoints += 1;
                    }

                    //point cards
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        int newscore = 0;
                        for (int k = 0; k < collected[i].Count; ++k)
                        {
                            if (collected[i][k].value == 1)
                                newscore++;
                            else if (collected[i][k].suit == Suit.spade && collected[i][k].value == 2)
                                newscore++;
                            else if (collected[i][k].suit == Suit.diamond && collected[i][k].value == 10)
                                newscore += 2;
                        }
                        score[i] += newscore;
                    }

                    if (Utils.MaxInt(score) < 16)
                        ResetRound();
                    else
                    {
                        //game end
                        _gameState = GameState.inactive;
                    }
                }
                else
                {
                    Thread.Sleep(waitPeriod/2);

                    //players
                    for (int i = currentPlayer; i < seats.Length + currentPlayer; ++i)
                    {
                        Thread.Sleep(waitPeriod/2);
                        hands[(i - 1) % seats.Length].Add(deck.Deal());
                        hands[(i - 1) % seats.Length].Add(deck.Deal());
                    }

                    //players
                    for (int i = currentPlayer; i < seats.Length + currentPlayer; ++i)
                    {
                        Thread.Sleep(waitPeriod/2);
                        hands[(i - 1) % seats.Length].Add(deck.Deal());
                        hands[(i - 1) % seats.Length].Add(deck.Deal());
                    }

                    _gameState = GameState.normalTurn;
                    if (seats[currentPlayer - 1] == PlayerType.AI)
                    {
                        ExecuteAIMove();
                    }
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
        /// Executed at the end of a turn.
        /// </summary>
        private static void EndOfTurn()
        {
            _gameState = GameState.endOfTurn;
            Thread.Sleep(waitPeriod);

            //applying the end of the turn
            if (takenCards != null && takenCards.Count > 0)
            {
                collected[currentPlayer - 1].Add(usedCard);
                for (int i = 0; i < takenCards.Count; ++i)
                {
                    collected[currentPlayer - 1].Add(takenCards[i]);
                    table.Remove(takenCards[i]);
                }
            }
            else
            {
                table.Add(usedCard);
            }

            currentPlayer++;
            if (currentPlayer > seats.Length)
                currentPlayer -= seats.Length;


            if (hands[currentPlayer - 1].Count == 0)
            {
                DealerTurn();
            }
            else if (seats[currentPlayer - 1] == PlayerType.AI)
            {
                _gameState = GameState.normalTurn;
                ExecuteAIMove();
            }
            else
            {
                _gameState = GameState.normalTurn;
            }
        }

        /// <summary>
        /// Runs 'EndOfTurn' method in another thread.
        /// </summary>
        private static void InitiateEndOfTurn()
        {
            if (AIThread != null && AIThread.IsAlive)
                StopAI();

            AIThread = new Thread(new ThreadStart(EndOfTurn));
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

        /// <summary>
        /// Find all combinations of the given cards. Useful for finding out what a player can collect from the table.
        /// </summary>
        /// <param name="cards">An array of cards.</param>
        /// <returns>An enumerable for all the combinations of cards in the given array.</returns>
        public static IEnumerable<Card[]> CardCombinations(Card[] cards)
        {
            return Enumerable
                .Range(1, (1 << (cards.Length)) - 1)
                .Select(index => cards
                .Where((v, i) => (index & (1 << i)) != 0)
                .ToArray());
        }

        /// <summary>
        /// Finds all possible moves for given card and list of cards on the table.
        /// </summary>
        /// <param name="card">Card to use.</param>
        /// <param name="tableCards">List of cards on the table.</param>
        /// <returns>List of possible casino moves for the given cards.</returns>
        public static List<CasinoMove> PossibleMoves(Card card, List<Card> tableCards)
        {

            List<CasinoMove> possibleMoves = new List<CasinoMove>();
            //forming all possible combinations from the table
            IEnumerable<Card[]> combinations = CardCombinations(tableCards.ToArray());

            bool usableCard = false;

            foreach (Card[] tableCombination in combinations)
            {
                int combinationSum = 0;
                for (int s = 0; s < tableCombination.Length; ++s)
                {
                    combinationSum += tableCombination[s].value;
                }

                bool combinationPossible = false;

                //special cards
                if (card.value == 1)
                {
                    if (combinationSum == 14)
                    {
                        combinationPossible = true;
                    }
                }
                else if (card.value == 2 && card.suit == Suit.spade)
                {
                    if (combinationSum == 15)
                    {
                        combinationPossible = true;
                    }
                }
                else if (card.value == 10 && card.suit == Suit.diamond)
                {
                    if (combinationSum == 16)
                    {
                        combinationPossible = true;
                    }
                }

                //normal cards
                else if (combinationSum == card.value)
                {
                    combinationPossible = true;
                }

                //checking if other cards can be taken as well
                if (combinationPossible)
                {
                    usableCard = true;
                    possibleMoves.Add(new CasinoMove(card, Utils.SortCards(new List<Card>(tableCombination))));

                    //finding out if some other cards can be taken as well
                    List<Card> remainingTableCards = new List<Card>();
                    for (int i = 0; i < tableCards.Count; ++i)
                    {
                        if (!tableCombination.Contains(tableCards[i]))
                        {
                            remainingTableCards.Add(tableCards[i]);
                        }
                    }

                    List<CasinoMove> comboMoves = PossibleMoves(card, remainingTableCards);
                    for (int i = 0; i < comboMoves.Count; ++i)
                    {
                        if (comboMoves[i].tableCards.Count == 0)
                        {
                            comboMoves.RemoveAt(i);
                            break;
                        }
                    }

                    if (comboMoves.Count > 0)
                    {
                        possibleMoves.RemoveAt(possibleMoves.Count - 1);
                        for (int i = 0; i < comboMoves.Count; ++i)
                        {
                            List<Card> allCards = new List<Card>();
                            for (int k = 0; k < tableCombination.Length; ++k)
                            {
                                allCards.Add(tableCombination[k]);
                            }
                            for (int k = 0; k < comboMoves[i].tableCards.Count; ++k)
                            {
                                allCards.Add(comboMoves[i].tableCards[k]);
                            }
                            possibleMoves.Add(new CasinoMove(card, Utils.SortCards(new List<Card>(allCards))));
                        }
                    }
                }
            }

            //simply throwing the card to the table
            if (!usableCard)
            {
                possibleMoves.Add(new CasinoMove(card, new List<Card>()));
            }

            return possibleMoves;
        }

        /// <summary>
        /// Finds all possible moves for the current player
        /// </summary>
        /// <returns>List of all possible moves for the current player as CasinoMoves</returns>
        public static List<CasinoMove> PossibleMoves()
        {
            List<CasinoMove> possibleMoves = new List<CasinoMove>();

            for (int c = 0; c < hands[currentPlayer - 1].Count; ++c)
            {
                Card card = hands[currentPlayer - 1][c];
                possibleMoves.AddRange(PossibleMoves(card, table));
            }

            return possibleMoves;
        }

        /// <summary>
        /// Finds all possible moves for the given hand cards and table
        /// </summary>
        /// <returns>List of all possible moves for the given parameters as CasinoMoves</returns>
        public static List<CasinoMove> PossibleMoves(List<Card> handCards, List<Card> tableCards)
        {
            List<CasinoMove> possibleMoves = new List<CasinoMove>();

            for (int c = 0; c < handCards.Count; ++c)
            {
                Card card = handCards[c];
                possibleMoves.AddRange(PossibleMoves(card, tableCards));
            }

            return possibleMoves;
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="card">Card used from the hand.</param>
        /// <param name="tableCards">List of possibly collected cards from the table.</param>
        private static void ApplyTurn(Card card, List<Card> tableCards)
        {
            if (_gameState == GameState.normalTurn)
            {
                usedCard = card;
                takenCards = tableCards;
                hands[currentPlayer - 1].Remove(usedCard);

                if (AIThread != null && AIThread.IsAlive)
                    EndOfTurn();
                else
                    InitiateEndOfTurn();
            }
        }
    }
}
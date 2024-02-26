using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game GOPS (Game Of Pure Strategy). Rules:</para>
    /// <para>Each player has cards from 1 to 13 that work as bids for a silent auction. Suits do not matter.
    /// The players place hidden "bids" of a prize card, and the highest bid wins the card.</para>
    /// <para>There are a total of 13 prize cards (from 1 to 13) and the current card is randomly selected from the remaining cards.</para>
    /// <para>In case of a tie, the value of the prize is divided evenly among the highest bidders.
    /// After all 13 cards have been auctioned on, the player with the highest amount of points wins.</para>
    /// </summary>
    public static class GOPSEngine
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
            /// It is a player's turn.
            /// </summary>
            normalTurn = 1,

            /// <summary>
            /// The game is between turns.
            /// </summary>
            endOfTurn = 2
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

        private static PlayerType[] _seats;
        /// <summary>
        /// Number of players. From 2 to 6
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

        private static List<Card>[] hands;
        /// <summary>
        /// List of hand cards for each player.
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

        private static List<Card> table;
        /// <summary>
        /// List of cards on the table. The first card in the list is the current prize.
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

        private static float[] points;
        /// <summary>
        /// Array of points for each player.
        /// </summary>
        public static float[] Points
        {
            get
            {
                if (points == null)
                    ResetGame();
                return points;
            }
        }

        private static int currentPlayer;
        /// <summary>
        /// The current player's number. Ranges from 1 to length of 'seats'.
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

        private static Card[] bids;
        /// <summary>
        /// Array of bids from all players for the current prize.
        /// </summary>
        public static Card[] Bids
        {
            get
            {
                if (bids == null)
                    ResetGame();
                return bids;
            }
        }

        private static GameState _gameState = GameState.normalTurn;
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
        /// time to wait between rounds in milliseconds
        /// </summary>
        public static int waitPeriod = 5000;

        //private variables
        private static Thread AIThread;
        private static Card[] previousBids;
        private static Card previousPrize;

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame()
        {
            StopAI();

            _gameState = GameState.normalTurn;

            hands = new List<Card>[seats.Length];
            points = new float[seats.Length];
            table = new List<Card>();
            currentPlayer = 1;
            bids = new Card[seats.Length];
            previousBids = new Card[seats.Length];
            previousPrize = new Card(Suit.spade, 1);

            //default bids
            for (int i = 0; i < bids.Length; ++i)
            {
                bids[i] = new Card(Suit.spade, 1);
                previousBids[i] = new Card(Suit.spade, 1);
            }

            //populating the table
            for (int i = 0; i < 13; ++i)
            {
                table.Add(new Card(Suit.spade, (ushort)(i + 1)));
            }

            table = Utils.ShuffleCards(table);

            //dealing to players
            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                points[i] = 0;

                for (int k = 0; k < 13; ++k)
                {
                    hands[i].Add(new Card((Suit)(i % 3 + 1), (ushort)(k+1)));
                }

                hands[i] = Utils.SortCards(hands[i]);
            }

            _gameState = GameState.normalTurn;

            if (seats[currentPlayer - 1] == PlayerType.AI)
                AITurn();
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
            bool validMove = false;
            for (int i = 0; i < hands[currentPlayer-1].Count; ++i)
            {
                if (hands[currentPlayer-1][i] == card)
                {
                    validMove = true;
                    break;
                }
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.local && _gameState == GameState.normalTurn)
            {
                ApplyTurn(card);
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="card">The used hand card.</param>
        public static void CustomMove(Card card)
        {
            bool validMove = false;
            for (int i = 0; i < hands[currentPlayer-1].Count; ++i)
            {
                if (hands[currentPlayer-1][i] == card)
                {
                    validMove = true;
                    break;
                }
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.custom && _gameState == GameState.normalTurn)
            {
                ApplyTurn(card);
            }
        }

        /// <summary>
        /// Gives the GOPSAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            Card aiMove = GOPSAI.MakeMove(hands[currentPlayer-1], table[0], previousPrize, previousBids, currentPlayer-1);

            System.TimeSpan calcTime = System.DateTime.UtcNow - startTime;
            if (calcTime.TotalMilliseconds < waitPeriod/5)
                Thread.Sleep(waitPeriod/5 - (int)(calcTime.TotalMilliseconds));

            ApplyTurn(aiMove);
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
        /// Executed at the end of a turn.
        /// </summary>
        private static void EndOfTurn()
        {
            _gameState = GameState.endOfTurn;

            //dividing points
            int highestBid = 0;
            List<int> highestBidders = new List<int>();
            for (int i = 0; i < bids.Length; ++i)
            {
                if (bids[i].value > highestBid)
                {
                    highestBid = bids[i].value;
                    highestBidders = new List<int>() { i };
                }
                else if (bids[i].value == highestBid)
                {
                    highestBidders.Add(i);
                }
            }

            for (int i = 0; i < highestBidders.Count; ++i)
            {
                points[highestBidders[i]] += ((float)table[0].value) / ((float)highestBidders.Count);
            }


            Thread.Sleep(waitPeriod);

            //resetting bids
            previousBids = Utils.IndependentCopyArray(bids);
            previousPrize = table[0];
            for (int i = 0; i < bids.Length; ++i)
            {
                bids[i] = new Card(Suit.spade, 1);
            }

            table.RemoveAt(0);

            if (table.Count > 0)
            {
                currentPlayer = 1;

                _gameState = GameState.normalTurn;

                if (seats[currentPlayer - 1] == PlayerType.AI)
                {
                    if (AIThread != null && AIThread.IsAlive)
                        ExecuteAIMove();
                    else
                        AITurn();
                }
            }
            else
            {
                //game end
                _gameState = GameState.inactive;
            }
        }

        /// <summary>
        /// Runs 'EndOfTurn' method in another thread.
        /// </summary>
        private static void StartEndOfTurn()
        {
            if (AIThread != null && AIThread.IsAlive)
                StopAI();

            AIThread = new Thread(new ThreadStart(EndOfTurn));
            AIThread.Start();
            WaitForJobDone(AIThread);
        }

        /// <summary>
        /// Finds all possible moves for the current player
        /// </summary>
        /// <returns>List of all possible moves for the current player as Cards</returns>
        public static List<Card> PossibleMoves()
        {
            List<Card> possibleMoves = PossibleMoves(hands[currentPlayer - 1]);

            return possibleMoves;
        }

        /// <summary>
        /// Finds all possible moves for the given hand cards
        /// </summary>
        /// <returns>List of all possible moves for the given parameters as GOPSMoves</returns>
        public static List<Card> PossibleMoves(List<Card> handCards)
        {
            return handCards;
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="move">Card used from the hand.</param>
        private static void ApplyTurn(Card move)
        {
            bids[currentPlayer-1] = move;
            hands[currentPlayer - 1].Remove(move);

            if (currentPlayer == seats.Length)
            {
                if (AIThread != null && AIThread.IsAlive)
                    EndOfTurn();
                else
                    StartEndOfTurn();
            }
            else
            {
                currentPlayer++;

                if (seats[currentPlayer - 1] == PlayerType.AI)
                {
                    if (AIThread != null && AIThread.IsAlive)
                        ExecuteAIMove();
                    else
                        AITurn();
                }
            }
        }
    }
}
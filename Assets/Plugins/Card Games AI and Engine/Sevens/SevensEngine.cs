using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game Sevens. Rules:</para>
    /// <para>The deck is divided among all players. Some players may get one card more or less than others.
    /// The player with the seven of hearts starts by placing the card on the table.</para>
    /// <para>Players take clockwise turns in either placing a card on the table or passing their turn.
    /// Passing is only allowed if the player cannot play any card.
    /// Each suit is played separately.</para>
    /// <para>Card with a face value of 7 is always playable.
    /// Cards with a face value of less than 7 is only playable if the card with one higher face value has been played.
    /// Cards with a face value of more than 7 is only playable if the card with one smaller face value has been played.</para>
    /// <para>The winner is the player who plays their last card first.</para>
    /// </summary>
    public static class SevensEngine
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
            /// It is a player's turn to play a card
            /// </summary>
            normalTurn = 1,

            /// <summary>
            /// A turn was passed
            /// </summary>
            passedTurn = 2
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

        private static List<Card>[] hands;
        /// <summary>
        /// Lists of all players' hand cards.
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
            table = new List<Card>();
            currentPlayer = 1;

            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
            }

            Deck deck = new Deck();
            int dealtPlayer = 0;
            while (deck.Cards.Count > 0)
            {
                Card card = deck.Deal();
                hands[dealtPlayer].Add(card);

                if (card.suit == Suit.heart && card.value == 7)
                    currentPlayer = dealtPlayer + 1;

                dealtPlayer++;
                if (dealtPlayer >= hands.Length)
                    dealtPlayer = 0;
            }

            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = Utils.SortCards(hands[i]);
            }

            _gameState = GameState.normalTurn;

            if (seats[currentPlayer-1] == PlayerType.AI)
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
        /// Passing is done by "playing" a heart of sevens.
        /// </summary>
        /// <param name="card">The card used by the player.</param>
        public static void LocalMove(Card card)
        {
            bool validMove = false;
            List<Card> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i] == card)
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
        /// Passing is done by "playing" a heart of sevens.
        /// </summary>
        /// <param name="card">The card used by the player.</param>
        public static void CustomMove(Card card)
        {
            bool validMove = false;
            List<Card> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i] == card)
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
        /// Gives the SevensAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            Card aiMove = SevensAI.MakeMove(hands[currentPlayer-1], table);

            System.TimeSpan calcTime = System.DateTime.UtcNow - startTime;
            if (calcTime.TotalMilliseconds < waitPeriod)
                Thread.Sleep(waitPeriod - (int)(calcTime.TotalMilliseconds));

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
        /// Passes the current player's turn.
        /// </summary>
        private static void PassTurn()
        {
            Thread.Sleep(waitPeriod);

            _gameState = GameState.normalTurn;

            currentPlayer++;
            if (currentPlayer > seats.Length)
                currentPlayer = 1;

            if (seats[currentPlayer - 1] == PlayerType.AI)
            {
                if (AIThread != null && AIThread.IsAlive)
                    ExecuteAIMove();
                else
                    AITurn();
            }
        }

        /// <summary>
        /// Executes 'PassTurn' in another thread.
        /// </summary>
        private static void StartPassTurn()
        {
            if (AIThread != null && AIThread.IsAlive)
                StopAI();

            AIThread = new Thread(new ThreadStart(PassTurn));
            AIThread.Start();
            WaitForJobDone(AIThread);
        }

        /// <summary>
        /// Finds all possible moves for the current player
        /// </summary>
        /// <returns>List of all possible moves for the current player as Cards</returns>
        public static List<Card> PossibleMoves()
        {
            List<Card> possibleMoves = PossibleMoves(hands[currentPlayer - 1], table);

            return possibleMoves;
        }

        /// <summary>
        /// Finds all possible moves for the given hand cards and table
        /// </summary>
        /// <returns>List of all possible moves for the given parameters as Cards</returns>
        public static List<Card> PossibleMoves(List<Card> handCards, List<Card> tableCards)
        {
            List<Card> possibleMoves = new List<Card>();

            for (int i = 0; i < handCards.Count; ++i)
            {
                if (handCards[i].value == 7)
                {
                    bool hearts7Played = false;
                    for (int k = 0; k < tableCards.Count; ++k)
                    {
                        if (tableCards[k].suit == Suit.heart && tableCards[k].value == 7)
                        {
                            hearts7Played = true;
                            break;
                        }
                    }

                    if (hearts7Played || handCards[i].suit == Suit.heart)
                        possibleMoves.Add(handCards[i]);
                }
                else if (handCards[i].value < 7)
                {
                    bool higherCardPlayed = false;

                    for (int k = 0; k < tableCards.Count; ++k)
                    {
                        if (tableCards[k].suit == handCards[i].suit && tableCards[k].value == handCards[i].value + 1)
                        {
                            higherCardPlayed = true;
                            break;
                        }
                    }

                    if (higherCardPlayed)
                        possibleMoves.Add(handCards[i]);
                }
                else if (handCards[i].value > 7)
                {
                    bool smallerCardPlayed = false;

                    for (int k = 0; k < tableCards.Count; ++k)
                    {
                        if (tableCards[k].suit == handCards[i].suit && tableCards[k].value == handCards[i].value -1)
                        {
                            smallerCardPlayed = true;
                            break;
                        }
                    }

                    if (smallerCardPlayed)
                        possibleMoves.Add(handCards[i]);
                }
            }

            //no moves available. allowing "pass" aka. playing an imaginary 7 of hearts
            if (possibleMoves.Count == 0)
            {
                possibleMoves.Add(new Card(Suit.heart, 7));
            }

            return possibleMoves;
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="card">Card played by a player.</param>
        private static void ApplyTurn(Card card)
        {
            if (_gameState == GameState.normalTurn)
            {
                if (table.Count == 0 || card.value != 7 || card.suit != Suit.heart)
                {
                    table.Add(card);
                    hands[currentPlayer - 1].Remove(card);

                    if (hands[currentPlayer - 1].Count == 0)
                    {
                        //current player won
                        _gameState = GameState.inactive;
                    }
                }
                else
                {
                    _gameState = GameState.passedTurn;
                }

                if (_gameState == GameState.passedTurn)
                {
                    if (AIThread != null && AIThread.IsAlive)
                        PassTurn();
                    else
                        StartPassTurn();
                }
                else if (_gameState == GameState.normalTurn)
                {
                    currentPlayer++;
                    if (currentPlayer > seats.Length)
                        currentPlayer = 1;

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
}
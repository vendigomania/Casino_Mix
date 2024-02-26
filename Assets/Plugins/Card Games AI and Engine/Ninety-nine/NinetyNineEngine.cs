using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game Ninety Nine. Rules:</para>
    /// <para>Each player is dealt 3 cards, and take turns in playing a card to the table.
    /// After playing a card, the player gets a new card.</para>
    /// <para>Only the total value of the table cards is relevant, and suits do not matter.
    /// If a player makes the table's total value go to over 99, that player loses a token.</para>
    /// <para>Each player has three tokens at the start of the game, and drops out when the last token is lost.
    /// Winner is the last person in the game.</para>
    /// <para>Some cards have values that are different than the face value of the card:
    /// Ace is either 1 or 11, decided by the player.
    /// 3 has the value 3 but the next player loses their turn.
    /// 4 has a value of 0 and the order of play is reversed. If there are only two players, the player gets another turn instead.
    /// 9 changes the total value of the table cards to 99 regardless of what it was previously.
    /// 10 is either -10 or +10, decided by the player.
    /// 11 and 12 have a value of 10.
    /// 13 has a value of 0.</para>
    /// </summary>
    public static class NinetyNineEngine
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
            /// Cards are being dealt to players
            /// </summary>
            cardDealing = 2,

            /// <summary>
            /// It is the end of a player's turn
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
        /// A class to represent a move in Ninety Nine.
        /// A move consists of a card and its value.
        /// </summary>
        public class NinetyNineMove
        {
            public NinetyNineMove(Card _card, int _value)
            {
                card = _card;
                value = _value;
            }

            public readonly Card card;
            public readonly int value;
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
        /// Lists of cards in each player's hand.
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

        private static int table;
        /// <summary>
        /// The value of the pile of cards on the table.
        /// </summary>
        public static int Table
        {
            get
            {
                return table;
            }
        }

        private static int[] tokens;
        /// <summary>
        /// Number of tokens left for each player.
        /// </summary>
        public static int[] Tokens
        {
            get
            {
                if (tokens == null)
                    ResetGame();
                return tokens;
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

        private static bool clockwisePlay;
        /// <summary>
        /// Boolean specifying if the game is currently played clockwise. Can be changed by playing a four of any suit.
        /// </summary>
        public static bool ClockwisePlay
        {
            get
            {
                return clockwisePlay;
            }
        }

        private static NinetyNineMove usedMove;
        /// <summary>
        /// The card used most recently
        /// </summary>
        public static NinetyNineMove UsedMove
        {
            get
            {
                if (usedMove == null)
                    ResetGame();
                return usedMove;
            }
        }

        //private variables
        private static Thread AIThread;
        private static Deck deck;

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

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame()
        {
            StopAI();

            _gameState = GameState.cardDealing;

            hands = new List<Card>[seats.Length];
            tokens = new int[seats.Length];
            clockwisePlay = true;
            table = 0;
            deck = new Deck();
            currentPlayer = 1;

            //dealing to players
            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                tokens[i] = 3;

                hands[i].Add(deck.Deal());
                hands[i].Add(deck.Deal());
                hands[i].Add(deck.Deal());

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

        public static void LocalMove(Card card, int value)
        {
            LocalMove(new NinetyNineMove(card, value));
        }

        /// <summary>
        /// Executes the given move if the current player is a local player.
        /// </summary>
        /// <param name="move">The move by the player.</param>
        public static void LocalMove(NinetyNineMove move)
        {
            bool validMove = false;
            List<NinetyNineMove> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i].card == move.card && possibleMoves[i].value == move.value)
                {
                    validMove = true;
                    break;
                }
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.local && _gameState == GameState.normalTurn)
            {
                ApplyTurn(move);
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="move">The move by the player.</param>
        public static void CustomMove(NinetyNineMove move)
        {
            bool validMove = false;
            List<NinetyNineMove> possibleMoves = PossibleMoves();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i].card == move.card && possibleMoves[i].value == move.value)
                {
                    validMove = true;
                    break;
                }
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.custom && _gameState == GameState.normalTurn)
            {
                ApplyTurn(move);
            }
        }

        /// <summary>
        /// Gives the NinetyNineAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            NinetyNineMove aiMove = NinetyNineAI.MakeMove(hands[currentPlayer-1], table);

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
        /// It is the end of a turn.
        /// </summary>
        private static void EndOfTurn()
        {
            _gameState = GameState.endOfTurn;

            Thread.Sleep(waitPeriod);

            hands[currentPlayer - 1].Add(deck.Deal());

            if (usedMove.card.value != 9)
                table += usedMove.value;
            else
                table = 99;

            //new round
            if (table > 99)
            {
                clockwisePlay = true;

                tokens[currentPlayer - 1]--;

                if (tokens[currentPlayer - 1] < 1)
                    NextPlayer(false);

                //checking for game end
                int playersInGame = 0;
                for (int i = 0; i < tokens.Length; ++i)
                {
                    if (tokens[i] > 0)
                        playersInGame++;
                }

                //game ends
                if (playersInGame < 2)
                {
                    _gameState = GameState.inactive;
                }
                else
                {
                    _gameState = GameState.cardDealing;

                    Thread.Sleep(waitPeriod);

                    for (int i = 0; i < hands.Length; ++i)
                    {
                        hands[i] = new List<Card>();
                    }
                    clockwisePlay = true;
                    table = 0;
                    deck = new Deck();

                    //dealing to players
                    for (int i = currentPlayer; i < seats.Length + currentPlayer; ++i)
                    {
                        if (tokens[(i - 1) % seats.Length] > 0)
                        {
                            Thread.Sleep(waitPeriod / 2);
                            hands[(i - 1) % seats.Length].Add(deck.Deal());
                            hands[(i - 1) % seats.Length].Add(deck.Deal());
                            hands[(i - 1) % seats.Length].Add(deck.Deal());
                        }
                    }

                    Thread.Sleep(waitPeriod/2);

                    _gameState = GameState.normalTurn;
                }
            }
            else
            {
                if (usedMove.card.value == 4)
                    clockwisePlay = !clockwisePlay;

                //if there's only 2 players in game, playing a 4 means another turn
                
                int playersInGame = 0;
                for (int i = 0; i < tokens.Length; ++i)
                {
                    if (tokens[i] > 0)
                        playersInGame++;
                }

                if (playersInGame != 2 || usedMove.card.value != 4)
                    NextPlayer(usedMove.card.value == 3);

                _gameState = GameState.normalTurn;

            }

            if (_gameState == GameState.normalTurn && seats[currentPlayer - 1] == PlayerType.AI)
            {
                if (AIThread != null && AIThread.IsAlive)
                    ExecuteAIMove();
                else
                    AITurn();
            }
        }

        /// <summary>
        /// Changes the turn to be the next player's turn.
        /// </summary>
        /// <param name="skipOne">Skip one player.</param>
        private static void NextPlayer(bool skipOne)
        {
            if (clockwisePlay)
            {
                currentPlayer++;
                if (currentPlayer > seats.Length)
                    currentPlayer = 1;
            }
            else
            {
                currentPlayer--;
                if (currentPlayer < 1)
                    currentPlayer = seats.Length;
            }

            if (tokens[currentPlayer - 1] < 1)
                NextPlayer(skipOne);

            if (skipOne)
                NextPlayer(false);
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
        public static List<NinetyNineMove> PossibleMoves()
        {
            List<NinetyNineMove> possibleMoves = PossibleMoves(hands[currentPlayer - 1]);

            return possibleMoves;
        }

        /// <summary>
        /// Finds all possible moves for the given hand cards
        /// </summary>
        /// <returns>List of all possible moves for the given parameters as NinetyNineMoves</returns>
        public static List<NinetyNineMove> PossibleMoves(List<Card> handCards)
        {
            List<NinetyNineMove> possibleMoves = new List<NinetyNineMove>();

            for (int i = 0; i < handCards.Count; ++i)
            {
                if (handCards[i].value == 1)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 1));
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 11));
                }
                else if (handCards[i].value == 4)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 0));
                }
                else if (handCards[i].value == 9)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 99));
                }
                else if (handCards[i].value == 10)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], -10));
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 10));
                }
                else if (handCards[i].value == 11 || handCards[i].value == 12)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 10));
                }
                else if (handCards[i].value == 13)
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], 0));
                }
                else
                {
                    possibleMoves.Add(new NinetyNineMove(handCards[i], handCards[i].value));
                }
            }

            return possibleMoves;
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="move">Move by a player.</param>
        private static void ApplyTurn(NinetyNineMove move)
        {
            usedMove = move;
            hands[currentPlayer - 1].Remove(move.card);

            if (AIThread != null && AIThread.IsAlive)
                EndOfTurn();
            else
                StartEndOfTurn();
        }
    }
}
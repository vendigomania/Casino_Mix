using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGAIE
{
    /// <summary>
    /// <para>Engine for the card game Blackjack. Rules:</para>
    /// <para>Each player is dealt two cards at the start and plays only against the house and not each other. 
    /// Players have 5 different actions to take: Stand, Hit, Double Down, Split or Surrender.
    /// A player's turn continues until the player is standing, surrendered or their hand cards' total value is over 21.</para>
    /// <para>The value of the hand is calculated so that ace is either 1 or 11 depending which is more beneficial to the player,
    /// cards over 10 have a value of 10 and every other card has their face value.</para>
    /// <para>Stand means keeping the current cards.
    /// Hit means getting a new card.
    /// Double down means only getting one more card, but doubling the wager, and is only allowed on the first turn.
    /// Split means splitting the cards into two new hands, both getting their own new card and wager, and is only allowed on the first turn if both cards have the same value.
    /// Surrender means forfeiting the round and getting half of the wager back.</para>
    /// <para>After all players are done, house keeps dealing itself cards until the total is over 16. 
    /// For all players standing with a total value of less than 22, if the house's value is lower than the player's or over 21, the player is paid according to their wager.
    /// Having the same value as the house means getting the wager back as is.</para>
    /// </summary>
    public static class BlackjackEngine
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
            /// The dealer is dealing cards
            /// </summary>
            cardDealing = 2,

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
        /// Enumeration to represent a move in Blackjack.
        /// </summary>
        public enum BlackjackMove
        {
            /// <summary>
            /// Keep the current total.
            /// </summary>
            stand = 0,
            /// <summary>
            /// Get a new card.
            /// </summary>
            hit = 1,
            /// <summary>
            /// Get only one new card, but double the wager.
            /// </summary>
            doubleDown = 2,
            /// <summary>
            /// Split the cards into two new hands.
            /// </summary>
            split = 3,
            /// <summary>
            /// Forfeit the round and get half of your wager back.
            /// </summary>
            surrender = 4
        }

        public enum HandState
        {
            /// <summary>
            /// The hand is freshly dealt.
            /// </summary>
            fresh = 0,
            /// <summary>
            /// The hand was just doubled.
            /// </summary>
            doubled = 1,
            /// <summary>
            /// The hand was just split.
            /// </summary>
            split = 2,
            /// <summary>
            /// The hand has surrendered.
            /// </summary>
            surrendered = 3,
            /// <summary>
            /// The hand is standing.
            /// </summary>
            standing = 4,
            /// <summary>
            /// The hand is playing
            /// </summary>
            playing = 5,
            /// <summary>
            /// The hand is bust (value over 21).
            /// </summary>
            bust = 6
        }

        private static PlayerType[] _seats;
        /// <summary>
        /// Number of players. From 1 to 6
        /// </summary>
        public static PlayerType[] seats
        {
            get
            {
                return _seats;
            }
            set
            {
                if (value != null && value.Length < 1)
                {
                    _seats = new PlayerType[1] { PlayerType.local };
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
        /// The players' hand cards.
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

        private static List<Card>[] secondHands;
        /// <summary>
        /// The players' cards on the "secondary" split hand.
        /// </summary>
        public static List<Card>[] SecondHands
        {
            get
            {
                if (secondHands == null)
                    ResetGame();
                return secondHands;
            }
        }

        private static List<Card> table;
        /// <summary>
        /// The house's cards
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

        private static int[] chips;
        /// <summary>
        /// Each player's chips.
        /// </summary>
        public static int[] Chips
        {
            get
            {
                if (chips == null)
                    ResetGame();
                return chips;
            }
        }

        private static int currentPlayer;
        /// <summary>
        /// The current player, ranges from 1 to length of 'seats'.
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

        private static int[] bets;
        /// <summary>
        /// The wager for this round for each player (on their main hand).
        /// </summary>
        public static int[] Bets
        {
            get
            {
                if (bets == null)
                    ResetGame();
                return bets;
            }
        }

        private static int[] secondaryBets;
        /// <summary>
        /// The wager for this round for each player on the secondary, split, hand.
        /// </summary>
        public static int[] SecondaryBets
        {
            get
            {
                if (secondaryBets == null)
                    ResetGame();
                return secondaryBets;
            }
        }

        private static HandState[] handStates;
        /// <summary>
        /// State of the player's (main) hand.
        /// </summary>
        public static HandState[] HandStates
        {
            get
            {
                if (handStates == null)
                    ResetGame();
                return handStates;
            }
        }

        private static HandState[] secondHandStates;
        /// <summary>
        /// State of the player's secondary, split, hand.
        /// </summary>
        public static HandState[] SecondHandStates
        {
            get
            {
                if (secondHandStates == null)
                    ResetGame();
                return secondHandStates;
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

        /// <summary>
        /// The base wager.
        /// </summary>
        public static int wager = 2;

        /// <summary>
        /// The amount of chips every player starts with.
        /// </summary>
        public static int startingChips = 10;

        //private variables
        private static Thread AIThread;
        private static Deck deck;
        private static BlackjackMove currentPlayerMove;

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame(bool resetChips = true)
        {
            //if resetchips is true, this was called by the user and not engine. stopping ai thread
            if (resetChips)
                StopAI();

            _gameState = GameState.normalTurn;

            hands = new List<Card>[seats.Length];
            if (resetChips)
                chips = new int[seats.Length];
            table = new List<Card>();
            currentPlayer = 1;
            bets = new int[seats.Length];
            deck = new Deck();
            currentPlayerMove = BlackjackMove.stand;
            handStates = new HandState[seats.Length];
            bets = new int[seats.Length];
            secondHands = new List<Card>[seats.Length];
            secondHandStates = new HandState[seats.Length];
            secondaryBets = new int[seats.Length];

            //default bets
            for (int i = 0; i < bets.Length; ++i)
            {
                bets[i] = wager;
            }

            //dealing the upcard to the table
            table.Add(deck.Deal());
            

            //dealing to players
            for (int i = 0; i < seats.Length; ++i)
            {
                hands[i] = new List<Card>();
                if (resetChips)
                    chips[i] = startingChips;
                chips[i] -= wager;

                hands[i].Add(deck.Deal());
                hands[i].Add(deck.Deal());
                handStates[i] = HandState.fresh;
                bets[i] = wager;

                secondHands[i] = new List<Card>();
                secondHandStates[i] = HandState.surrendered;
                secondaryBets[i] = 0;
            }

            _gameState = GameState.normalTurn;

            if (seats[currentPlayer - 1] == PlayerType.AI)
            {
                if (AIThread != null && AIThread.IsAlive)
                    ExecuteAIMove();
                else
                    AITurn();
            }
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
        /// <param name="move">The Blackjackmove you want to execute.</param>
        public static void LocalMove(BlackjackMove move)
        {
            bool mainHand = true;
            if (handStates[currentPlayer - 1] == HandState.bust || handStates[currentPlayer - 1] == HandState.standing)
                mainHand = false;

            bool validMove = false;
            if (PossibleMoves(mainHand).Contains(move))
                validMove = true;

            if (validMove && seats[currentPlayer - 1] == PlayerType.local && _gameState == GameState.normalTurn)
            {
                ApplyTurn(move);
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="move">The Blackjackmove you want to execute.</param>
        public static void CustomMove(BlackjackMove move)
        {
            bool mainHand = true;
            if (handStates[currentPlayer - 1] == HandState.bust || handStates[currentPlayer - 1] == HandState.standing)
                mainHand = false;

            bool validMove = false;
            if (PossibleMoves(mainHand).Contains(move))
                validMove = true;

            if (validMove && seats[currentPlayer - 1] == PlayerType.custom && _gameState == GameState.normalTurn)
            {
                ApplyTurn(move);
            }
        }

        /// <summary>
        /// Gives the BlackjackAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            BlackjackMove aiMove;
            if (PossibleMoves().Count > 0)
                aiMove = BlackjackAI.MakeMove(hands[currentPlayer - 1], handStates[currentPlayer - 1], table[0]);
            else
                aiMove = BlackjackAI.MakeMove(secondHands[currentPlayer - 1], secondHandStates[currentPlayer - 1], table[0]);

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
        /// Executed after a move is made.
        /// </summary>
        private static void EndOfTurn()
        {
            bool mainHand = true;
            if (PossibleMoves().Count <= 0)
                mainHand = false;
            switch (currentPlayerMove)
            {
                case BlackjackMove.surrender:
                    chips[currentPlayer - 1] += wager / 2;
                    if (mainHand)
                        handStates[currentPlayer - 1] = HandState.surrendered;
                    else
                        secondHandStates[currentPlayer - 1] = HandState.surrendered;
                    break;

                case BlackjackMove.stand:
                    if (mainHand)
                        handStates[currentPlayer - 1] = HandState.standing;
                    else
                        secondHandStates[currentPlayer - 1] = HandState.standing;
                    break;

                case BlackjackMove.hit:
                    _gameState = GameState.cardDealing;
                    if (mainHand)
                    {
                        hands[currentPlayer - 1].Add(deck.Deal());
                        Thread.Sleep(waitPeriod * 2);
                        if (HandTotalValue(hands[currentPlayer - 1]) == 21)
                        {
                            handStates[currentPlayer - 1] = HandState.standing;
                        }
                        else if (HandTotalValue(hands[currentPlayer - 1]) > 21)
                        {
                            handStates[currentPlayer - 1] = HandState.bust;
                        }
                        else
                        {
                            handStates[currentPlayer - 1] = HandState.playing;
                        }
                    }
                    else
                    {
                        secondHands[currentPlayer - 1].Add(deck.Deal());
                        Thread.Sleep(waitPeriod * 2);
                        if (HandTotalValue(secondHands[currentPlayer - 1]) == 21)
                        {
                            secondHandStates[currentPlayer - 1] = HandState.standing;
                        }
                        else if (HandTotalValue(secondHands[currentPlayer - 1]) > 21)
                        {
                            secondHandStates[currentPlayer - 1] = HandState.bust;
                        }
                        else
                        {
                            secondHandStates[currentPlayer - 1] = HandState.playing;
                        }
                    }

                    _gameState = GameState.normalTurn;
                    break;

                case BlackjackMove.doubleDown:
                    _gameState = GameState.cardDealing;
                    if (mainHand)
                    {
                        handStates[currentPlayer - 1] = HandState.doubled;
                        bets[currentPlayer - 1] += wager;
                        chips[currentPlayer - 1] -= wager;
                        hands[currentPlayer - 1].Add(deck.Deal());
                        Thread.Sleep(waitPeriod*2);
                        if (HandTotalValue(hands[currentPlayer - 1]) > 21)
                        {
                            handStates[currentPlayer - 1] = HandState.bust;
                        }
                        else
                        {
                            handStates[currentPlayer - 1] = HandState.standing;
                        }
                    }
                    else
                    {
                        secondHandStates[currentPlayer - 1] = HandState.doubled;
                        secondaryBets[currentPlayer - 1] += wager;
                        chips[currentPlayer - 1] -= wager;
                        secondHands[currentPlayer - 1].Add(deck.Deal());
                        Thread.Sleep(waitPeriod*2);
                        if (HandTotalValue(secondHands[currentPlayer - 1]) > 21)
                        {
                            secondHandStates[currentPlayer - 1] = HandState.bust;
                        }
                        else
                        {
                            secondHandStates[currentPlayer - 1] = HandState.standing;
                        }
                    }

                    _gameState = GameState.normalTurn;
                    break;

                case BlackjackMove.split:
                    _gameState = GameState.cardDealing;
                    secondHands[currentPlayer - 1] = new List<Card>() { hands[currentPlayer - 1][1], deck.Deal() };
                    hands[currentPlayer - 1] = new List<Card>{hands[currentPlayer - 1][0], deck.Deal()};

                    handStates[currentPlayer - 1] = HandState.split;
                    secondHandStates[currentPlayer - 1] = HandState.split;

                    secondaryBets[currentPlayer - 1] = wager;
                    chips[currentPlayer - 1] -= wager;

                    Thread.Sleep(waitPeriod*2);
                    _gameState = GameState.normalTurn;
                    break;
            }

            bool nextPlayer = false;
            if (mainHand && (handStates[currentPlayer - 1] == HandState.standing || handStates[currentPlayer - 1] == HandState.bust || handStates[currentPlayer - 1] == HandState.surrendered))
            {
                //see if there's a 2nd hand from splitting
                if (secondHands[currentPlayer - 1] == null || secondHands[currentPlayer-1].Count < 1)
                {
                    nextPlayer = true;
                }
            }
            else if (!mainHand && (secondHandStates[currentPlayer - 1] == HandState.standing || secondHandStates[currentPlayer - 1] == HandState.bust || secondHandStates[currentPlayer - 1] == HandState.surrendered))
            {
                nextPlayer = true;
            }

            if (nextPlayer)
            {
                _gameState = GameState.endOfTurn;
                Thread.Sleep(waitPeriod*2);

                currentPlayer++;
                if (currentPlayer == seats.Length + 1)
                {
                    currentPlayer = currentPlayer = seats.Length;

                    _gameState = GameState.cardDealing;
                    //round end. dealer reveals their hand (aka. deals one card to table)
                    table.Add(deck.Deal());

                    int dealerTotal = HandTotalValue(table);
                    while (dealerTotal < 17)
                    {
                        Thread.Sleep(waitPeriod*2);
                        table.Add(deck.Deal());
                        dealerTotal = HandTotalValue(table);
                    }
                    _gameState = GameState.endOfTurn;
                    Thread.Sleep(waitPeriod*5);

                    //dividing points
                    for (int i = 0; i < seats.Length; ++i)
                    {
                        //main hand
                        if (handStates[i] == HandState.standing)
                        { 
                            if (dealerTotal < HandTotalValue(hands[i]) || dealerTotal > 21)
                            chips[i] += 2 * bets[i];
                            else if (dealerTotal == HandTotalValue(hands[i]))
                                chips[i] += bets[i];
                        }

                        //secondary hand if split
                        if (secondHandStates[i] == HandState.standing)
                        {
                            if (dealerTotal < HandTotalValue(secondHands[i]) || dealerTotal > 21)
                                chips[i] += 2 * secondaryBets[i];
                            else if (dealerTotal == HandTotalValue(secondHands[i]))
                                chips[i] += secondaryBets[i];
                        }
                    }

                    //preparing next round
                    ResetGame(false);
                    _gameState = GameState.normalTurn;
                }
                else
                {
                    _gameState = GameState.normalTurn;
                    if (seats[currentPlayer - 1] == PlayerType.AI)
                    {
                        if (AIThread != null && AIThread.IsAlive)
                            ExecuteAIMove();
                        else
                            AITurn();
                    }
                }
            }
            else
            {
                _gameState = GameState.normalTurn;
                if (seats[currentPlayer - 1] == PlayerType.AI)
                {
                    if (AIThread != null && AIThread.IsAlive)
                        ExecuteAIMove();
                    else
                        AITurn();
                }
            }

        }

        /// <summary>
        /// Starts the 'EndOfTurn' method in another thread.
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
        /// Finds all possible moves for the current player.
        /// </summary>
        /// <returns>List of all possible moves for the current player as BlackJackMove.</returns>
        public static List<BlackjackMove> PossibleMoves(bool mainHand = true)
        {
            if (mainHand)
                return PossibleMoves(hands[currentPlayer - 1], handStates[currentPlayer-1]);
            else
                return PossibleMoves(secondHands[currentPlayer - 1], secondHandStates[currentPlayer - 1]);
        }

        /// <summary>
        /// Finds all possible options for the given hand cards.
        /// </summary>
        /// <returns>List of all possible moves for the given parameters as BlackjackMoves.</returns>
        public static List<BlackjackMove> PossibleMoves(List<Card> handCards, HandState handState)
        {
            List<BlackjackMove> possibleMoves = new List<BlackjackMove>();

                switch (handState)
                {
                    case HandState.fresh:
                        possibleMoves.Add(BlackjackMove.stand);
                    if (HandTotalValue(handCards) < 21)
                    {
                        possibleMoves.Add(BlackjackMove.surrender);
                        possibleMoves.Add(BlackjackMove.hit);
                        possibleMoves.Add(BlackjackMove.doubleDown);

                        if (handCards.Count == 2 && handCards[0].value == handCards[1].value)
                            possibleMoves.Add(BlackjackMove.split);
                    }
                        break;

                    case HandState.playing:
                        //the hand was hit at least once already
                        possibleMoves.Add(BlackjackMove.stand);
                        possibleMoves.Add(BlackjackMove.hit);
                        break;

                    case HandState.split:
                        //this hand is the result of splitting a hand
                        possibleMoves.Add(BlackjackMove.stand);
                        possibleMoves.Add(BlackjackMove.hit);
                        possibleMoves.Add(BlackjackMove.doubleDown);
                        break;

                    case HandState.doubled:
                    case HandState.standing:
                    case HandState.surrendered:
                    case HandState.bust:
                        //no more moves allowed
                        break;
                }

            return possibleMoves;
        }

        /// <summary>
        /// Calculates the total value of the given hand.
        /// </summary>
        /// <param name="handCards">List of cards in the hand.</param>
        /// <returns>The total value (in terms of Blackjack) of the hand.</returns>
        public static int HandTotalValue(List<Card> handCards)
        {
            int total = 0;
            int aces = 0;
            for (int i = 0; i < handCards.Count; ++i)
            {
                if (handCards[i].value > 1 && handCards[i].value < 11)
                    total += handCards[i].value;
                else if (handCards[i].value == 1)
                    aces++;
                else if (handCards[i].value > 10)
                    total += 10;
            }

            if (aces > 0)
            {
                if (total + aces - 1 < 11)
                {
                    total += 11 + aces - 1;
                }
                else
                {
                    total += aces;
                }
            }
            

            return total;
        }

        /// <summary>
        /// Applies the given move to the game.
        /// </summary>
        /// <param name="move">The move to apply.</param>
        private static void ApplyTurn(BlackjackMove move)
        {
            currentPlayerMove = move;

            if (AIThread != null && AIThread.IsAlive)
                EndOfTurn();
            else
                StartEndOfTurn();
        }
    }
}
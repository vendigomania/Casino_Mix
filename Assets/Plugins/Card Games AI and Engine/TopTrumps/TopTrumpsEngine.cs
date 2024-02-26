using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CGAIE
{
    /// <summary>
    /// <para>General Top Trumps engine. Rules:</para>
    /// <para>The game can use any kind of cards. Not just classic playing cards.</para>
    /// <para>Each card in the deck has same attributes, but with different values.</para>
    /// <para>The deck is divided among all players as evenly as possible, and all players are only able to see the topmost card of their own cards.</para>
    /// <para>First player initiates the first round, and from there, each round is initiated by the winner of the previous round.</para>
    /// <para>The player initiating a round picks one of the attributes that the cards have, and then everyone plays their card on the table.</para>
    /// <para>The player with the card that has the highest value in the picked attribute wins the round and gets the cards from other players.</para>
    /// <para>In case of a tie, the cards go to the side instead and the next round is a tie-breaker, where the winner of that round gets all the cards from the previous round.</para>
    /// <para>If a player does not have cards to take part in a tie-breaker, they lose automatically.</para>
    /// <para>When a player runs out of cards from their hand, they shuffle the pile that they have accumulated through wins.</para>
    /// <para>When a player is truly out of cards, they are out of the game.</para>
    /// <para>The player who collects the entire deck, wins.</para>
    /// </summary>
    public static class TopTrumpsEngine
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
            /// It is a player's turn to choose an attribute.
            /// </summary>
            normalTurn = 1,

            /// <summary>
            /// It is a player's turn to choose an attribute after a tie.
            /// </summary>
            normalTurnAfterTie = 2,

            /// <summary>
            /// The game is between turns.
            /// </summary>
            endOfRound = 3
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

        private static List<CustomCard>[] hands;
        /// <summary>
        /// List of cards for each player.
        /// </summary>
        public static List<CustomCard>[] Hands
        {
            get
            {
                if (hands == null)
                    ResetGame(false);
                return hands;
            }
        }

        private static List<CustomCard>[] unShuffledHands;
        /// <summary>
        /// List of cards each player has won, waiting to be shuffled in their deck.
        /// </summary>
        public static List<CustomCard>[] UnShuffledHands
        {
            get
            {
                if (unShuffledHands == null)
                    ResetGame(false);
                return unShuffledHands;
            }
        }

        private static List<CustomCard> table;
        /// <summary>
        /// List of cards on the table. Everyone's top-most card goes on the "table".
        /// </summary>
        public static List<CustomCard> Table
        {
            get
            {
                if (table == null)
                    ResetGame(false);
                return table;
            }
        }

        private static List<CustomCard> additionalCards;
        /// <summary>
        /// List of additional cards on the table, or the "pot". Cards from a tie end up here and the winner of the tie-breaker gets them.
        /// </summary>
        public static List<CustomCard> AdditionalCards
        {
            get
            {
                if (additionalCards == null)
                    ResetGame(false);
                return additionalCards;
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
                    ResetGame(false);
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

        private static string currentAttribute;
        /// <summary>
        /// The attribute currently being compared. Chosen by the previous round's winner.
        /// </summary>
        public static string CurrentAttribute
        {
            get
            {
                if (currentAttribute == null)
                    ResetGame(false);
                return currentAttribute;
            }
        }

        private static List<int> tieBreakerParticipants;
        /// <summary>
        /// Players that take part in the current tie-breaker (only used if 'PrivateTieBreakers' is True.
        /// </summary>
        public static List<int> TieBreakerParticipants
        {
            get
            {
                if (tieBreakerParticipants == null)
                    ResetGame(false);
                return tieBreakerParticipants;
            }
        }

        /// <summary>
        /// Are there tie-breakers in Top Trumps. If not, after a tie, everyone takes their cards back and puts them on the "unshuffled" cards.
        /// </summary>
        public static bool NoTieBreakers = false;
        /// <summary>
        /// Should tie-breakers only include players who tied in the first place.
        /// </summary>
        public static bool PrivateTieBreakers = true;
        /// <summary>
        /// If the lowest and highest possible value of the relevant attribute are both played during the same round, does the lowest value player win instead. For example, a two (lowest value) would win against the ace (highest value).
        /// </summary>
        public static bool LowestValueBeatsHighest = false;

        /// <summary>
        /// <para>The deck used for the game. This deck has to be defined before the game can start.</para>
        ///  <para>The deck consists of an array of attributes that each of its cards should have, as well as a list of the CustomCards themself.</para>
        /// </summary>
        public static CustomDeck deck;

        /// <summary>
        /// time to wait between rounds in milliseconds
        /// </summary>
        public static int waitPeriod = 3500;

        //private variables
        private static Thread AIThread;
        private static List<int> tablePlayers = new List<int>();

        /// <summary>
        /// Resets all variables related to the current game and stops the AI if needed.
        /// </summary>
        public static void ResetGame(bool warnIfIncompleteDeck = true)
        {
            StopAI();

            if (deck == null || deck.CardAttributes.Length < 1 || deck.Cards.Count < seats.Length)
            {
                if (warnIfIncompleteDeck)
                    Debug.LogWarning("Cannot start a game of Top Trumps without the Deck variable having at least one card for each player and at least one card attribute.");
                return;
            }

            _gameState = GameState.normalTurn;

            hands = new List<CustomCard>[seats.Length];
            unShuffledHands = new List<CustomCard>[seats.Length];
            table = new List<CustomCard>();
            additionalCards = new List<CustomCard>();
            tieBreakerParticipants = new List<int>();
            currentAttribute = "";
            currentPlayer = 1;

            //distributing the cards
            CustomDeck tmpDeck = Utils.IndependentCopyCustomDeck(deck);
            int cards = tmpDeck.Cards.Count;
            for (int i = 0; i < cards; ++i)
            {
                if (hands[i % seats.Length] == null)
                    hands[i % seats.Length] = new List<CustomCard>();
                if (unShuffledHands[i % seats.Length] == null)
                    unShuffledHands[i % seats.Length] = new List<CustomCard>();

                hands[i % seats.Length].Add(tmpDeck.Deal());
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
        public static void LocalMove(string attribute)
        {
            bool validMove = false;

            if (hands[currentPlayer - 1].Count > 0 && hands[currentPlayer - 1][0].attributes.ContainsKey(attribute))
            {
                validMove = true;
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.local && (_gameState == GameState.normalTurn || _gameState == GameState.normalTurnAfterTie))
            {
                ApplyTurn(attribute);
            }
        }

        /// <summary>
        /// Executes the given move if the current player is a custom player.
        /// </summary>
        /// <param name="card">The used hand card.</param>
        public static void CustomMove(string attribute)
        {
            bool validMove = false;

            if (hands[currentPlayer - 1].Count > 0 && hands[currentPlayer - 1][0].attributes.ContainsKey(attribute))
            {
                validMove = true;
            }

            if (validMove && seats[currentPlayer - 1] == PlayerType.custom && (_gameState == GameState.normalTurn || _gameState == GameState.normalTurnAfterTie))
            {
                ApplyTurn(attribute);
            }
        }

        /// <summary>
        /// Gives the GOPSAI the current state. The AI returns a move and the method applies it to the board.
        /// </summary>
        private static void ExecuteAIMove()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            //asking the AI for a move
            string aiMove = TopTrumpsAI.MakeMove(hands[currentPlayer - 1][0], deck, unShuffledHands);

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
        /// Executed at the end of a turn.
        /// </summary>
        private static void EndOfTurn()
        {
            _gameState = GameState.endOfRound;

            //finding out the player(s) with the highest value card
            List<int> highestValuePlayers = new List<int>();
            float highestValue = float.MinValue;
            for (int i = 0; i < table.Count; ++i)
            {
                if (!table[i].attributes.ContainsKey(currentAttribute))
                {
                    Debug.LogWarning("Card " + table[i] + " did not have the relevant attribute.");
                    continue;
                }

                if (table[i].attributes[currentAttribute] > highestValue)
                {
                    highestValue = table[i].attributes[currentAttribute];
                    highestValuePlayers = new List<int>() { tablePlayers[i] };
                }
                else if (table[i].attributes[currentAttribute] == highestValue)
                {
                    highestValuePlayers.Add(tablePlayers[i]);
                }
            }

            //checking if the round included the highest value card as well as the lowest value card
            //and the option to have the lowest value beat the highest value is on
            if (LowestValueBeatsHighest)
            {
                float lowestPossibleValue = float.MaxValue;
                float highestPossibleValue = float.MinValue;

                for (int i = 0; i < deck.Cards.Count; ++i)
                {
                    if (deck.Cards[i].attributes.ContainsKey(currentAttribute))
                    {
                        float attributeValue = deck.Cards[i].attributes[currentAttribute];
                        if (attributeValue < lowestPossibleValue)
                        {
                            lowestPossibleValue = attributeValue;
                        }
                        if (attributeValue > highestPossibleValue)
                        {
                            highestPossibleValue = attributeValue;
                        }
                    }
                }

                //checking if table had these values
                bool hasLowest = false;
                bool hasHighest = false;
                List<int> lowestValuePlayers = new List<int>();
                for (int i = 0; i < table.Count; ++i)
                {
                    if (table[i].attributes.ContainsKey(currentAttribute))
                    {
                        float attributeValue = table[i].attributes[currentAttribute];

                        if (attributeValue == lowestPossibleValue)
                        {
                            hasLowest = true;
                            lowestValuePlayers.Add(tablePlayers[i]);
                        }

                        if (attributeValue == highestPossibleValue)
                            hasHighest = true;
                    }
                }

                //table had both highest and lowest possible value
                if (hasHighest && hasLowest)
                {
                    highestValuePlayers = lowestValuePlayers;
                }
            }

            Thread.Sleep(waitPeriod);

            //in case of a tie, either
            //1) moving the cards to the side to wait for the next winner or
            //2) everyone takes back their cards
            if (highestValuePlayers.Count > 1)
            {
                if (NoTieBreakers)
                {
                    for (int i = 0; i < table.Count; ++i)
                    {
                        unShuffledHands[tablePlayers[i]].Add(table[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < table.Count; ++i)
                    {
                        additionalCards.Add(table[i]);
                    }
                }
            }

            //giving the cards to the winner
            else if (highestValuePlayers.Count == 1)
            {
                for (int i = 0; i < table.Count; ++i)
                {
                    unShuffledHands[highestValuePlayers[0]].Add(table[i]);
                }

                for (int i = 0; i < additionalCards.Count; ++i)
                {
                    unShuffledHands[highestValuePlayers[0]].Add(additionalCards[i]);
                }
                additionalCards.Clear();
            }


            //clearing the table
            table.Clear();
            tablePlayers.Clear();
            tieBreakerParticipants.Clear();

            //checking for end-of-game and finding out tie-breaker participants if there was a tie
            int playersÏnGame = 0;
            List<int> tmpTieBreakerParticipants = new List<int>();
            for (int i = 0; i < hands.Length; ++i)
            {
                if (hands[i].Count > 0 || unShuffledHands[i].Count > 0)
                {
                    playersÏnGame++;

                    if (!PrivateTieBreakers || highestValuePlayers.Contains(i))
                        tmpTieBreakerParticipants.Add(i+1);
                }
            }


            if (playersÏnGame > 1)
            {
                //the winner is the new player starting the round. If the round was a tie, the previous winner continues
                //unless the previous winner doesn't have any cards
                if (highestValuePlayers.Count == 1)
                    currentPlayer = highestValuePlayers[0]+1;

                //Shuffling won cards back to players' decks if their hand is empty
                for (int i = 0; i < hands.Length; ++i)
                {
                    if (hands[i].Count == 0 && unShuffledHands[i].Count > 0)
                    {
                        hands[i].AddRange(unShuffledHands[i]);
                        hands[i] = Utils.ShuffleCards(hands[i]);
                        unShuffledHands[i].Clear();
                    }    
                }

                //if the current player doesn't have any cards, the turn goes to the next player with cards
                for (int i = 0; i < hands.Length; ++i)
                {
                    if (!tmpTieBreakerParticipants.Contains(currentPlayer))
                    {
                        currentPlayer++;
                        if (currentPlayer > hands.Length)
                            currentPlayer = 1;
                        continue;
                    }

                    break;
                }


                //corner case: there was a tie but no one eligible has cards to play
                if (hands[currentPlayer - 1].Count == 0 && unShuffledHands[currentPlayer - 1].Count == 0)
                {
                    //giving the turn to whoever is the next player with cards
                    for (int i = 0; i < hands.Length; ++i)
                    {
                        if (hands[currentPlayer - 1].Count == 0 && unShuffledHands[currentPlayer - 1].Count == 0)
                        {
                            currentPlayer++;
                            if (currentPlayer > hands.Length)
                                currentPlayer = 1;
                            continue;
                        }

                        break;
                    }
                }
                else if(PrivateTieBreakers && highestValuePlayers.Count > 1)
                {
                    tieBreakerParticipants = tmpTieBreakerParticipants;
                }

                if (highestValuePlayers.Count > 1)
                    _gameState = GameState.normalTurnAfterTie;
                else
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
        public static string[] PossibleMoves()
        {
            return deck.CardAttributes;
        }

        /// <summary>
        /// Applies the given move in the game.
        /// </summary>
        /// <param name="move">Card used from the hand.</param>
        private static void ApplyTurn(string attribute)
        {
            currentAttribute = attribute;

            //placing everyone's topmost card on the table
            //except when there's private ties
            for (int i = 0; i < seats.Length; ++i)
            {
                if (hands[i].Count > 0 && (!PrivateTieBreakers || _gameState == GameState.normalTurn || tieBreakerParticipants.Count == 0 || tieBreakerParticipants.Contains(i+1)))
                {
                    tablePlayers.Add(i);
                    table.Add(hands[i][0]);
                    hands[i].RemoveAt(0);
                }
            }

            //going to 'end of turn' / showdown
            if (AIThread != null && AIThread.IsAlive)
                EndOfTurn();
            else
                StartEndOfTurn();
        }
    }
}
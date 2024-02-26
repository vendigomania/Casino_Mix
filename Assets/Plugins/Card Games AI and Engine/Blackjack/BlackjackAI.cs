using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Blackjack.
    /// </summary>
    public class BlackjackAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum BlackjackAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3
        }

        /// <summary>
        /// The type of the AI. As a BlackjackAIType enumeration.
        /// </summary>
        public static BlackjackAIType AIType = BlackjackAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="handState">The state of the current player's hand.</param>
        /// <param name="dealerUpCard">The dealer's upcard.</param>
        /// <returns>Blackjack move as a BlackjackEngine.BlackjackMove.</returns>
        public static BlackjackEngine.BlackjackMove MakeMove(List<Card> hand, BlackjackEngine.HandState handState, Card dealerUpCard)
        {
            List<BlackjackEngine.BlackjackMove> possibleMoves = BlackjackEngine.PossibleMoves(hand, handState);

            switch (AIType)
            {
                case BlackjackAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case BlackjackAIType.easy:
                    return MakeMoveEasy(possibleMoves, hand);

                case BlackjackAIType.medium:
                    return MakeMoveMedium(possibleMoves, hand, handState);

                case BlackjackAIType.hard:
                    return MakeMoveHard(possibleMoves, hand, handState, dealerUpCard);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        private static BlackjackEngine.BlackjackMove MakeMoveRandom(List<BlackjackEngine.BlackjackMove> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];

            return BlackjackEngine.BlackjackMove.stand;
        }

        private static BlackjackEngine.BlackjackMove MakeMoveEasy(List<BlackjackEngine.BlackjackMove> possibleMoves, List<Card> hand)
        {
            if (BlackjackEngine.HandTotalValue(hand) < 14)
            {
                if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.hit))
                    return BlackjackEngine.BlackjackMove.hit;
            }

            return BlackjackEngine.BlackjackMove.stand;
        }

        private static BlackjackEngine.BlackjackMove MakeMoveMedium(List<BlackjackEngine.BlackjackMove> possibleMoves, List<Card> hand, BlackjackEngine.HandState handState)
        {
            int handValue = BlackjackEngine.HandTotalValue(hand);

            if (handState == BlackjackEngine.HandState.fresh)
            {
                if ((handValue == 11 || handValue == 10) && possibleMoves.Contains(BlackjackEngine.BlackjackMove.doubleDown))
                    return BlackjackEngine.BlackjackMove.doubleDown;

                if (hand[0].value < 10 && hand[0].value != 5 && hand[0].value != 4 && possibleMoves.Contains(BlackjackEngine.BlackjackMove.split))
                    return BlackjackEngine.BlackjackMove.split;
            }

            if (handValue < 17)
            {
                if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.hit))
                    return BlackjackEngine.BlackjackMove.hit;
            }

            return BlackjackEngine.BlackjackMove.stand;
        }

        private static BlackjackEngine.BlackjackMove MakeMoveHard(List<BlackjackEngine.BlackjackMove> possibleMoves, List<Card> hand, BlackjackEngine.HandState handState, Card dealerUpCard)
        {
            int handValue = BlackjackEngine.HandTotalValue(hand);

            //"basic strategy"

            //first surrender if needed
            if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.surrender))
            {
                if ((dealerUpCard.value > 8 || dealerUpCard.value == 1) && handValue == 16)
                    return BlackjackEngine.BlackjackMove.surrender;
                if ((dealerUpCard.value > 9 || dealerUpCard.value == 1) && handValue == 15)
                    return BlackjackEngine.BlackjackMove.surrender;
            }

            if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.split))
            {
                //2,3 or 7
                if ((hand[0].value == 2 || hand[0].value == 3 || hand[0].value == 7) && dealerUpCard.value < 8 && dealerUpCard.value > 1)
                {
                    return BlackjackEngine.BlackjackMove.split;
                }

                //4
                if (hand[0].value == 4 && dealerUpCard.value < 7 && dealerUpCard.value > 4)
                {
                    return BlackjackEngine.BlackjackMove.split;
                }

                //6
                if (hand[0].value == 6 && dealerUpCard.value < 7 && dealerUpCard.value > 1)
                {
                    return BlackjackEngine.BlackjackMove.split;
                }

                //8, 1
                if (hand[0].value == 8 || hand[0].value == 1)
                {
                    return BlackjackEngine.BlackjackMove.split;
                }

                //9
                if (hand[0].value == 9 && ((dealerUpCard.value < 7 && dealerUpCard.value > 1) || dealerUpCard.value == 8 || dealerUpCard.value == 9))
                {
                    return BlackjackEngine.BlackjackMove.split;
                }

                //5, 10, 11, 12, 13 do not split
            }

            bool aces = false;
            int softTotal = 0;
            for (int i = 0; i < hand.Count; ++i)
            {
                if (aces || hand[i].value != 1)
                {
                    if (hand[i].value < 10)
                        softTotal += hand[i].value;
                    else
                    {
                        softTotal += 10;
                    }
                }

                if (hand[i].value == 1)
                    aces = true;
            }

            //"soft" total, meaning total of the hand minus the ace
            if (aces && softTotal < 10)
            {
                if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.doubleDown))
                {
                    if (softTotal < 9 && dealerUpCard.value == 6)
                        return BlackjackEngine.BlackjackMove.doubleDown;

                    if (softTotal < 8 && dealerUpCard.value == 5)
                        return BlackjackEngine.BlackjackMove.doubleDown;

                    if (softTotal < 8 && softTotal > 3 && dealerUpCard.value == 4)
                        return BlackjackEngine.BlackjackMove.doubleDown;

                    if (softTotal < 8 && softTotal > 5 && dealerUpCard.value == 3)
                        return BlackjackEngine.BlackjackMove.doubleDown;

                    if (softTotal == 7 && dealerUpCard.value == 2)
                        return BlackjackEngine.BlackjackMove.doubleDown;
                }

                if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.hit))
                {
                    if (softTotal == 7 && (dealerUpCard.value == 1 || dealerUpCard.value > 8))
                        return BlackjackEngine.BlackjackMove.hit;

                    if (softTotal < 7)
                        return BlackjackEngine.BlackjackMove.hit;
                }

                if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.stand))
                {
                    if (softTotal == 7 && dealerUpCard.value != 1 && dealerUpCard.value < 9)
                        return BlackjackEngine.BlackjackMove.stand;

                    if (softTotal > 7)
                        return BlackjackEngine.BlackjackMove.stand;
                }
            }

            //hard total values
            if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.doubleDown))
            {
                if (handValue == 11)
                    return BlackjackEngine.BlackjackMove.doubleDown;

                if (handValue == 10 && dealerUpCard.value < 10 && dealerUpCard.value > 1)
                    return BlackjackEngine.BlackjackMove.doubleDown;

                if (handValue == 19 && dealerUpCard.value < 7 && dealerUpCard.value > 2)
                    return BlackjackEngine.BlackjackMove.doubleDown;
            }

            if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.hit))
            {
                if (handValue < 12)
                    return BlackjackEngine.BlackjackMove.hit;

                if (handValue == 12 && dealerUpCard.value > 1 && dealerUpCard.value < 4)
                    return BlackjackEngine.BlackjackMove.hit;

                if (handValue < 17 && (dealerUpCard.value == 1 || dealerUpCard.value > 6))
                    return BlackjackEngine.BlackjackMove.hit;
            }

            //all other options are checked. time to stand
            if (possibleMoves.Contains(BlackjackEngine.BlackjackMove.stand))
                return BlackjackEngine.BlackjackMove.stand;


            //fallback. should never happen
            return MakeMoveRandom(possibleMoves);
        }
    }
}
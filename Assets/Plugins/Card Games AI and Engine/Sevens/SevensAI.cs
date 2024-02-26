using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Sevens.
    /// </summary>
    public static class SevensAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum SevensAIType
        {
            random = 0,
            medium = 1,
            hard = 2
        }

        /// <summary>
        /// The type of the AI. As a SevensAIType enumeration.
        /// </summary>
        public static SevensAIType AIType = SevensAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="table">List of cards already on the table..</param>
        /// <returns>Sevens move as a Card.</returns>
        public static Card MakeMove(List<Card> hand, List<Card> table)
        {
            List<Card> possibleMoves = SevensEngine.PossibleMoves(hand, table);

            switch (AIType)
            {
                case SevensAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case SevensAIType.medium:
                    return MakeMoveMedium(possibleMoves);

                case SevensAIType.hard:
                    return MakeMoveHard(possibleMoves, hand);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        public static Card MakeMoveRandom(List<Card> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];
            else
                return new Card(Suit.heart, 7);
        }

        public static Card MakeMoveMedium(List<Card> possibleMoves)
        {
            //play the card furthest from 7
            if (possibleMoves.Count > 0)
            {
                int edgeCard = 0;
                int distanceFrom7 = -1;
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    int dist = DistanceFromN(possibleMoves[i], 7);
                    if (dist > distanceFrom7)
                    {
                        edgeCard = i;
                        distanceFrom7 = dist;
                    }
                }

                return possibleMoves[edgeCard];
            }
            else //pass
                return new Card(Suit.heart, 7);
        }

        public static Card MakeMoveHard(List<Card> possibleMoves, List<Card> hand)
        {
            //play the card that helps AI eventually play the cards furthest from 7
            if (possibleMoves.Count > 0)
            {
                for (int d = 6; d >= 0; --d)
                {
                    for (int i = 0; i < hand.Count; ++i)
                    {
                        for (int k = 0; k < possibleMoves.Count; ++k)
                        {
                            if (possibleMoves[k].suit == hand[i].suit && ((hand[i].value <= 7 && possibleMoves[k].value <= 7) || (hand[i].value >= 7 && possibleMoves[k].value >= 7)))
                            {
                                if (DistanceFromN(hand[i], possibleMoves[k].value) == d)
                                {
                                    //furthest card from one that we can play is hand[i], and the card we can play is possibleMoves[k]
                                    return possibleMoves[k];
                                }
                            }
                        }
                    }
                }
            }
            
            //pass
            return new Card(Suit.heart, 7);
        }

        private static int DistanceFromN(Card card, int n)
        {
            int dist = card.value - n;
            if (dist < 0) dist = - dist;
            return dist;
        }
    }
}

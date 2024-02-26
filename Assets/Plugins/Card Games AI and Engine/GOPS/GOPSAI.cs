using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game GOPS (Game Of Pure Strategy).
    /// </summary>
    public static class GOPSAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum GOPSAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3
        }

        /// <summary>
        /// The type of the AI. As a GOPSAIType enumeration.
        /// </summary>
        public static GOPSAIType AIType = GOPSAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="table">The current card prize on the table.</param>
        /// <param name="previousPrize">The previous prize card.</param>
        /// <param name="previousBids">List of the previous prize's bids.</param>
        /// <param name="currentPlayerIndex">The index of this player. I.e. which bid in previousBids is this player's.</param>
        /// <returns>GOPS move as a Card.</returns>
        public static Card MakeMove(List<Card> hand, Card table, Card previousPrize, Card[] previousBids, int currentPlayerIndex)
        {
            List<Card> possibleMoves = GOPSEngine.PossibleMoves(hand);

            switch (AIType)
            {
                case GOPSAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case GOPSAIType.easy:
                    return MakeMoveEasy(possibleMoves, table);

                case GOPSAIType.medium:
                    return MakeMoveMedium(possibleMoves, table);

                case GOPSAIType.hard:
                    return MakeMoveHard(possibleMoves, table, previousPrize, previousBids, currentPlayerIndex);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        private static Card MakeMoveRandom(List<Card> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];

            return new Card(Suit.spade, 1);
        }

        private static Card MakeMoveEasy(List<Card> possibleMoves, Card table)
        {
            //doing the basic strategy of bidding the exact value of the table card

            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (possibleMoves[i].value == table.value)
                    return possibleMoves[i];
            }

            return MakeMoveRandom(possibleMoves);
        }

        private static Card MakeMoveMedium(List<Card> possibleMoves, Card table)
        {
            //doing the "oneup" strategy of bidding table value + 1

            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                //special case of 13 where AI bids 1
                if (table.value == 13)
                {
                    if (possibleMoves[i].value == 1)
                        return possibleMoves[i];
                }
                else if (possibleMoves[i].value == table.value + 1)
                    return possibleMoves[i];
            }

            return MakeMoveEasy(possibleMoves, table);
        }

        private static Card MakeMoveHard(List<Card> possibleMoves, Card table, Card previousPrize, Card[] previousBids, int currentPlayerIndex)
        {
            //bidding one higher than the previous bid winner, relative to the card's value
            //if the winner was this AI, bid with the same difference
            //if not possible, bidding as close as possible
            //if card is <7, bid with lower offer, with some random to throw off other players

            System.Random random = new System.Random(System.DateTime.UtcNow.Millisecond / 3 + 4492*currentPlayerIndex);

            if (possibleMoves.Count == 13)
            {
                //first round. if <7, play random lowball offer. if >=7, bid value +0 to +2
                if (table.value < 7)
                    return possibleMoves[random.Next(0, possibleMoves.Count / 3)];
                else
                {
                    int lowEnd = Utils.MinInt(table.value, possibleMoves.Count);
                    return possibleMoves[random.Next(lowEnd-1, Utils.MinInt(lowEnd + 2, possibleMoves.Count))];
                }
            }

            //special case if bidding is done on 13 and AI still as 13 left
            if (table.value == 13)
            {
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    if (possibleMoves[i].value == 13)
                        return possibleMoves[i];
                }
            }

            int targetBidDifference = -13;
            if (table.value > 6)
            {
                for (int i = 0; i < previousBids.Length; ++i)
                {
                    if (previousBids[i].value - previousPrize.value > targetBidDifference)
                    {
                        if (i == currentPlayerIndex)
                            targetBidDifference = previousBids[i].value - previousPrize.value;
                        else
                            targetBidDifference = previousBids[i].value - previousPrize.value + random.Next(0, 2);
                    }
                }

                if (targetBidDifference < -2)
                    targetBidDifference = -2;

            }
            else
            {
                targetBidDifference = random.Next(-2, 3);
            }

            for (int i = 0; i < 13; ++i)
            {
                for (int k = possibleMoves.Count - 1; k >= 0; --k)
                {
                    if (possibleMoves[k].value - table.value == targetBidDifference + i)
                    {
                        return possibleMoves[k];
                    }
                }
            }

            return MakeMoveMedium(possibleMoves, table);
        }

    }
}

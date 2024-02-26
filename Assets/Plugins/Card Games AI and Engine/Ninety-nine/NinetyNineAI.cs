using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Ninety Nine.
    /// </summary>
    public static class NinetyNineAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum NinetyNineAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3
        }

        /// <summary>
        /// The type of the AI. As a NinetyNineAIType enumeration.
        /// </summary>
        public static NinetyNineAIType AIType = NinetyNineAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="table">The current value of the pile of cards on the table.</param>
        /// <returns>Ninety Nine move as a NinetyNineEngine.NinetyNineMove.</returns>
        public static NinetyNineEngine.NinetyNineMove MakeMove(List<Card> hand, int table)
        {
            List<NinetyNineEngine.NinetyNineMove> possibleMoves = NinetyNineEngine.PossibleMoves(hand);

            switch (AIType)
            {
                case NinetyNineAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case NinetyNineAIType.easy:
                    return MakeMoveEasy(possibleMoves, table);

                case NinetyNineAIType.medium:
                    return MakeMoveMedium(possibleMoves, table);

                case NinetyNineAIType.hard:
                    return MakeMoveHard(possibleMoves, table);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        private static NinetyNineEngine.NinetyNineMove MakeMoveRandom(List<NinetyNineEngine.NinetyNineMove> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];
            else
                return new NinetyNineEngine.NinetyNineMove(new Card(Suit.club, 13), 0);
        }

        private static NinetyNineEngine.NinetyNineMove MakeMoveEasy(List<NinetyNineEngine.NinetyNineMove> possibleMoves, int table)
        {
            //doing randomly any move that isn't a losing move if possible

            List<NinetyNineEngine.NinetyNineMove> reasonableMoves = new List<NinetyNineEngine.NinetyNineMove>();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (table + possibleMoves[i].value <= 99 || possibleMoves[i].card.value == 9)
                {
                    reasonableMoves.Add(possibleMoves[i]);
                }
            }
            if (reasonableMoves.Count > 0)
                return reasonableMoves[new System.Random().Next(0, reasonableMoves.Count)];

            return MakeMoveRandom(possibleMoves);
        }

        private static NinetyNineEngine.NinetyNineMove MakeMoveMedium(List<NinetyNineEngine.NinetyNineMove> possibleMoves, int table)
        {
            List<int> savedCards = new List<int>() { 4, 9, 10, 13 };

            //doing randomly any move that isn't a losing move if possible, but saving 4s, 9s, 10s and 13s
            List<NinetyNineEngine.NinetyNineMove> reasonableMoves = new List<NinetyNineEngine.NinetyNineMove>();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (table + possibleMoves[i].value <= 99 && !savedCards.Contains(possibleMoves[i].card.value))
                {
                    reasonableMoves.Add(possibleMoves[i]);
                }
            }
            if (reasonableMoves.Count > 0)
                return reasonableMoves[new System.Random().Next(0, reasonableMoves.Count)];

            return MakeMoveEasy(possibleMoves, table);
        }

        private static NinetyNineEngine.NinetyNineMove MakeMoveHard(List<NinetyNineEngine.NinetyNineMove> possibleMoves, int table)
        {
            List<int> normalOrder = new List<int>() { 1, 8, 7, 6, 5, 3, 2, 11, 12, 9, 10, 13, 4 };
            List<int> order99 = new List<int>() { 4, 9, 13, 10 };

            //special case if early game and got 9 and at least one 4/9/13
            if (table < 40)
            {
                int nines = 0;
                int skipCards = 0;
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    if (possibleMoves[i].card.value == 4 || possibleMoves[i].card.value == 9 || possibleMoves[i].card.value == 13)
                        skipCards++;

                    if (possibleMoves[i].card.value == 9)
                        nines++;
                }
                if (nines > 0 && skipCards > 1)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].card.value == 9)
                        {
                            return possibleMoves[i];
                        }
                    }
                }
            }
            //playing cards in priority order depending on the situation
            else if (table < 99)
            {
                for (int o = 0; o < normalOrder.Count; ++o)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].card.value == normalOrder[o] && (possibleMoves[i].card.value == 9 || table + possibleMoves[i].value <= 99))
                        {
                            return possibleMoves[i];
                        }
                    }
                }
            }
            else if (table == 99)
            {
                for (int o = 0; o < order99.Count; ++o)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].card.value == order99[o] && (possibleMoves[i].card.value == 9 || table + possibleMoves[i].value <= 99))
                        {
                            return possibleMoves[i];
                        }
                    }
                }
            }

            return MakeMoveMedium(possibleMoves, table);
        }

    }
}

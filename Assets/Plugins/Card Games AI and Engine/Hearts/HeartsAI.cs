using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Hearts.
    /// </summary>
    public static class HeartsAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum HeartsAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3
        }

        /// <summary>
        /// The type of the AI. As a HeartsAIType enumeration.
        /// </summary>
        public static HeartsAIType AIType = HeartsAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="collected">Array of lists of cards collected by each player.</param>
        /// <param name="table">The cards that have been played for the current trick already.</param>
        /// <param name="gs">The state of the game. I.e. should the AI pass cards or is it a normal turn.</param>
        /// <returns>Hearts move as a Card.</returns>
        public static Card MakeMove(List<Card> hand, List<Card>[] collected, List<Card> table, HeartsEngine.GameState gs)
        {
            List<Card> possibleMoves = HeartsEngine.PossibleMoves(hand, collected, table, gs);

            switch (AIType)
            {
                case HeartsAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case HeartsAIType.easy:
                    return MakeMoveEasy(possibleMoves, gs, table);

                case HeartsAIType.medium:
                    return MakeMoveMedium(possibleMoves, gs, table, collected);

                case HeartsAIType.hard:
                    return MakeMoveHard(possibleMoves, gs, table, collected);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        public static Card MakeMoveRandom(List<Card> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];
            else
                return null;
        }

        public static Card MakeMoveEasy(List<Card> possibleMoves, HeartsEngine.GameState gs, List<Card> table)
        {
            if (gs == HeartsEngine.GameState.normalTurn)
            {
                //lowest possible card value of the suit the AI has the least
                if (table.Count == 0)
                {
                    int[] counts = new int[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        counts[(int)possibleMoves[i].suit - 1]++;
                    }

                    int min = 20;
                    int minSuit = 0;

                    for (int i = 0; i < counts.Length; ++i)
                    {
                        if (counts[i] > 0 && counts[i] < min)
                        {
                            min = counts[i];
                            minSuit = i;
                        }
                    }

                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if ((int)possibleMoves[i].suit - 1 == minSuit)
                        {
                            if (possibleMoves[i].suit != Suit.spade || possibleMoves[i].value != 12)
                                return possibleMoves[i];
                        }
                    }
                }

                //lowest possible card value if same suit. otherwise queen of spades or highest possible heart
                if (table.Count > 0)
                {
                    if (possibleMoves[0].suit == table[0].suit)
                    {
                        return possibleMoves[0];
                    }

                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.spade && possibleMoves[i].value == 12)
                            return possibleMoves[i];
                    }

                    int maxHeart = 0;
                    int maxHeartIndex = -1;
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.heart && possibleMoves[i].value == 1)
                        {
                            return possibleMoves[i];
                        }
                        if (possibleMoves[i].suit == Suit.heart && possibleMoves[i].value > maxHeart)
                        {
                            maxHeart = possibleMoves[i].value;
                            maxHeartIndex = i;
                        }
                    }
                    if (maxHeart > 0 && maxHeartIndex >= 0)
                    {
                        return possibleMoves[maxHeartIndex];
                    }
                }
            }

            return MakeMoveRandom(possibleMoves);
        }

        public static Card MakeMoveMedium(List<Card> possibleMoves, HeartsEngine.GameState gs, List<Card> table, List<Card>[] collected)
        {
            //card passing
            if (gs == HeartsEngine.GameState.cardPassing)
            {
                // 13 & 14 of spades as priority
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    if (possibleMoves[i].suit == Suit.spade && (possibleMoves[i].value == 1 || possibleMoves[i].value == 13))
                    {
                        return possibleMoves[i];
                    }
                }

                //biggest values of the suit the AI has the least
                int[] counts = new int[4] { 0, 0, 0, 0 };
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    counts[(int)possibleMoves[i].suit - 1]++;
                }

                int min = 20;
                int minSuit = 0;

                for (int i = 0; i < counts.Length; ++i)
                {
                    if (counts[i] > 0 && counts[i] < min)
                    {
                        min = counts[i];
                        minSuit = i;
                    }
                }

                for (int i = possibleMoves.Count - 1; i >= 0; --i)
                {
                    if ((int)possibleMoves[i].suit - 1 == minSuit)
                    {
                        return possibleMoves[i];
                    }
                }
            }

            if (gs == HeartsEngine.GameState.normalTurn)
            {
                //lowest possible card value of the suit the AI has the least
                if (table.Count == 0)
                {
                    int[] counts = new int[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        counts[(int)possibleMoves[i].suit - 1]++;
                    }

                    int min = 20;
                    int minSuit = 0;

                    for (int i = 0; i < counts.Length; ++i)
                    {
                        if (counts[i] > 0 && counts[i] < min)
                        {
                            min = counts[i];
                            minSuit = i;
                        }
                    }

                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if ((int)possibleMoves[i].suit - 1 == minSuit)
                        {
                            if (possibleMoves[i].suit != Suit.spade || possibleMoves[i].value != 12)
                            {
                                return possibleMoves[i];
                            }
                        }
                    }
                }

                //highest possible card without risking taking the table pile if same suit. otherwise queen of spades or highest possible heart
                //if last player and same suit, highest possible card if not risking heart/queen of spades.
                if (table.Count > 0)
                {
                    bool first = true;
                    //if first trick, highest club
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        if (collected[i].Count > 0)
                        {
                            first = false;
                            break;
                        }
                    }

                    if (first)
                    {
                        return possibleMoves[possibleMoves.Count - 1];
                    }

                    //AI has same suit
                    if (possibleMoves[0].suit == table[0].suit)
                    {
                        //highest card of the main suit
                        int currentHighest = table[0].value;
                        for (int i = 1; i < table.Count; ++i)
                        {
                            if (table[i].suit == table[0].suit && currentHighest < table[i].value)
                                currentHighest = table[i].value;
                        }

                        //AI is last person on the trick
                        if (table.Count == collected.Length - 1)
                        {
                            int pileScore = 0;
                            for (int i = 0; i < table.Count; ++i)
                            {
                                if (table[i].suit == Suit.heart)
                                    pileScore++;
                                else if (table[i].suit == Suit.spade && table[i].value == 12)
                                    pileScore += 13;
                            }

                            if (pileScore == 0 || (possibleMoves[0].value > currentHighest))
                            {
                                return possibleMoves[possibleMoves.Count - 1];
                            }
                        }

                        //highest card AI has that isn't higher than the table's highest card
                        for (int i = possibleMoves.Count - 1; i >= 0; --i)
                        {
                            if (possibleMoves[i].value < currentHighest)
                            {
                                return possibleMoves[i];
                            }
                        }

                        //lowest value card of the main suit
                        return possibleMoves[0];
                    }

                    //not the same suit
                    //queen of spades
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.spade && possibleMoves[i].value == 12)
                            return possibleMoves[i];
                    }

                    //highest heart
                    int maxHeart = 0;
                    int maxHeartIndex = -1;
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.heart && possibleMoves[i].value == 1)
                        {
                            return possibleMoves[i];
                        }
                        if (possibleMoves[i].suit == Suit.heart && possibleMoves[i].value > maxHeart)
                        {
                            maxHeart = possibleMoves[i].value;
                            maxHeartIndex = i;
                        }
                    }
                    if (maxHeart > 0 && maxHeartIndex >= 0)
                    {
                        return possibleMoves[maxHeartIndex];
                    }

                    //any high card
                    for (int i = 14; i > 1; --i)
                    {
                        if (i == 14)
                            i = 1;

                        for (int k = possibleMoves.Count - 1; k >= 0; --k)
                        {
                            if (possibleMoves[k].value == i)
                                return possibleMoves[k];
                        }

                        if (i == 1)
                            i = 14;
                    }
                }
            }

            return MakeMoveRandom(possibleMoves);
        }

        public static Card MakeMoveHard(List<Card> possibleMoves, HeartsEngine.GameState gs, List<Card> table, List<Card>[] collected)
        {
            //card passing
            if (gs == HeartsEngine.GameState.cardPassing)
            {
                // 13 & 14 of spades as priority
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    if (possibleMoves[i].suit == Suit.spade && (possibleMoves[i].value == 1 || possibleMoves[i].value == 13))
                    {
                        return possibleMoves[i];
                    }
                }

                //if last card, try to pass a non-ace heart to block blowout chance
                bool lastCard = false;
                for (int i = 0; i < collected.Length; ++i)
                {
                    if (collected[i].Count == 2)
                    {
                        lastCard = true;
                        break;
                    }
                }

                if (lastCard)
                {
                    for (int i = possibleMoves.Count-1; i >= 0; --i)
                    {
                        if (possibleMoves[i].suit == Suit.heart && possibleMoves[i].value > 1)
                            return possibleMoves[i];
                    }
                }

                //biggest values of the suit the AI has the least
                int[] counts = new int[4] { 0, 0, 0, 0 };
                for (int i = 0; i < possibleMoves.Count; ++i)
                {
                    //not giving <12 spades in any scenario
                    if (possibleMoves[i].suit != Suit.spade || possibleMoves[i].value == 12)
                        counts[(int)possibleMoves[i].suit - 1]++;
                }

                int min = 20;
                int minSuit = 0;

                for (int i = 0; i < counts.Length; ++i)
                {
                    if (counts[i] > 0 && counts[i] < min)
                    {
                        min = counts[i];
                        minSuit = i;
                    }
                }

                for (int i = possibleMoves.Count - 1; i >= 0; --i)
                {
                    if ((int)possibleMoves[i].suit - 1 == minSuit)
                    {
                        return possibleMoves[i];
                    }
                }
            }

            if (gs == HeartsEngine.GameState.normalTurn)
            {
                //lowest possible card value of the suit the AI has the least
                if (table.Count == 0)
                {
                    bool playSpade = true;
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.spade && (possibleMoves[i].value == 1 || possibleMoves[i].value > 11))
                        {
                            playSpade = false;
                            break;
                        }
                    }
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        for (int k = 0; k < collected[i].Count; ++k)
                        {
                            if (collected[i][k].suit == Suit.spade && collected[i][k].value == 12)
                            {
                                playSpade = false;
                                break;
                            }
                        }

                        if (!playSpade)
                            break;
                    }

                    if (playSpade)
                    {
                        for (int i = possibleMoves.Count-1; i >= 0; --i)
                        {
                            if (possibleMoves[i].suit == Suit.spade)
                                return possibleMoves[i];
                        }
                    }

                    int[] counts = new int[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        //avoiding very high cards even if they are the last of a suit
                        if (possibleMoves[i].value > 1 && possibleMoves[i].value < 12)
                            counts[(int)possibleMoves[i].suit - 1]++;
                    }

                    int min = 20;
                    int minSuit = 0;

                    for (int i = 0; i < counts.Length; ++i)
                    {
                        if (counts[i] > 0 && counts[i] < min)
                        {
                            min = counts[i];
                            minSuit = i;
                        }
                    }

                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if ((int)possibleMoves[i].suit - 1 == minSuit)
                        {
                            if (possibleMoves[i].suit != Suit.spade || possibleMoves[i].value != 12)
                            {
                                return possibleMoves[i];
                            }
                        }
                    }
                }

                //highest possible card without risking taking the table pile if same suit. otherwise queen of spades or highest possible heart
                //if last player and same suit, highest possible card if not risking heart/queen of spades.
                if (table.Count > 0)
                {
                    bool first = true;
                    //if first trick, highest club
                    for (int i = 0; i < collected.Length; ++i)
                    {
                        if (collected[i].Count > 0)
                        {
                            first = false;
                            break;
                        }
                    }

                    if (first)
                    {
                        return possibleMoves[possibleMoves.Count - 1];
                    }

                    //AI has same suit
                    if (possibleMoves[0].suit == table[0].suit)
                    {
                        //highest card of the main suit in table
                        int currentHighest = table[0].value;
                        for (int i = 1; i < table.Count; ++i)
                        {
                            if (table[i].suit == table[0].suit && currentHighest < table[i].value)
                                currentHighest = table[i].value;
                        }

                        //AI is last person on the trick
                        if (table.Count == collected.Length - 1)
                        {
                            int pileScore = 0;
                            for (int i = 0; i < table.Count; ++i)
                            {
                                if (table[i].suit == Suit.heart)
                                    pileScore++;
                                else if (table[i].suit == Suit.spade && table[i].value == 12)
                                    pileScore += 13;
                            }

                            if (pileScore <= 1 || (possibleMoves[0].value > currentHighest))
                            {
                                return possibleMoves[possibleMoves.Count - 1];
                            }
                        }

                        //highest card AI has that isn't higher than the table's highest card
                        for (int i = possibleMoves.Count - 1; i >= 0; --i)
                        {
                            if (possibleMoves[i].value < currentHighest)
                            {
                                return possibleMoves[i];
                            }
                        }

                        //lowest value card of the main suit
                        return possibleMoves[0];
                    }

                    //not the same suit
                    //queen of spades
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (possibleMoves[i].suit == Suit.spade && possibleMoves[i].value == 12)
                            return possibleMoves[i];
                    }

                    //highest or 2nd highest heart. depending if there's a chance for blowout
                    for (int i = possibleMoves.Count-1; i >= 0; --i)
                    {
                        if (possibleMoves[i].suit == Suit.heart)
                        {
                            int playersWithPoints = 0;
                            for (int k = 0; k < collected.Length; ++k)
                            {
                                for (int l = 0; l < collected[k].Count; ++l)
                                {
                                    if (collected[k][k].suit == Suit.heart)
                                    {
                                        playersWithPoints++;
                                        break;
                                    }
                                }
                            }

                            if (playersWithPoints > 1)
                                return possibleMoves[i];
                            if (i > 0 && possibleMoves[i - 1].suit == Suit.heart)
                                return possibleMoves[i - 1];
                            else
                                break;
                        }
                    }

                    //any high card
                    for (int i = 14; i > 1; --i)
                    {
                        if (i == 14)
                            i = 1;

                        for (int k = possibleMoves.Count - 1; k >= 0; --k)
                        {
                            if (possibleMoves[k].value == i)
                                return possibleMoves[k];
                        }

                        if (i == 1)
                            i = 14;
                    }
                }
            }

            //backup
            return MakeMoveMedium(possibleMoves, gs, table, collected);
        }
    }
}

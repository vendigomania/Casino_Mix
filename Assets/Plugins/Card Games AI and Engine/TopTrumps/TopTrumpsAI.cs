using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Top Trumps.
    /// </summary>
    public static class TopTrumpsAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum TopTrumpsAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3,
            veryHard = 4
        }

        /// <summary>
        /// The type of the AI. As a TopTrumpsAIType enumeration.
        /// </summary>
        public static TopTrumpsAIType AIType = TopTrumpsAIType.veryHard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="topCard">The AI's card that it is about to play.</param>
        /// <param name="deck">The CustomCard deck used in the game. The deck should be "full", containing all cards that are circulated in the game.</param>
        /// <param name="unShuffledPiles">Array of everyone's cards that they have won but not yet shuffled back to their hand.</param>
        /// <returns>Top Trumps move as a string, corresponding the CustomCard attribute that will be played.</returns>
        public static string MakeMove(CustomCard topCard, CustomDeck deck, List<CustomCard>[] unShuffledPiles)
        {
            switch (AIType)
            {
                case TopTrumpsAIType.random:
                    return MakeMoveRandom(topCard, deck);

                case TopTrumpsAIType.easy:
                    return MakeMoveEasy(topCard, deck);

                case TopTrumpsAIType.medium:
                    return MakeMoveMedium(topCard, deck);

                case TopTrumpsAIType.hard:
                    return MakeMoveHard(topCard, deck);

                case TopTrumpsAIType.veryHard:
                    return MakeMoveVeryHard(topCard, deck, unShuffledPiles);
            }

            //fallback
            return MakeMoveRandom(topCard, deck);
        }

        private static string MakeMoveRandom(CustomCard topCard, CustomDeck deck)
        {
            //random attribute
            return deck.CardAttributes[new System.Random().Next(0, deck.CardAttributes.Length)];
        }

        private static string MakeMoveEasy(CustomCard topCard, CustomDeck deck)
        {
            //picking one random card out of the deck and compare the attributes to the AI's top card.

            List<CustomCard> randomCards = new List<CustomCard>();
            int cards = 1;
            for (int i = 0; i < cards; ++i)
            {
                randomCards.Add(deck.Cards[new System.Random().Next(0, deck.Cards.Count)]);
            }

            //ranking the attributes of all the selected cards
            List<float>[] attributeRankings = new List<float>[deck.CardAttributes.Length];

            for (int i = 0; i < randomCards.Count; ++i)
            {
                for (int a = 0; a < deck.CardAttributes.Length; ++a)
                {
                    if (attributeRankings[a] == null)
                        attributeRankings[a] = new List<float>();
                    attributeRankings[a].Add(randomCards[i].attributes[deck.CardAttributes[a]]);
                }
            }


            //sorting the attributeRankings
            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                attributeRankings[i].Sort(delegate (float a, float b)
                {
                    if (a == b) return 0;
                    return a > b ? 1 : -1;
                });
            }

            //finding out which attribute is suggested to be the best of the top card based on the rankings of the random cards
            float[] topCardRanking = new float[deck.CardAttributes.Length];

            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                topCardRanking[i] = float.MinValue;
                bool noSmallerValues = true;
                for (int k = 0; k < attributeRankings[i].Count; ++k)
                {
                    if (topCard.attributes.ContainsKey(deck.CardAttributes[i]) && attributeRankings[i][k] >= topCard.attributes[deck.CardAttributes[i]])
                    {
                        topCardRanking[i] = k;
                        noSmallerValues = false;
                        break;
                    }
                }
                if (noSmallerValues)
                    topCardRanking[i] = attributeRankings[i].Count;
            }

            //picking the one with the highest rank
            float highestValue = float.MinValue;
            int highestValueIndex = 0;

            for (int i = 0; i < topCardRanking.Length; ++i)
            {
                if (topCardRanking[i] > highestValue)
                {
                    highestValue = topCardRanking[i];
                    highestValueIndex = i;
                }
            }

            return deck.CardAttributes[highestValueIndex];
        }

        private static string MakeMoveMedium(CustomCard topCard, CustomDeck deck)
        {
            //consider random 5% of the total cards and compare the attributes to the AI's top card.

            List<CustomCard> randomCards = new List<CustomCard>();
            int cards = 1;
            float portion = 0.05f;
            if (deck.Cards.Count * portion > 1)
                cards = (int)(deck.Cards.Count * portion);
            for (int i = 0; i < cards; ++i)
            {
                randomCards.Add(deck.Cards[new System.Random().Next(0, deck.Cards.Count)]);
            }

            //ranking the attributes of all the selected cards
            List<float>[] attributeRankings = new List<float>[deck.CardAttributes.Length];

            for (int i = 0; i < randomCards.Count; ++i)
            {
                for (int a = 0; a < deck.CardAttributes.Length; ++a)
                {
                    if (attributeRankings[a] == null)
                        attributeRankings[a] = new List<float>();
                    attributeRankings[a].Add(randomCards[i].attributes[deck.CardAttributes[a]]);
                }
            }


            //sorting the attributeRankings
            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                attributeRankings[i].Sort(delegate (float a, float b)
                {
                    if (a == b) return 0;
                    return a > b ? 1 : -1;
                });
            }

            //finding out which attribute is suggested to be the best of the top card based on the rankings of the random cards
            float[] topCardRanking = new float[deck.CardAttributes.Length];

            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                topCardRanking[i] = float.MinValue;
                bool noSmallerValues = true;
                for (int k = 0; k < attributeRankings[i].Count; ++k)
                {
                    if (topCard.attributes.ContainsKey(deck.CardAttributes[i]) && attributeRankings[i][k] >= topCard.attributes[deck.CardAttributes[i]])
                    {
                        topCardRanking[i] = k;
                        noSmallerValues = false;
                        break;
                    }
                }
                if (noSmallerValues)
                    topCardRanking[i] = attributeRankings[i].Count;
            }

            //picking the one with the highest rank
            float highestValue = float.MinValue;
            int highestValueIndex = 0;

            for (int i = 0; i < topCardRanking.Length; ++i)
            {
                if (topCardRanking[i] > highestValue)
                {
                    highestValue = topCardRanking[i];
                    highestValueIndex = i;
                }
            }

            return deck.CardAttributes[highestValueIndex];
        }

        private static string MakeMoveHard(CustomCard topCard, CustomDeck deck)
        {
            //consider random 50% of the total cards and compare the attributes to the AI's top card.

            List<CustomCard> randomCards = new List<CustomCard>();
            int cards = 1;
            float portion = 0.5f;
            if (deck.Cards.Count * portion > 1)
                cards = (int)(deck.Cards.Count * portion);
            for (int i = 0; i < cards; ++i)
            {
                randomCards.Add(deck.Cards[new System.Random().Next(0, deck.Cards.Count)]);
            }

            //ranking the attributes of all the selected cards
            List<float>[] attributeRankings = new List<float>[deck.CardAttributes.Length];

            for (int i = 0; i < randomCards.Count; ++i)
            {
                for (int a = 0; a < deck.CardAttributes.Length; ++a)
                {
                    if (attributeRankings[a] == null)
                        attributeRankings[a] = new List<float>();
                    attributeRankings[a].Add(randomCards[i].attributes[deck.CardAttributes[a]]);
                }
            }


            //sorting the attributeRankings
            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                attributeRankings[i].Sort(delegate(float a, float b)
                {
                    if (a == b) return 0;
                    return a > b? 1 : -1;
                });
            }

            //finding out which attribute is suggested to be the best of the top card based on the rankings of the random cards
            float[] topCardRanking = new float[deck.CardAttributes.Length];

            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                topCardRanking[i] = float.MinValue;
                bool noSmallerValues = true;
                for (int k = 0; k < attributeRankings[i].Count; ++k)
                {
                    if (topCard.attributes.ContainsKey(deck.CardAttributes[i]) && attributeRankings[i][k] >= topCard.attributes[deck.CardAttributes[i]])
                    {
                        topCardRanking[i] = k;
                        noSmallerValues = false;
                        break;
                    }
                }
                if (noSmallerValues)
                    topCardRanking[i] = attributeRankings[i].Count;
            }

            //picking the one with the highest rank
            float highestValue = float.MinValue;
            int highestValueIndex = 0;

            for (int i = 0; i < topCardRanking.Length; ++i)
            {
                if (topCardRanking[i] > highestValue)
                {
                    highestValue = topCardRanking[i];
                    highestValueIndex = i;
                }
            }

            return deck.CardAttributes[highestValueIndex];
        }

        private static string MakeMoveVeryHard(CustomCard topCard, CustomDeck deck, List<CustomCard>[] unShuffledPiles)
        {
            //consider all cards in the deck except those in the "unshuffled" piles (or the AI's own top card) and compare the attributes to the AI's top card.

            List<CustomCard>[] tmpCopyUnshuffledCards = Utils.IndependentCopyMatrix(unShuffledPiles);

            List<CustomCard> randomCards = new List<CustomCard>();
            for (int i = 0; i < deck.Cards.Count; ++i)
            {
                bool inUnshuffledPile = false;
                for (int u = 0; u < tmpCopyUnshuffledCards.Length; ++u)
                {
                    for (int k = 0; k < tmpCopyUnshuffledCards[u].Count; ++k)
                    {
                        if (tmpCopyUnshuffledCards[u][k].ID == deck.Cards[i].ID)
                        {
                            tmpCopyUnshuffledCards[u].RemoveAt(k);
                            inUnshuffledPile = true;
                            break;
                        }
                    }

                    if (inUnshuffledPile)
                        break;
                }

                if (!inUnshuffledPile)
                    randomCards.Add(deck.Cards[i]);
            }

            //ranking the attributes of all the selected cards
            List<float>[] attributeRankings = new List<float>[deck.CardAttributes.Length];

            for (int i = 0; i < randomCards.Count; ++i)
            {
                for (int a = 0; a < deck.CardAttributes.Length; ++a)
                {
                    if (attributeRankings[a] == null)
                        attributeRankings[a] = new List<float>();
                    attributeRankings[a].Add(randomCards[i].attributes[deck.CardAttributes[a]]);
                }
            }


            //sorting the attributeRankings
            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                attributeRankings[i].Sort(delegate (float a, float b)
                {
                    if (a == b) return 0;
                    return a > b ? 1 : -1;
                });
            }

            //finding out which attribute is suggested to be the best of the top card based on the rankings of the random cards
            float[] topCardRanking = new float[deck.CardAttributes.Length];

            for (int i = 0; i < attributeRankings.Length; ++i)
            {
                topCardRanking[i] = float.MinValue;
                bool noSmallerValues = true;
                for (int k = 0; k < attributeRankings[i].Count; ++k)
                {
                    if (topCard.attributes.ContainsKey(deck.CardAttributes[i]) && attributeRankings[i][k] >= topCard.attributes[deck.CardAttributes[i]])
                    {
                        topCardRanking[i] = k;
                        noSmallerValues = false;
                        break;
                    }
                }
                if (noSmallerValues)
                    topCardRanking[i] = attributeRankings[i].Count;
            }

            //picking the one with the highest rank
            float highestValue = float.MinValue;
            int highestValueIndex = 0;

            for (int i = 0; i < topCardRanking.Length; ++i)
            {
                if (topCardRanking[i] > highestValue)
                {
                    highestValue = topCardRanking[i];
                    highestValueIndex = i;
                }
            }

            return deck.CardAttributes[highestValueIndex];
        }

    }
}

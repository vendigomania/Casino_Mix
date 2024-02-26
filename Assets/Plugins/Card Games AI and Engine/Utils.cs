using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// A class for general useful methods for the card game AI & engine pack
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Finds the highest integer of the given values
        /// </summary>
        internal static int MaxInt(params int[] values)
        {
            int max = int.MinValue;
            foreach (int value in values)
            {
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <summary>
        /// Finds the smallest integer of the given values
        /// </summary>
        internal static int MinInt(params int[] values)
        {
            int min = int.MaxValue;
            foreach (int value in values)
            {
                if (value < min)
                    min = value;
            }

            return min;
        }

        /// <summary>
        /// Method used to create an independent copy of a 2-dimensional matrix that is an array of lists.
        /// </summary>
        /// <param name="original">The original matrix that you want to make a copy of.</param>
        /// <returns>An independent copy of the matrix given as parameter.</returns>
        internal static List<T>[] IndependentCopyMatrix<T>(List<T>[] original)
        {
            List<T>[] copy = new List<T>[original.Length];
            for (int i = 0; i < original.Length; ++i)
            {
                copy[i] = new List<T>();
                for (int k = 0; k < original[i].Count; ++k)
                {
                    copy[i].Add(original[i][k]);
                }
            }
            return copy;
        }

        /// <summary>
        /// Method used to create an independent copy of a 1-dimensional array.
        /// </summary>
        /// <param name="original">The original array that you want to make a copy of.</param>
        /// <returns>An independent copy of the array given as parameter.</returns>
        internal static T[] IndependentCopyArray<T>(T[] original)
        {
            T[] copy = new T[original.Length];
            for (int i = 0; i < original.Length; ++i)
            {
                copy[i] = original[i];
            }
            return copy;
        }

        /// <summary>
        /// Method used to create an independent copy of a 1-dimensional List.
        /// </summary>
        /// <param name="original">The original list that you want to make a copy of.</param>
        /// <returns>An independent copy of the list given as parameter.</returns>
        internal static List<T> IndependentCopyList<T>(List<T> original)
        {
            List<T> copy = new List<T>();
            for (int i = 0; i < original.Count; ++i)
            {
                copy.Add(original[i]);
            }
            return copy;
        }

        internal static CustomDeck IndependentCopyCustomDeck(CustomDeck original)
        {
            CustomDeck copy = new CustomDeck(original.CardAttributes);

            for (int i = 0; i < original.Cards.Count; ++i)
            {
                copy.AddCard(original.Cards[i]);
            }

            return copy;
        }

        /// <summary>
        /// Compares the two card sets to see if they are equal.
        /// </summary>
        /// <param name="set1">First card set</param>
        /// <param name="set2">Second card set</param>
        /// <returns>True if the sets contain the same cards in same indexes.</returns>
        internal static bool EqualCardSets<T>(List<T>[] set1, List<T>[] set2)
        {
            if (set1 == set2)
                return true;
            if (set1 == null || set2 == null)
                return false;
            if (set1.Length != set2.Length)
                return false;
            for (int i = 0; i < set1.Length; ++i)
            {
                if (!EqualCardLists(set1[i], set2[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compares the two lists of cards to see if they are equal.
        /// </summary>
        /// <param name="list1">First card list</param>
        /// <param name="list2">Second card list</param>
        /// <returns>True if the lists contain the same cards in same indexes.</returns>
        internal static bool EqualCardLists<T>(List<T> list1, List<T> list2)
        {
            if (list1 == list2)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (list1.Count != list2.Count)
                return false;

            for (int k = 0; k < list1.Count; ++k)
            {
                if (!(list1[k]).Equals(list2[k]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Shuffles the given list (cards)
        /// </summary>
        /// <param name="cards">Original list</param>
        /// <returns>Shuffled list.</returns>
        public static List<T> ShuffleCards<T>(List<T> cards)
        {
            List<T> copyOfCards = IndependentCopyList(cards);
            List<T> shuffledCards = new List<T>();
            System.Random random = new System.Random(System.DateTime.UtcNow.Millisecond/2 + 829);
            for (int i = 0; i < cards.Count; ++i)
            {
                int index = random.Next(0, copyOfCards.Count);
                shuffledCards.Add(copyOfCards[index]);
                copyOfCards.RemoveAt(index);
            }

            return shuffledCards;
        }

        /// <summary>
        /// Sorts the given card list.
        /// Sorting order is from 1 to 13, each suit at a time: clubs, diamonds, hearts, spades.
        /// </summary>
        /// <param name="cards">List of cards, unsorted</param>
        /// <param name="ace14">Boolean to specify if ace is treated as 14 instead of 1. False by default</param>
        /// <returns>Sorted list of the cards.</returns>
        public static List<Card> SortCards(List<Card> cards, bool ace14 = false)
        {
            List<Card> sortedCards = new List<Card>();

            if (cards == null || cards.Count == 0)
                return sortedCards;

            int minInd = -1;
            int floorValue = 0;

            for (int c = 0; c < cards.Count; ++c)
            {
                for (int i = 0; i < cards.Count; ++i)
                {
                    int value = cards[i].sortValue(ace14);

                    if ((minInd < 0 || value < cards[minInd].sortValue(ace14)) && value >= floorValue)
                    {
                        minInd = i;

                        if (value == floorValue)
                            break;
                    }
                }
                floorValue = cards[minInd].sortValue(ace14)+1;
                sortedCards.Add(cards[minInd]);
                minInd = -1;
            }

            return sortedCards;
        }

    }
}

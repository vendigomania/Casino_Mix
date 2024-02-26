using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// AI for the card game Casino.
    /// </summary>
    public static class CasinoAI
    {
        /// <summary>
        /// Enumeration to define the difficulty of the AI.
        /// </summary>
        public enum CasinoAIType
        {
            random = 0,
            easy = 1,
            medium = 2,
            hard = 3
        }

        /// <summary>
        /// The type of the AI. As a HeartsAIType enumeration.
        /// </summary>
        public static CasinoAIType AIType = CasinoAIType.hard;

        /// <summary>
        /// The AI makes a move using the specified AI type.
        /// </summary>
        /// <param name="hand">List of current player's hand cards.</param>
        /// <param name="table">List of the cards on the table.</param>
        /// <param name="collected">Array of lists of all players' collected cards.</param>
        /// <returns>Casino move as a CasinoEngine.CasinoMove.</returns>
        public static CasinoEngine.CasinoMove MakeMove(List<Card> hand, List<Card> table, List<Card>[] collected)
        {
            List<CasinoEngine.CasinoMove> possibleMoves = CasinoEngine.PossibleMoves(hand, table);

            switch (AIType)
            {
                case CasinoAIType.random:
                    return MakeMoveRandom(possibleMoves);

                case CasinoAIType.easy:
                    return MakeMoveEasy(possibleMoves);

                case CasinoAIType.medium:
                    return MakeMoveMedium(possibleMoves);

                case CasinoAIType.hard:
                    return MakeMoveHard(possibleMoves, collected);
            }

            //fallback
            return MakeMoveRandom(possibleMoves);
        }

        public static CasinoEngine.CasinoMove MakeMoveRandom(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            if (possibleMoves.Count > 0)
                return possibleMoves[new System.Random().Next(0, possibleMoves.Count)];
            else
                return new CasinoEngine.CasinoMove();
        }

        public static CasinoEngine.CasinoMove MakeMoveEasy(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            List<CasinoEngine.CasinoMove> mostPointMoves = MostPoints(possibleMoves);

            //randomly doing any of the moves that gives the most points
            if (mostPointMoves.Count > 0)
            {
                return mostPointMoves[new System.Random().Next(0, mostPointMoves.Count)];
            }

            return MakeMoveRandom(possibleMoves);
        }

        public static CasinoEngine.CasinoMove MakeMoveMedium(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            List<CasinoEngine.CasinoMove> mostPointMoves = MostPoints(possibleMoves);

            int mostPoints = PointsInAMove(mostPointMoves[0]);

            //if there are points to gain, get them
            if (mostPoints > 0)
            {
                //randomly doing any of the moves that gives the most points
                if (mostPointMoves.Count > 0)
                {
                    return mostPointMoves[new System.Random().Next(0, mostPointMoves.Count)];
                }
            }

            List<CasinoEngine.CasinoMove> mostSpadesMoves = MostSpades(possibleMoves);

            int mostSpades = SpadesInAMove(mostSpadesMoves[0]);

            if (mostSpades > 0)
            {
                //randomly doing any of the moves that gives the most spades
                if (mostSpadesMoves.Count > 0)
                {
                    return mostSpadesMoves[new System.Random().Next(0, mostSpadesMoves.Count)];
                }
            }

            //taking as many cards as possible if no spades are available
            List<CasinoEngine.CasinoMove> mostCardsMoves = MostCards(possibleMoves);

            //randomly doing any of the moves that gives the most cards
            if (mostCardsMoves.Count > 0)
            {
                return mostCardsMoves[new System.Random().Next(0, mostCardsMoves.Count)];
            }


            return MakeMoveRandom(possibleMoves);
        }

        public static CasinoEngine.CasinoMove MakeMoveHard(List<CasinoEngine.CasinoMove> possibleMoves, List<Card>[] collected)
        {
            List<CasinoEngine.CasinoMove> mostPointMoves = MostPoints(possibleMoves);

            int mostPoints = PointsInAMove(mostPointMoves[0]);

            //if there are points to gain, get them
            if (mostPoints > 0)
            {
                //randomly doing any of the moves that gives the most points
                if (mostPointMoves.Count > 0)
                {
                    return mostPointMoves[new System.Random().Next(0, mostPointMoves.Count)];
                }
            }

            if (!SpadesFinished(collected))
            {

                List<CasinoEngine.CasinoMove> mostSpadesMoves = MostSpades(possibleMoves);

                int mostSpades = SpadesInAMove(mostSpadesMoves[0]);

                if (mostSpades > 0)
                {
                    //randomly doing any of the moves that gives the most spades
                    if (mostSpadesMoves.Count > 0)
                    {
                        return mostSpadesMoves[new System.Random().Next(0, mostSpadesMoves.Count)];
                    }
                }
            }

            //taking as many cards as possible if no spades are available
            List<CasinoEngine.CasinoMove> mostCardsMoves = MostCards(possibleMoves);

            int mostCards = CardsInAMove(mostCardsMoves[0]);

            if (mostCards > 0)
            {
                //randomly doing any of the moves that gives the most cards
                if (mostCardsMoves.Count > 0)
                {
                    return mostCardsMoves[new System.Random().Next(0, mostCardsMoves.Count)];
                }
            }

            //discarding a card that's the least beneficial for the opponents

            //primarily no point cards or spades
            List<CasinoEngine.CasinoMove> primaryDiscards = new List<CasinoEngine.CasinoMove>();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (CardPoints(possibleMoves[i].handCard) == 0 && possibleMoves[i].handCard.suit != Suit.spade)
                    primaryDiscards.Add(possibleMoves[i]);
            }

            if (primaryDiscards.Count > 0)
            {
                return primaryDiscards[new System.Random().Next(0, primaryDiscards.Count)];
            }

            //secondary discards: no point cards
            List<CasinoEngine.CasinoMove> secondaryDiscards = new List<CasinoEngine.CasinoMove>();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (CardPoints(possibleMoves[i].handCard) == 0)
                    secondaryDiscards.Add(possibleMoves[i]);
            }

            if (secondaryDiscards.Count > 0)
            {
                return secondaryDiscards[new System.Random().Next(0, secondaryDiscards.Count)];
            }

            //tertiary discards: no 10 of diamonds
            List<CasinoEngine.CasinoMove> tertiaryDiscards = new List<CasinoEngine.CasinoMove>();
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                if (CardPoints(possibleMoves[i].handCard) < 2)
                    tertiaryDiscards.Add(possibleMoves[i]);
            }

            if (tertiaryDiscards.Count > 0)
            {
                return tertiaryDiscards[new System.Random().Next(0, tertiaryDiscards.Count)];
            }

            return MakeMoveRandom(possibleMoves);
        }

        private static bool SpadesFinished(List<Card>[] collected)
        {
            int[] spadeCounts = new int[collected.Length];
            int totalSpades = 0;
            for (int i = 0; i < collected.Length; ++i)
            {
                int spades = 0;
                for (int k = 0; k < collected[i].Count; ++k)
                {
                    if (collected[i][k].suit == Suit.spade)
                        spades++;
                }

                spadeCounts[i] = spades;
                totalSpades += spades;
            }
            if (totalSpades == 13)
                return true;

            int mostSpades = Utils.MaxInt(spadeCounts);
            int secondMostSpades = 0;
            bool firstHighest = true;
            for (int i = 0; i < spadeCounts.Length; ++i)
            {
                if (spadeCounts[i] > secondMostSpades)
                {
                    if (spadeCounts[i] == mostSpades && firstHighest)
                        firstHighest = false;
                    else
                        secondMostSpades = spadeCounts[i];
                }
            }

            if (mostSpades - secondMostSpades > 13 - totalSpades)
                return true;

            return false;
        }

        private static int CardPoints(Card card)
        {
            if (card.suit == Suit.diamond && card.value == 10)
                return 2;
            else if ((card.suit == Suit.spade && card.value == 2) || card.value == 1)
                return 1;
            return 0;
        }

        private static int PointsInAMove(CasinoEngine.CasinoMove move)
        {
            if (move.tableCards.Count < 1)
                return 0;

            int points = CardPoints(move.handCard);

            for (int k = 0; k < move.tableCards.Count; ++k)
            {

                points += CardPoints(move.tableCards[k]);
            }

            return points;
        }

        private static List<CasinoEngine.CasinoMove> MostPoints(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            int[] movePoints = new int[possibleMoves.Count];
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                movePoints[i] = PointsInAMove(possibleMoves[i]);
            }

            int mostPoints = Utils.MaxInt(movePoints);

            List<CasinoEngine.CasinoMove> mostPointMoves = new List<CasinoEngine.CasinoMove>();

            for (int i = 0; i < movePoints.Length; ++i)
            {
                if (movePoints[i] == mostPoints)
                    mostPointMoves.Add(possibleMoves[i]);
            }

            return mostPointMoves;
        }

        private static int SpadesInAMove(CasinoEngine.CasinoMove move)
        {
            int spades = 0;
            if (move.handCard.suit == Suit.spade && move.tableCards.Count > 0)
                spades++;
            for (int k = 0; k < move.tableCards.Count; ++k)
            {
                if (move.tableCards[k].suit == Suit.spade)
                    spades++;
            }

            return spades;
        }

        private static List<CasinoEngine.CasinoMove> MostSpades(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            int[] totalSpades = new int[possibleMoves.Count];
            for (int i = 0; i < possibleMoves.Count; ++i)
            {

                totalSpades[i] = SpadesInAMove(possibleMoves[i]);
            }

            int mostSpades = Utils.MaxInt(totalSpades);

            List<CasinoEngine.CasinoMove> mostSpadesMoves = new List<CasinoEngine.CasinoMove>();

            for (int i = 0; i < totalSpades.Length; ++i)
            {
                if (totalSpades[i] == mostSpades)
                    mostSpadesMoves.Add(possibleMoves[i]);
            }

            return mostSpadesMoves;
        }

        private static int CardsInAMove(CasinoEngine.CasinoMove move)
        {
            int cards = 0;
            if (move.tableCards.Count > 0)
                cards = move.tableCards.Count + 1;

            return cards;
        }

        private static List<CasinoEngine.CasinoMove> MostCards(List<CasinoEngine.CasinoMove> possibleMoves)
        {
            int[] totalCards = new int[possibleMoves.Count];
            for (int i = 0; i < possibleMoves.Count; ++i)
            {
                totalCards[i] = CardsInAMove(possibleMoves[i]);
            }

            int mostCards = Utils.MaxInt(totalCards);

            List<CasinoEngine.CasinoMove> mostCardsMoves = new List<CasinoEngine.CasinoMove>();

            for (int i = 0; i < totalCards.Length; ++i)
            {
                if (totalCards[i] == mostCards)
                    mostCardsMoves.Add(possibleMoves[i]);
            }

            return mostCardsMoves;
        }
    }
}

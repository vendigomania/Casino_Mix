using System.Collections;
using System.Collections.Generic;

namespace CGAIE
{
    /// <summary>
    /// Enumeration for card suits.
    /// </summary>
    public enum Suit
    {
        club = 1,
        diamond = 2,
        heart = 3,
        spade = 4
    }

    /// <summary>
    /// Class implementing a playing card.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Constructor for a card.
        /// </summary>
        /// <param name="_suit">Suit as an enumeration. On of the four traditional playing card suits.</param>
        /// <param name="_value">Value of the card. From 1 to 13.</param>
        public Card(Suit _suit, ushort _value)
        {
            suit = _suit;
            value = _value;
        }

        /// <summary>
        /// Suit as an enumeration. On of the four traditional playing card suits.
        /// </summary>
        public readonly Suit suit;
        /// <summary>
        /// Value of the card. From 1 to 13.
        /// </summary>
        public readonly ushort value;

        public static bool operator ==(Card a, Card b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.suit == b.suit && a.value == b.value;
        }

        public static bool operator !=(Card a, Card b)
        {
            return a.suit != b.suit || a.value != b.value;
        }

        public override bool Equals(object obj)
        {
            return this == (Card)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Displays the card as a string in the format "suit value"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "" + suit + " " + value;
        }

        /// <summary>
        /// Returns the sort value of the card. The sort value is a positive integer, going from 1 to 13 or 2 to 14 for each suit.
        /// The suits are in order: clubs, diamonds, hearts, spades.
        /// </summary>
        /// <param name="ace14">Boolean to specify if ace is considered 14 instead of 1. False by default.</param>
        /// <returns>The sort value of the card as an integer.</returns>
        public int sortValue(bool ace14 = false)
        {
            if (ace14 && value == 1)
                return (int)(suit - 1) * 13 + 13;
            else if (ace14)
                return (int)(suit - 1) * 13 + value - 1;
            else
                return (int)(suit - 1) * 13 + value;
        }
    }

    /// <summary>
    /// Class implementing a typical deck of 52 playing cards.
    /// </summary>
    public class Deck
    {
        private readonly System.Random random;
        private List<Card> cards;
        /// <summary>
        /// List of the cards in the deck
        /// </summary>
        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }

        /// <summary>
        /// Create a new deck. Populated with normal 52 playing cards by default.
        /// </summary>
        /// <param name="empty">Optionally the deck can be empty.</param>
        public Deck(bool empty = false)
        {
            random = new System.Random();

            cards = new List<Card>();

            if (!empty)
            {
                for (int s = 1; s <= 4; ++s)
                {
                    for (int v = 1; v <= 13; ++v)
                    {
                        cards.Add(new Card((Suit)s, (ushort)v));
                    }
                }
            }
        }

        /// <summary>
        /// Deals one random card from the deck.
        /// </summary>
        /// <returns>A random card from the deck. Null if the deck is empty.</returns>
        public Card Deal()
        {
            if (cards.Count > 0)
            {
                int ind = random.Next(cards.Count);
                Card card = cards[ind];
                cards.RemoveAt(ind);
                return card;
            }
            return null;
        }
    }


    /// <summary>
    /// Class implementing a custom card.
    /// </summary>
    public class CustomCard
    {
        /// <summary>
        /// Constructor for a custom card.
        /// </summary>
        /// <param name="_id">Unique identifier of the card. This id can be used to match the correct sprite. Lowest ID should be 1.</param>
        /// <param name="_attributes">Dictionary of the card's attributes. The keys should match the list of attributes in the relevant deck.</param>
        public CustomCard(uint _id, Dictionary<string, float> _attributes)
        {
            ID = _id;
            attributes = _attributes;
        }

        /// <summary>
        /// Unique identifier of the card. This id can be used to match the correct sprite. Lowest ID should be 1.
        /// </summary>
        public readonly uint ID;
        /// <summary>
        /// Dictionary of the card's attributes. The keys should match the list of attributes in the relevant deck.
        /// </summary>
        public readonly Dictionary<string, float> attributes;

        public static bool operator ==(CustomCard a, CustomCard b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.ID == b.ID && a.attributes.Equals(b.attributes);
        }

        public static bool operator !=(CustomCard a, CustomCard b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (CustomCard)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "CustomCard " + ID;
        }
    }

    /// <summary>
    /// Class implementing any kind of deck with any amount of cards and thier attributes.
    /// </summary>
    public class CustomDeck
    {
        private readonly System.Random random;

        private List<CustomCard> cards;

        /// <summary>
        /// List of the CustomCards in this deck.
        /// </summary>
        public List<CustomCard> Cards
        {
            get
            {
                return cards;
            }
        }

        /// <summary>
        /// List of all the attributes that the Custom Cards in this deck have.
        /// </summary>
        public readonly string[] CardAttributes;

        /// <summary>
        /// Constructor for a custom deck. The deck is empty at the start. Use AddCard()-method to add cards.
        /// </summary>
        public CustomDeck(string[] cardAttributes)
        {
            random = new System.Random();

            cards = new List<CustomCard>();
            CardAttributes = cardAttributes;
        }

        /// <summary>
        /// Adds the given card in the deck if it contains all required attributes.
        /// </summary>
        /// <param name="card">Custom Card to be added to the deck</param>
        /// <returns>True if the card had all the required attributes. False otherwise.</returns>
        public bool AddCard(CustomCard card)
        {
            for (int i = 0; i < CardAttributes.Length; ++i)
            {
                if (!card.attributes.ContainsKey(CardAttributes[i]))
                {
                    return false;
                }
            }

            cards.Add(card);
            return true;
        }

        /// <summary>
        /// Deals one random card from the deck.
        /// </summary>
        /// <returns>A random card from the deck. Null if the deck is empty.</returns>
        public CustomCard Deal()
        {
            if (cards.Count > 0)
            {
                int ind = random.Next(cards.Count);
                CustomCard card = cards[ind];
                cards.RemoveAt(ind);
                return card;
            }
            return null;
        }
    }

}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CGAIE.DemoBase;

/// <summary>
/// The namespace for all Card Game AI and Engine Pack -related elements.
/// </summary>
namespace CGAIE
{

    /// <summary>
    /// A class that handles the Top Trumps demo game
    /// </summary>
    public class TopTrumpsDemo : ICardGameDemo
    {
        public TopTrumpsEngine.PlayerType[] TopTrumpsSeats = new TopTrumpsEngine.PlayerType[4] { TopTrumpsEngine.PlayerType.local, TopTrumpsEngine.PlayerType.AI, TopTrumpsEngine.PlayerType.AI, TopTrumpsEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (TopTrumpsEngine.Hands == null || TopTrumpsEngine.Hands.Length == 0 || TopTrumpsEngine.Hands[TopTrumpsEngine.CurrentPlayer - 1] == null)
                    return;

                bool TopTrumpsHandsChanged = !Utils.EqualCardSets(previousCustomCards, TopTrumpsEngine.Hands);
                bool TopTrumpsUnshuffledHandsChanged = !Utils.EqualCardSets(previousUnShuffledCustomCards, TopTrumpsEngine.UnShuffledHands);

                if (TopTrumpsHandsChanged || TopTrumpsUnshuffledHandsChanged)
                {
                    if (TopTrumpsEngine.seats[TopTrumpsEngine.CurrentPlayer - 1] == TopTrumpsEngine.PlayerType.local)
                        mainPlayer = TopTrumpsEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < TopTrumpsSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= TopTrumpsSeats.Length)
                            opponentIndex -= TopTrumpsSeats.Length;

                        //updating the amount of cards each opponent has
                        opponentDeckTexts[i].text = "" + (TopTrumpsEngine.Hands[opponentIndex].Count + TopTrumpsEngine.UnShuffledHands[opponentIndex].Count);

                        opponentNames[i].GetComponent<Text>().text = "Player " + (opponentIndex + 1);
                    }


                    if (TopTrumpsEngine.Hands[mainPlayer - 1].Count < 1)
                    {
                        currentPlayerHand[0].enabled = false;
                    }
                    else
                    {
                        currentPlayerHand[0].enabled = true;
                        currentPlayerHand[0].sprite = cardSprites[TopTrumpsEngine.Hands[mainPlayer-1][0].ID - 1];
                    }

                    deckText.text = "" + (Utils.MaxInt(0,TopTrumpsEngine.Hands[mainPlayer - 1].Count - 1, 0) + TopTrumpsEngine.UnShuffledHands[mainPlayer-1].Count);

                    currentPlayerName.GetComponent<Text>().text = "Player " + mainPlayer;


                    previousPlayer = TopTrumpsEngine.CurrentPlayer;
                    previousCustomCards = Utils.IndependentCopyMatrix(TopTrumpsEngine.Hands);
                    previousUnShuffledCustomCards = Utils.IndependentCopyMatrix(TopTrumpsEngine.UnShuffledHands);
                }

                if (!Utils.EqualCardLists(previousCustomCardTable, TopTrumpsEngine.Table))
                {
                    statusChange = true;
                    for (int i = 0; i < tableCards.Length; ++i)
                    {
                        if (TopTrumpsEngine.Table.Count > i)
                        {
                            tableCards[i].sprite = cardSprites[TopTrumpsEngine.Table[i].ID-1];
                            tableCards[i].enabled = true;
                        }
                        else
                        {
                            tableCards[i].enabled = false;
                        }
                    }

                    previousCustomCardTable = Utils.IndependentCopyList(TopTrumpsEngine.Table);
                }

                //displaying the "pot" after a tie using the "used cards" section
                for (int i = 0; i < usedCardSRs.Length; ++i)
                {
                    if (TopTrumpsEngine.AdditionalCards.Count > i && !usedCardSRs[i].enabled)
                    {
                        usedCardSRs[i].sprite = cardSprites[TopTrumpsEngine.AdditionalCards[i].ID - 1];
                        usedCardSRs[i].enabled = true;
                    }
                    else if (TopTrumpsEngine.AdditionalCards.Count <= i && usedCardSRs[i].enabled)
                    {
                        usedCardSRs[i].enabled = false;
                    }
                }

                //changing the status text
                if (statusChange || previousTopTrumpsState != TopTrumpsEngine.gameState)
                {
                    switch (TopTrumpsEngine.gameState)
                    {
                        case TopTrumpsEngine.GameState.normalTurnAfterTie:
                            gameStateText.GetComponent<Text>().text = "Round was a tie. Player " + TopTrumpsEngine.CurrentPlayer + "'s turn to select an attribute.";
                            break;

                        case TopTrumpsEngine.GameState.normalTurn:
                            if (TopTrumpsEngine.CurrentAttribute.Equals("")) //first round or after reset
                                gameStateText.GetComponent<Text>().text = "Player " + TopTrumpsEngine.CurrentPlayer + "'s turn to select an attribute.";
                            else
                                gameStateText.GetComponent<Text>().text = "Round won by Player " + TopTrumpsEngine.CurrentPlayer + ". Their turn to select an attribute.";
                            break;

                        case TopTrumpsEngine.GameState.endOfRound:
                            gameStateText.GetComponent<Text>().text = "The selected attribute was: " + TopTrumpsEngine.CurrentAttribute + ".";
                            break;

                        case TopTrumpsEngine.GameState.inactive:
                            string winner = "";
                            for (int i = 0; i < TopTrumpsEngine.Hands.Length; ++i)
                            {
                                if (TopTrumpsEngine.Hands[i].Count + TopTrumpsEngine.UnShuffledHands[i].Count > 0)
                                {
                                    winner = "Player " + (i + 1);
                                }
                            }
                            gameStateText.GetComponent<Text>().text = "The game has ended. Winner: " + winner;
                            break;
                    }

                    previousTopTrumpsState = TopTrumpsEngine.gameState;
                }

            }
            catch
            {
                //Some of the variables are changed in another thread, rarely causing a nullreference or argumentoutofrange exception
                //They are harmless, so they are caught here
            }
        }

        /// <summary>
        /// Called at the start of the demo. Initializes the demo and the engine
        /// </summary>
        /// <param name="parameters">all relevant parameters for the game such as player and AI types.</param>
        void ICardGameDemo.StartDemo(params object[] parameters)
        {
            TopTrumpsSeats = (TopTrumpsEngine.PlayerType[])parameters[0];
            TopTrumpsEngine.seats = TopTrumpsSeats;
            TopTrumpsAI.AIType = (TopTrumpsAI.TopTrumpsAIType)parameters[1];
            TopTrumpsEngine.NoTieBreakers = (bool)parameters[2];
            TopTrumpsEngine.PrivateTieBreakers = (bool)parameters[3];
            TopTrumpsEngine.LowestValueBeatsHighest = (bool)parameters[4];
            uint duplicates = (uint)parameters[5];
            bool customDeck = (bool)parameters[6];
            TopTrumpsEngine.deck = new CustomDeck((string[])parameters[7]);

            //normal playing card deck as the custom cards
            if (!customDeck)
            {
                for (int s = 0; s < 4; ++s)
                {
                    for (int v = 2; v < 15; ++v)
                    {
                        Dictionary<string, float> newCardAttributes = new Dictionary<string, float>();
                        for (int i = 0; i < TopTrumpsEngine.deck.CardAttributes.Length; ++i)
                        {
                            switch (TopTrumpsEngine.deck.CardAttributes[i])
                            {
                                case "Value":
                                    //Aces are value 14, everything else is as much as their number says
                                    if (v == 2)
                                        newCardAttributes.Add(TopTrumpsEngine.deck.CardAttributes[i], 14);
                                    else
                                        newCardAttributes.Add(TopTrumpsEngine.deck.CardAttributes[i], v - 1);
                                    break;

                                case "Suit":
                                    newCardAttributes.Add(TopTrumpsEngine.deck.CardAttributes[i], s);
                                    break;
                            }
                        }
                        CustomCard newCard = new CustomCard((uint)(s * 13 + (v - 1)), newCardAttributes);

                        //adding the card multiple times for each duplicate
                        for (int i = 0; i < duplicates + 1; ++i)
                        {
                            TopTrumpsEngine.deck.AddCard(newCard);
                        }
                    }
                }
            }
            else
            {
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 6, 10976, 72 }, 1));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 6, 5261, 25 }, 2));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { float.MaxValue, 8495, 750 }, 3));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { float.MaxValue, 5026, 90 }, 4));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 4, 10000, 4 }, 5));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 4, 4225, 24 }, 6));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 4, 8500, 800 }, 7));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 4, 8000, 350 }, 8));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 7, 5247, 10 }, 9));
                TopTrumpsEngine.deck.AddCard(CreateCustomCard(TopTrumpsEngine.deck.CardAttributes, new float[3] { 3, 4725, 100 }, 10));
            }

            CreateTopTrumpsDemoScene();
            TopTrumpsEngine.ResetGame();
        }

        private CustomCard CreateCustomCard(string[] attributeNames, float[] attributeValues, uint id)
        {
            Dictionary<string, float> newCardAttributes = new Dictionary<string, float>();
            for (int i = 0; i < attributeNames.Length; ++i)
            {
                if (attributeValues.Length > i)
                    newCardAttributes.Add(attributeNames[i], attributeValues[i]);
            }
            CustomCard newCard = new CustomCard(id, newCardAttributes);

            return newCard;
        }

        /// <summary>
        /// Creates game objects in the scene that are relevant for Top Trumps
        /// </summary>
        private void CreateTopTrumpsDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[2];
            opponentHands = new SpriteRenderer[TopTrumpsSeats.Length - 1][];
            opponentDeckTexts = new Text[TopTrumpsSeats.Length - 1];
            opponentNames = new GameObject[TopTrumpsSeats.Length - 1];
            tableCards = new SpriteRenderer[TopTrumpsSeats.Length];
            usedCardSRs = new SpriteRenderer[TopTrumpsSeats.Length*3];


            //current player hand
            //top card
            GameObject topCard = new GameObject("Top card");
            SpriteRenderer srTop = topCard.AddComponent<SpriteRenderer>();
            int cardIndexTop = cardTextures.Length-1;
            if (TopTrumpsEngine.seats[TopTrumpsEngine.CurrentPlayer - 1] == TopTrumpsEngine.PlayerType.local)
            {
                if (0 < TopTrumpsEngine.Hands[TopTrumpsEngine.CurrentPlayer - 1].Count)
                {
                    cardIndexTop = (int)(TopTrumpsEngine.Hands[TopTrumpsEngine.CurrentPlayer - 1][0].ID - 1);
                }
                else
                    srTop.enabled = false;
            }
            Texture2D cardTextureTop = cardTextures[cardIndexTop];
            srTop.sprite = cardSprites[cardIndexTop];
            srTop.sortingOrder = 0;
            topCard.transform.parent = board.transform;
            topCard.transform.localPosition = new Vector3(-1.5f, -4f, 0);
            topCard.transform.localScale = new Vector3(100f / cardTextureTop.width, 384f / 256f * 100f / cardTextureTop.height, 1);
            currentPlayerHand[0] = srTop;

            //The rest of the player's hand
            GameObject cardPile = new GameObject("Top card");
            SpriteRenderer srPile = cardPile.AddComponent<SpriteRenderer>();
            int cardIndexPile = cardTextures.Length-1;
            Texture2D cardTexturePile = cardTextures[cardIndexPile];
            srPile.sprite = cardSprites[cardIndexPile];
            srPile.sortingOrder = 0;
            cardPile.transform.parent = board.transform;
            cardPile.transform.localPosition = new Vector3(1f, -4f, 0);
            cardPile.transform.localScale = new Vector3(100f / cardTexturePile.width, 384f / 256f * 100f / cardTexturePile.height, 1);
            currentPlayerHand[1] = srPile;

            //number on top of the pile
            GameObject dt = new GameObject("deck text");
            dt.layer = 5; //default UI layer
            dt.transform.parent = UICanvas.transform;
            Text dtt = dt.AddComponent<Text>();
            dtt.font = defaultFont;
            dtt.fontSize = 24;
            dtt.color = Color.white;
            dtt.rectTransform.sizeDelta = new Vector2(71.6f, 113.3f);
            dtt.rectTransform.localPosition = new Vector3(108.1f, -236.3f, 0);
            dtt.rectTransform.localScale = new Vector3(1, 1, 1);
            dtt.text = "" + (TopTrumpsEngine.Hands[TopTrumpsEngine.CurrentPlayer - 1].Count + TopTrumpsEngine.UnShuffledHands[TopTrumpsEngine.CurrentPlayer - 1].Count - 1);
            dtt.alignment = TextAnchor.MiddleCenter;
            deckText = dtt;

            //current player name
            currentPlayerName = new GameObject("current player name");
            currentPlayerName.layer = 5; //default UI layer
            currentPlayerName.transform.parent = UICanvas.transform;
            Text nameText = currentPlayerName.AddComponent<Text>();
            nameText.font = defaultFont;
            nameText.fontSize = 18;
            nameText.color = Color.white;
            nameText.rectTransform.sizeDelta = new Vector2(200, 50);
            nameText.rectTransform.localPosition = new Vector3(0, -320f, 0);
            nameText.rectTransform.localScale = new Vector3(1, 1, 1);
            nameText.text = "Player " + TopTrumpsEngine.CurrentPlayer;
            nameText.alignment = TextAnchor.MiddleCenter;


            //opponents' hands
            for (int i = 0; i < TopTrumpsSeats.Length - 1; ++i)
            {
                //only show the amount of cards for opponents as a number
                opponentHands[i] = new SpriteRenderer[1];

                GameObject opponentCard = new GameObject("opponent " + (i + 1) + " cards");
                SpriteRenderer opponentSr = opponentCard.AddComponent<SpriteRenderer>();
                int opponentCardIndex = cardTextures.Length-1;
                Texture2D cardTexture = cardTextures[opponentCardIndex];
                opponentSr.sprite = cardSprites[opponentCardIndex];
                opponentSr.sortingOrder = 1;
                opponentSr.enabled = true;
                opponentCard.transform.parent = board.transform;
                opponentCard.transform.localPosition = new Vector3(-6.6f + i % 2 * 3f, 3f - (i / 2) * 1.6f, 0);
                opponentCard.transform.localScale = new Vector3(50f / cardTexture.width, 384f / 256f * 50f / cardTexture.height, 1);
                opponentHands[i][0] = opponentSr;

                //number on top of the opponents' piles
                GameObject opponentDt = new GameObject("deck text");
                opponentDt.layer = 5; //default UI layer
                opponentDt.transform.parent = UICanvas.transform;
                Text opponentDtt = opponentDt.AddComponent<Text>();
                opponentDtt.font = defaultFont;
                opponentDtt.fontSize = 20;
                opponentDtt.color = Color.white;
                opponentDtt.rectTransform.sizeDelta = new Vector2(40f, 50f);
                opponentDtt.rectTransform.localPosition = new Vector3(-457.9f + 216f * (i%2), 242.7f - 114.7f * (i/2), 0);
                opponentDtt.rectTransform.localScale = new Vector3(1, 1, 1);
                opponentDtt.text = "" + (TopTrumpsEngine.Hands[TopTrumpsEngine.CurrentPlayer - 1].Count + TopTrumpsEngine.UnShuffledHands[TopTrumpsEngine.CurrentPlayer - 1].Count - 1);
                opponentDtt.alignment = TextAnchor.MiddleCenter;
                opponentDeckTexts[i] = opponentDtt;

                //Creating the name texts 
                opponentNames[i] = new GameObject("opponent name " + (i + 1));
                opponentNames[i].layer = 5; //default UI layer
                opponentNames[i].transform.parent = UICanvas.transform;
                nameText = opponentNames[i].AddComponent<Text>();
                nameText.font = defaultFont;
                nameText.fontSize = 18;
                nameText.color = Color.white;
                nameText.rectTransform.sizeDelta = new Vector2(200, 50);
                nameText.rectTransform.localPosition = new Vector3(216 * (i % 2) - 460f, 200f - 115 * (i / 2), 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2);
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            for (int i = 0; i < TopTrumpsSeats.Length; ++i)
            {
                GameObject tableCard = new GameObject("Table card");
                SpriteRenderer srTable = tableCard.AddComponent<SpriteRenderer>();
                int tableCardIndex = 0;
                Texture2D tableCardTexture = cardTextures[tableCardIndex];
                srTable.sprite = cardSprites[tableCardIndex];
                srTable.sortingOrder = 0;
                srTable.enabled = false;
                tableCard.transform.parent = board.transform;
                tableCard.transform.localPosition = new Vector3(-2f + i, 0.5f, 0);
                tableCard.transform.localScale = new Vector3(100f / tableCardTexture.width, 384f / 256f * 100f / tableCardTexture.height, 1);
                tableCards[i] = srTable;
            }


            //additional cards (the "pot" after a tie)
            for (int i = 0; i < usedCardSRs.Length; ++i)
            {
                GameObject usedCard = new GameObject("used card");
                SpriteRenderer usedCardsr = usedCard.AddComponent<SpriteRenderer>();
                Texture2D usedCardTexture = cardTextures[cardTextures.Length-1];
                usedCardsr.sprite = cardSprites[cardTextures.Length-1];
                usedCardsr.sortingOrder = i;
                usedCardsr.enabled = false;
                usedCardSRs[i] = usedCardsr;
                usedCard.transform.parent = board.transform;
                usedCard.transform.localPosition = new Vector3(-2f + i*0.2f, -2f, 0);
                usedCard.transform.localScale = new Vector3(70f / usedCardTexture.width, 384f / 256f * 70f / usedCardTexture.height, 1);
            }

            //Creating attribute buttons
            for (int i = 0; i < TopTrumpsEngine.deck.CardAttributes.Length; ++i)
            {
                //attribute -button
                GameObject hitb = new GameObject("Attribute button " + (i+1));
                hitb.layer = 5; //default UI layer
                hitb.transform.parent = UICanvas.transform;
                Image hitbi = hitb.AddComponent<Image>();
                hitbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
                hitbi.preserveAspect = true;
                Button hitbb = hitb.AddComponent<Button>();
                hitbb.targetGraphic = hitbi;
                hitbi.rectTransform.sizeDelta = new Vector2(100, 50);
                hitbi.rectTransform.localPosition = new Vector3(350, 150 -i * 100, 0);
                hitbi.rectTransform.localScale = new Vector3(1, 1, 1);
                GameObject hitbt = new GameObject("Attribute text");
                hitbt.layer = 5; //default UI layer
                hitbt.transform.parent = hitb.transform;
                Text hitbtt = hitbt.AddComponent<Text>();
                hitbtt.font = defaultFont;
                hitbtt.fontSize = 15;
                hitbtt.color = Color.white;
                hitbtt.rectTransform.sizeDelta = new Vector2(100, 50);
                hitbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
                hitbtt.rectTransform.localScale = new Vector3(1, 1, 1);
                hitbtt.text = TopTrumpsEngine.deck.CardAttributes[i];
                hitbtt.alignment = TextAnchor.MiddleCenter;
                int passedIndex = i;
                hitbb.onClick.AddListener(delegate { TopTrumpsAttributeClicked(passedIndex); });
            }


            previousCustomCards = Utils.IndependentCopyMatrix(TopTrumpsEngine.Hands);
            previousUnShuffledCustomCards = Utils.IndependentCopyMatrix(TopTrumpsEngine.UnShuffledHands);
            previousPlayer = TopTrumpsEngine.CurrentPlayer;
            previousCustomCardTable = Utils.IndependentCopyList(TopTrumpsEngine.Table);
            previousTopTrumpsState = TopTrumpsEngine.GameState.inactive;
            gameStateText.GetComponent<Text>().text = "It is player " + TopTrumpsEngine.CurrentPlayer + "'s turn to select an attribute.";


            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < TopTrumpsSeats.Length; ++i)
            {
                if (TopTrumpsSeats[i] == TopTrumpsEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = TopTrumpsEngine.CurrentPlayer;
        }

        void TopTrumpsAttributeClicked(int index)
        {
            TopTrumpsEngine.LocalMove(TopTrumpsEngine.deck.CardAttributes[index]);
        }
    }
}
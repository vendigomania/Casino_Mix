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
    /// A class that handles the Casino demo game
    /// </summary>
    public class CasinoDemo : ICardGameDemo
    {
        public CasinoEngine.PlayerType[] casinoSeats = new CasinoEngine.PlayerType[2] { CasinoEngine.PlayerType.local, CasinoEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (CasinoEngine.Hands == null || CasinoEngine.Hands.Length == 0 || CasinoEngine.Hands[CasinoEngine.CurrentPlayer - 1] == null)
                    return;

                deckText.text = CasinoEngine.cardsInDeck + "";

                bool casinoHandsChanged = !Utils.EqualCardSets(previousCards, CasinoEngine.Hands);

                if (previousPlayer != CasinoEngine.CurrentPlayer || casinoHandsChanged)
                {
                    //clearing highlights
                    for (int i = 0; i < currentPlayerHandHighlights.Length; ++i)
                    {
                        currentPlayerHandHighlights[i].enabled = false;
                    }
                    for (int i = 0; i < tableCardsHighlights.Length; ++i)
                    {
                        tableCardsHighlights[i].enabled = false;
                    }

                    if (CasinoEngine.seats[CasinoEngine.CurrentPlayer - 1] == CasinoEngine.PlayerType.local)
                        mainPlayer = CasinoEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < casinoSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= casinoSeats.Length)
                            opponentIndex -= casinoSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (CasinoEngine.Hands == null || CasinoEngine.Hands.Length <= opponentIndex || CasinoEngine.Hands[opponentIndex] == null || CasinoEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                opponentHands[i][k].enabled = false;
                            }
                            else
                            {
                                opponentHands[i][k].enabled = true;
                            }
                        }

                    }

                    //applying any changes in current player's hand
                    for (int i = 0; i < currentPlayerHand.Length; ++i)
                    {
                        if (CasinoEngine.Hands[mainPlayer - 1].Count < i + 1)
                        {
                            currentPlayerHand[i].enabled = false;
                        }
                        else
                        {
                            int sprite = 52;
                            if (CasinoEngine.seats[mainPlayer - 1] == CasinoEngine.PlayerType.local)
                                sprite = CasinoEngine.Hands[mainPlayer - 1][i].sortValue() - 1;
                            currentPlayerHand[i].sprite = cardSprites[sprite];
                            currentPlayerHand[i].enabled = true;
                        }
                    }




                    previousPlayer = CasinoEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(CasinoEngine.Hands);
                }

                //updating score for players
                int playerIndex = mainPlayer - 1;
                currentPlayerName.GetComponent<Text>().text = "Player " + (playerIndex + 1) + "\n" + CasinoEngine.Score[playerIndex];
                for (int i = 0; i < casinoSeats.Length - 1; ++i)
                {
                    playerIndex++;
                    if (playerIndex >= casinoSeats.Length)
                        playerIndex -= casinoSeats.Length;
                    opponentNames[i].GetComponent<Text>().text = "Player " + (playerIndex + 1) + "\n" + CasinoEngine.Score[playerIndex];
                }

                //displaying the most recently used card if applicable
                if (CasinoEngine.gameState == CasinoEngine.GameState.endOfTurn && !usedCardSR.enabled)
                {
                    usedCardSR.sprite = cardSprites[CasinoEngine.UsedCard.sortValue() - 1];
                    usedCardSR.enabled = true;
                }
                else if (CasinoEngine.gameState != CasinoEngine.GameState.endOfTurn && usedCardSR.enabled)
                {
                    usedCardSR.enabled = false;
                }


                if (previousTable.Count != CasinoEngine.Table.Count)
                {
                    statusChange = true;

                    //applying any changes in table cards
                    for (int i = 0; i < tableCards.Length; ++i)
                    {
                        if (CasinoEngine.Table.Count < i + 1)
                        {
                            tableCards[i].enabled = false;
                        }
                        else
                        {
                            tableCards[i].sprite = cardSprites[CasinoEngine.Table[i].sortValue() - 1];
                            tableCards[i].enabled = true;
                        }
                    }

                    previousTable = Utils.IndependentCopyList(CasinoEngine.Table);
                }

                if (statusChange || previousCasinoState != CasinoEngine.gameState)
                {
                    switch (CasinoEngine.gameState)
                    {
                        case CasinoEngine.GameState.cardDealing:
                            gameStateText.GetComponent<Text>().text = "Cards are being dealt to players.";
                            break;

                        case CasinoEngine.GameState.normalTurn:
                            gameStateText.GetComponent<Text>().text = "It is player " + CasinoEngine.CurrentPlayer + "'s turn.";
                            break;

                        case CasinoEngine.GameState.inactive:
                            string winners = "";
                            int winningScore = Utils.MaxInt(CasinoEngine.Score);
                            for (int i = 0; i < CasinoEngine.Score.Length; ++i)
                            {
                                if (CasinoEngine.Score[i] == winningScore)
                                {
                                    if (winners != "")
                                        winners += ", ";
                                    winners += "Player " + (i + 1);
                                }
                            }
                            gameStateText.GetComponent<Text>().text = "The game has ended. Most points: " + winners;
                            break;

                        case CasinoEngine.GameState.endOfTurn:
                            gameStateText.GetComponent<Text>().text = "Player " + CasinoEngine.CurrentPlayer + " has played " + CasinoEngine.UsedCard.value + " of " + CasinoEngine.UsedCard.suit + "s.";
                            break;
                    }

                    previousCasinoState = CasinoEngine.gameState;
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
            casinoSeats = (CasinoEngine.PlayerType[])parameters[0];
            CasinoEngine.seats = casinoSeats;
            CasinoAI.AIType = (CasinoAI.CasinoAIType)parameters[1];
            CreateCasinoDemoScene();
            CasinoEngine.ResetGame();
        }

        /// <summary>
        /// Creates game objects in the scene that are relevant for Casino
        /// </summary>
        private void CreateCasinoDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[4];
            currentPlayerHandHighlights = new SpriteRenderer[4];
            opponentHands = new SpriteRenderer[casinoSeats.Length - 1][];
            opponentNames = new GameObject[casinoSeats.Length - 1];
            tableCards = new SpriteRenderer[13];
            tableCardsHighlights = new SpriteRenderer[13];
            selectedHandCards = new List<Card>();
            selectedTableCards = new List<Card>();
            Sprite highlightSprite = Sprite.Create(selectedCardTexture, new Rect(0, 0, selectedCardTexture.width, selectedCardTexture.height), new Vector2(0, 0));

            //current player hand
            for (int i = 0; i < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("Hand card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                Texture2D cardTexture = cardTextures[cardIndex];
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                sr.enabled = false;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-0.5f * currentPlayerHand.Length + i, -4f, 0);
                card.transform.localScale = new Vector3(100f / cardTexture.width, 384f / 256f * 100f / cardTexture.height, 1);
                currentPlayerHand[i] = sr;

                //highlight sprite renderer
                GameObject highlight = new GameObject("Hand card " + (i + 1) + " highlight");
                highlight.transform.parent = card.transform;
                highlight.transform.localPosition = new Vector3(0, 0, 0);
                highlight.transform.localScale = new Vector3(1, 1, 1);
                SpriteRenderer hsr = highlight.AddComponent<SpriteRenderer>();
                hsr.sprite = highlightSprite;
                hsr.sortingOrder = 1;
                hsr.enabled = false;
                currentPlayerHandHighlights[i] = hsr;

                //Handling the clicks on the card
                card.AddComponent<BoxCollider2D>();
                ClickHandler ch = card.AddComponent<ClickHandler>();
                ch.index = i;
            }

            //current player name
            currentPlayerName = new GameObject("Current player name");
            currentPlayerName.layer = 5; //default UI layer
            currentPlayerName.transform.parent = UICanvas.transform;
            Text nameText = currentPlayerName.AddComponent<Text>();
            nameText.font = defaultFont;
            nameText.fontSize = 18;
            nameText.color = Color.white;
            nameText.rectTransform.sizeDelta = new Vector2(200, 50);
            nameText.rectTransform.localPosition = new Vector3(0, -320f, 0);
            nameText.rectTransform.localScale = new Vector3(1, 1, 1);
            nameText.text = "Player " + 1 + "\n" + CasinoEngine.Score[0];
            nameText.alignment = TextAnchor.MiddleCenter;


            //opponents' hands
            for (int i = 0; i < casinoSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[4];

                for (int k = 0; k < 4; ++k)
                {
                    GameObject card = new GameObject("Opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    Texture2D cardTexture = cardTextures[cardIndex];
                    sr.sprite = cardSprites[cardIndex];
                    sr.sortingOrder = k;
                    sr.enabled = false;
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-(casinoSeats.Length - 3) * 0.1f + k * 0.15f - 6 + i * 3f, 2f, 0);
                    card.transform.localScale = new Vector3(50f / cardTexture.width, 384f / 256f * 50f / cardTexture.height, 1);
                    opponentHands[i][k] = sr;
                }

                //Creating the name texts 
                opponentNames[i] = new GameObject("opponent name " + (i + 1));
                opponentNames[i].layer = 5; //default UI layer
                opponentNames[i].transform.parent = UICanvas.transform;
                nameText = opponentNames[i].AddComponent<Text>();
                nameText.font = defaultFont;
                nameText.fontSize = 18;
                nameText.color = Color.white;
                nameText.rectTransform.sizeDelta = new Vector2(200, 50);
                nameText.rectTransform.localPosition = new Vector3(216 * i - 410f, 110f, 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2) + "\n" + CasinoEngine.Score[i];
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            for (int i = 0; i < tableCards.Length; ++i)
            {
                GameObject card = new GameObject("Table card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                Texture2D cardTexture = cardTextures[cardIndex];
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                sr.enabled = false;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-0.5f * tableCards.Length + i, -1f, 0);
                card.transform.localScale = new Vector3(100f / cardTexture.width, 384f / 256f * 100f / cardTexture.height, 1);
                tableCards[i] = sr;

                //highlight sprite renderer
                GameObject highlight = new GameObject("Table card " + (i + 1) + " highlight");
                highlight.transform.parent = card.transform;
                highlight.transform.localPosition = new Vector3(0, 0, 0);
                highlight.transform.localScale = new Vector3(1, 1, 1);
                SpriteRenderer hsr = highlight.AddComponent<SpriteRenderer>();
                hsr.sprite = highlightSprite;
                hsr.sortingOrder = 1;
                hsr.enabled = false;
                tableCardsHighlights[i] = hsr;

                //Handling the clicks on the tile
                card.AddComponent<BoxCollider2D>();
                ClickHandler ch = card.AddComponent<ClickHandler>();
                ch.index = 10 + i;
            }

            //deck
            GameObject deck = new GameObject("deck");
            SpriteRenderer decksr = deck.AddComponent<SpriteRenderer>();
            Texture2D deckCardTexture = cardTextures[52];
            decksr.sprite = cardSprites[52];
            decksr.sortingOrder = 0;
            decksr.enabled = true;
            deck.transform.parent = board.transform;
            deck.transform.localPosition = new Vector3(4.35f, -1f, 0);
            deck.transform.localScale = new Vector3(100f / deckCardTexture.width, 384f / 256f * 100f / deckCardTexture.height, 1);

            //number on top of deck
            GameObject dt = new GameObject("deck text");
            dt.layer = 5; //default UI layer
            dt.transform.parent = UICanvas.transform;
            Text dtt = dt.AddComponent<Text>();
            dtt.font = defaultFont;
            dtt.fontSize = 24;
            dtt.color = Color.white;
            dtt.rectTransform.sizeDelta = new Vector2(71.6f, 113.3f);
            dtt.rectTransform.localPosition = new Vector3(350f, -100f, 0);
            dtt.rectTransform.localScale = new Vector3(1, 1, 1);
            dtt.text = "52";
            dtt.alignment = TextAnchor.MiddleCenter;
            deckText = dtt;

            //displaying the card that was just used
            GameObject usedCard = new GameObject("used card");
            SpriteRenderer usedCardsr = usedCard.AddComponent<SpriteRenderer>();
            Texture2D usedCardTexture = cardTextures[52];
            usedCardsr.sprite = cardSprites[52];
            usedCardsr.sortingOrder = 0;
            usedCardsr.enabled = false;
            usedCardSR = usedCardsr;
            usedCard.transform.parent = board.transform;
            usedCard.transform.localPosition = new Vector3(2.5f, -1f, 0);
            usedCard.transform.localScale = new Vector3(100f / usedCardTexture.width, 384f / 256f * 100f / usedCardTexture.height, 1);

            //finish turn -button
            GameObject ftb = new GameObject("Finish");
            ftb.layer = 5; //default UI layer
            ftb.transform.parent = UICanvas.transform;
            Image ftbi = ftb.AddComponent<Image>();
            ftbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            ftbi.preserveAspect = true;
            Button ftbb = ftb.AddComponent<Button>();
            ftbb.targetGraphic = ftbi;
            ftbi.rectTransform.sizeDelta = new Vector2(100, 50);
            ftbi.rectTransform.localPosition = new Vector3(350, 150, 0);
            ftbi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject ftbt = new GameObject("Finish turn Text");
            ftbt.layer = 5; //default UI layer
            ftbt.transform.parent = ftb.transform;
            Text ftbtt = ftbt.AddComponent<Text>();
            ftbtt.font = defaultFont;
            ftbtt.fontSize = 15;
            ftbtt.color = Color.white;
            ftbtt.rectTransform.sizeDelta = new Vector2(100, 50);
            ftbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            ftbtt.rectTransform.localScale = new Vector3(1, 1, 1);
            ftbtt.text = "Finish turn";
            ftbtt.alignment = TextAnchor.MiddleCenter;
            ftbb.onClick.AddListener(FinishTurnButtonOnClick);

            previousCards = Utils.IndependentCopyMatrix(CasinoEngine.Hands);
            previousTable = Utils.IndependentCopyList(CasinoEngine.Table);
            previousPlayer = CasinoEngine.CurrentPlayer;
            previousCasinoState = CasinoEngine.GameState.inactive;

            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < casinoSeats.Length; ++i)
            {
                if (casinoSeats[i] == CasinoEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = CasinoEngine.CurrentPlayer;
        }


        /// <summary>
        /// Executed when the finish-turn button is clicked.
        /// </summary>
        void FinishTurnButtonOnClick()
        {

            if (selectedHandCards.Count > 0)
            {
                CasinoEngine.LocalMove(selectedHandCards[0], selectedTableCards);

                //clearing selected hand/table cards
                ClearHighlights();
            }
        }
    }
}
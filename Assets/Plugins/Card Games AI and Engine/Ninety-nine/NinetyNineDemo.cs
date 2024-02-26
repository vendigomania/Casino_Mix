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
    /// A class that handles the NinetyNine demo game
    /// </summary>
    public class NinetyNineDemo : ICardGameDemo
    {
        public NinetyNineEngine.PlayerType[] ninetyNineSeats = new NinetyNineEngine.PlayerType[4] { NinetyNineEngine.PlayerType.local, NinetyNineEngine.PlayerType.AI, NinetyNineEngine.PlayerType.AI, NinetyNineEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (NinetyNineEngine.Hands == null || NinetyNineEngine.Hands.Length == 0 || NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1] == null)
                    return;

                bool NinetyNineHandsChanged = !Utils.EqualCardSets(previousCards, NinetyNineEngine.Hands);

                if (previousPlayer != NinetyNineEngine.CurrentPlayer || NinetyNineHandsChanged)
                {
                    if (NinetyNineEngine.seats[NinetyNineEngine.CurrentPlayer - 1] == NinetyNineEngine.PlayerType.local)
                        mainPlayer = NinetyNineEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < ninetyNineSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= ninetyNineSeats.Length)
                            opponentIndex -= ninetyNineSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (NinetyNineEngine.Hands == null || NinetyNineEngine.Hands.Length <= opponentIndex || NinetyNineEngine.Hands[opponentIndex] == null || NinetyNineEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                opponentHands[i][k].enabled = false;
                            }
                            else
                            {
                                opponentHands[i][k].enabled = true;
                            }
                        }

                    }

                    int padding = 0;
                    //applying any changes in current player's hand
                    for (int i = 0; i + padding < currentPlayerHand.Length; ++i)
                    {
                        if (NinetyNineEngine.Hands[mainPlayer - 1].Count <= i)
                        {
                            currentPlayerHand[i + padding].enabled = false;
                            handCardValueTexts[i + padding].enabled = false;
                            handCardValues[i + padding] = -1;
                            handCards[i + padding] = new Card(Suit.club, 2);
                        }
                        else
                        {
                            int sprite = 52;
                            if (NinetyNineEngine.seats[mainPlayer - 1] == NinetyNineEngine.PlayerType.local)
                            {
                                sprite = NinetyNineEngine.Hands[mainPlayer - 1][i].sortValue() - 1;

                                //secondary option for aces and tens
                                if (NinetyNineEngine.Hands[mainPlayer - 1][i].value == 1 || NinetyNineEngine.Hands[mainPlayer - 1][i].value == 10)
                                {
                                    handCardValues[i + padding] = NinetyNineEngine.Hands[mainPlayer - 1][i].value;
                                    handCardValueTexts[i + padding].text = "" + handCardValues[i + padding];
                                    handCards[i + padding] = NinetyNineEngine.Hands[mainPlayer - 1][i];
                                    currentPlayerHand[i + padding].sprite = cardSprites[sprite];
                                    currentPlayerHand[i + padding].enabled = true;
                                    handCardValueTexts[i + padding].enabled = true;
                                    padding++;
                                }

                                //main card
                                handCardValues[i + padding] = NinetyNineEngine.Hands[mainPlayer - 1][i].value;
                                handCards[i + padding] = NinetyNineEngine.Hands[mainPlayer - 1][i];
                                if (handCardValues[i + padding] == 1)
                                    handCardValues[i + padding] = 11;
                                else if (handCardValues[i + padding] == 4)
                                    handCardValues[i + padding] = 0;
                                else if (handCardValues[i + padding] == 9)
                                    handCardValues[i + padding] = 99;
                                else if (handCardValues[i + padding] == 10)
                                    handCardValues[i + padding] = -10;
                                else if (handCardValues[i + padding] == 11)
                                    handCardValues[i + padding] = 10;
                                else if (handCardValues[i + padding] == 12)
                                    handCardValues[i + padding] = 10;
                                else if (handCardValues[i + padding] == 13)
                                    handCardValues[i + padding] = 0;

                                handCardValueTexts[i + padding].text = "" + handCardValues[i + padding];
                                handCardValueTexts[i + padding].enabled = true;
                            }
                            else
                            {
                                handCardValueTexts[i + padding].enabled = false;
                            }

                            currentPlayerHand[i + padding].sprite = cardSprites[sprite];
                            currentPlayerHand[i + padding].enabled = true;
                        }
                    }

                    previousPlayer = NinetyNineEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(NinetyNineEngine.Hands);
                }

                //updating score for players
                int playerIndexNinetyNine = mainPlayer - 1;
                currentPlayerName.GetComponent<Text>().text = "Player " + (playerIndexNinetyNine + 1) + "\n" + NinetyNineEngine.Tokens[playerIndexNinetyNine];
                for (int i = 0; i < ninetyNineSeats.Length - 1; ++i)
                {
                    playerIndexNinetyNine++;
                    if (playerIndexNinetyNine >= ninetyNineSeats.Length)
                        playerIndexNinetyNine -= ninetyNineSeats.Length;
                    opponentNames[i].GetComponent<Text>().text = "Player " + (playerIndexNinetyNine + 1) + "\n" + NinetyNineEngine.Tokens[playerIndexNinetyNine];
                }

                //displaying the most recently used card if applicable
                if (NinetyNineEngine.gameState == NinetyNineEngine.GameState.endOfTurn && !usedCardSR.enabled)
                {
                    usedCardSR.sprite = cardSprites[NinetyNineEngine.UsedMove.card.sortValue() - 1];
                    usedCardSR.enabled = true;
                }
                else if (NinetyNineEngine.gameState != NinetyNineEngine.GameState.endOfTurn && usedCardSR.enabled)
                {
                    usedCardSR.enabled = false;
                }

                tableValueText.text = NinetyNineEngine.Table + "";

                if (statusChange || previousNinetyNineState != NinetyNineEngine.gameState)
                {
                    switch (NinetyNineEngine.gameState)
                    {
                        case NinetyNineEngine.GameState.cardDealing:
                            gameStateText.GetComponent<Text>().text = "Cards are being dealt to players.";
                            break;

                        case NinetyNineEngine.GameState.normalTurn:
                            gameStateText.GetComponent<Text>().text = "It is player " + NinetyNineEngine.CurrentPlayer + "'s turn.";
                            break;

                        case NinetyNineEngine.GameState.inactive:
                            string winner = "";
                            for (int i = 0; i < NinetyNineEngine.Tokens.Length; ++i)
                            {
                                if (NinetyNineEngine.Tokens[i] > 0)
                                {
                                    winner = "Player " + (i + 1);
                                    break;
                                }
                            }
                            gameStateText.GetComponent<Text>().text = "The game has ended. Winner: " + winner;
                            break;

                        case NinetyNineEngine.GameState.endOfTurn:
                            gameStateText.GetComponent<Text>().text = "Player " + NinetyNineEngine.CurrentPlayer + " has played " + NinetyNineEngine.UsedMove.card.value + " with the value of " + NinetyNineEngine.UsedMove.value + ".";
                            break;
                    }

                    previousNinetyNineState = NinetyNineEngine.gameState;
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
            ninetyNineSeats = (NinetyNineEngine.PlayerType[])parameters[0];
            NinetyNineEngine.seats = ninetyNineSeats;
            NinetyNineAI.AIType = (NinetyNineAI.NinetyNineAIType)parameters[1];
            CreateNinetyNineDemoScene();
            NinetyNineEngine.ResetGame();
        }


        /// <summary>
        /// Creates game objects in the scene that are relevant for NinetyNine
        /// </summary>
        private void CreateNinetyNineDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[6];
            handCardValues = new int[6];
            handCardValueTexts = new Text[6];
            handCards = new Card[6];
            opponentHands = new SpriteRenderer[ninetyNineSeats.Length - 1][];
            opponentNames = new GameObject[ninetyNineSeats.Length - 1];

            int padding = 0;
            //current player hand
            for (int i = 0; i + padding < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("hand card " + (i + padding + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                if (NinetyNineEngine.seats[NinetyNineEngine.CurrentPlayer - 1] == NinetyNineEngine.PlayerType.local)
                {
                    if (i < NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1].Count)
                    {
                        cardIndex = (int)(NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].suit - 1) * 13 + NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].value - 1;

                        //creating the alternative card for aces and tens
                        if (NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].value == 1 || NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].value == 10)
                        {
                            GameObject card2 = new GameObject("hand card " + (i + padding + 2));
                            SpriteRenderer sr2 = card2.AddComponent<SpriteRenderer>();
                            Texture2D cardTexture2 = cardTextures[cardIndex];
                            sr2.sprite = cardSprites[cardIndex];
                            sr2.sortingOrder = 0;
                            card2.transform.parent = board.transform;
                            card2.transform.localPosition = new Vector3(-0.5f * currentPlayerHand.Length + i + padding, -4f, 0);
                            card2.transform.localScale = new Vector3(100f / cardTexture2.width, 384f / 256f * 100f / cardTexture2.height, 1);
                            currentPlayerHand[i + padding] = sr2;
                            //Handling the clicks on the tile
                            card2.AddComponent<BoxCollider2D>();
                            ClickHandler ch2 = card2.AddComponent<ClickHandler>();
                            ch2.index = i + padding;

                            //card value text
                            handCardValues[i + padding] = NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].value;
                            GameObject cardValueText2 = new GameObject("Card value text");
                            cardValueText2.layer = 5; //default UI layer
                            cardValueText2.transform.parent = UICanvas.transform;
                            Text cvText2 = cardValueText2.AddComponent<Text>();
                            cvText2.font = defaultFont;
                            cvText2.fontSize = 30;
                            cvText2.color = Color.white;
                            cvText2.rectTransform.sizeDelta = new Vector2(100, 50);
                            cvText2.rectTransform.localPosition = new Vector3(35 + -73 * currentPlayerHand.Length / 2f + 73 * (i + padding), -150f, 0);
                            cvText2.rectTransform.localScale = new Vector3(1, 1, 1);
                            cvText2.text = "" + handCardValues[i + padding];
                            cvText2.alignment = TextAnchor.MiddleCenter;
                            handCardValueTexts[i + padding] = cvText2;

                            handCards[i + padding] = NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i];

                            padding++;
                        }
                    }
                    else
                        sr.enabled = false;
                }
                Texture2D cardTexture = cardTextures[cardIndex];
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-0.5f * currentPlayerHand.Length + i + padding, -4f, 0);
                card.transform.localScale = new Vector3(100f / cardTexture.width, 384f / 256f * 100f / cardTexture.height, 1);
                currentPlayerHand[i + padding] = sr;
                //Handling the clicks on the tile
                card.AddComponent<BoxCollider2D>();
                ClickHandler ch = card.AddComponent<ClickHandler>();
                ch.index = i + padding;

                if (i < NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1].Count)
                {
                    //card value text
                    handCardValues[i + padding] = NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i].value;
                    if (handCardValues[i + padding] == 1)
                        handCardValues[i + padding] = 11;
                    else if (handCardValues[i + padding] == 4)
                        handCardValues[i + padding] = 0;
                    else if (handCardValues[i + padding] == 9)
                        handCardValues[i + padding] = 99;
                    else if (handCardValues[i + padding] == 10)
                        handCardValues[i + padding] = -10;
                    else if (handCardValues[i + padding] == 11)
                        handCardValues[i + padding] = 10;
                    else if (handCardValues[i + padding] == 12)
                        handCardValues[i + padding] = 10;
                    else if (handCardValues[i + padding] == 13)
                        handCardValues[i + padding] = 0;
                }
                else
                    handCardValues[i + padding] = 0;

                GameObject cardValueText = new GameObject("Card value text");
                cardValueText.layer = 5; //default UI layer
                cardValueText.transform.parent = UICanvas.transform;
                Text cvText = cardValueText.AddComponent<Text>();
                cvText.font = defaultFont;
                cvText.fontSize = 30;
                cvText.color = Color.white;
                cvText.rectTransform.sizeDelta = new Vector2(100, 50);
                cvText.rectTransform.localPosition = new Vector3(35 + -73 * currentPlayerHand.Length / 2f + 73 * (i + padding), -150f, 0);
                cvText.rectTransform.localScale = new Vector3(1, 1, 1);
                cvText.text = "" + handCardValues[i + padding];
                cvText.alignment = TextAnchor.MiddleCenter;
                handCardValueTexts[i + padding] = cvText;
                if (i >= NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1].Count || NinetyNineEngine.seats[NinetyNineEngine.CurrentPlayer - 1] != NinetyNineEngine.PlayerType.local)
                {
                    handCardValueTexts[i + padding].enabled = false;
                    handCards[i + padding] = new Card(Suit.club, 2);
                    handCardValues[i + padding] = -1;
                }
                else
                    handCards[i + padding] = NinetyNineEngine.Hands[NinetyNineEngine.CurrentPlayer - 1][i];
            }

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
            nameText.text = "Player 1";
            nameText.alignment = TextAnchor.MiddleCenter;


            //opponents' hands
            for (int i = 0; i < ninetyNineSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[3];

                for (int k = 0; k < opponentHands[i].Length; ++k)
                {
                    GameObject card = new GameObject("opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    Texture2D cardTexture = cardTextures[cardIndex];
                    sr.sprite = cardSprites[cardIndex];
                    sr.sortingOrder = k;
                    if (NinetyNineEngine.Hands[i + 1].Count <= k)
                        sr.enabled = false;
                    else
                        sr.enabled = true;
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-0.15f * (opponentHands[i].Length) - (ninetyNineSeats.Length - 3) * 0.1f + k * 0.15f - 6.4f + i % 2 * 3f, 3.1f - (i / 2) * 1.6f, 0);
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
                nameText.rectTransform.localPosition = new Vector3(216 * (i % 2) - 490f, 200f - 115 * (i / 2), 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2) + ": " + NinetyNineEngine.Tokens[i + 1];
                nameText.alignment = TextAnchor.MiddleCenter;
            }


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

            //table value
            GameObject dt = new GameObject("table vlaue text");
            dt.layer = 5; //default UI layer
            dt.transform.parent = UICanvas.transform;
            Text dtt = dt.AddComponent<Text>();
            dtt.font = defaultFont;
            dtt.fontSize = 64;
            dtt.color = Color.black;
            dtt.rectTransform.sizeDelta = new Vector2(150f, 100f);
            dtt.rectTransform.localPosition = new Vector3(0f, 0f, 0);
            dtt.rectTransform.localScale = new Vector3(1, 1, 1);
            dtt.text = "0";
            dtt.alignment = TextAnchor.MiddleCenter;
            tableValueText = dtt;

            previousCards = Utils.IndependentCopyMatrix(NinetyNineEngine.Hands);
            previousPlayer = NinetyNineEngine.CurrentPlayer;
            previousNinetyNineState = NinetyNineEngine.GameState.inactive;

            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < ninetyNineSeats.Length; ++i)
            {
                if (ninetyNineSeats[i] == NinetyNineEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = NinetyNineEngine.CurrentPlayer;
        }
    }
}
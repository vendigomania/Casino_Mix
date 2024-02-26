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
    /// A class that handles the GOPS demo game
    /// </summary>
    public class GOPSDemo : ICardGameDemo
    {
        public GOPSEngine.PlayerType[] GOPSSeats = new GOPSEngine.PlayerType[4] { GOPSEngine.PlayerType.local, GOPSEngine.PlayerType.AI, GOPSEngine.PlayerType.AI, GOPSEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (GOPSEngine.Hands == null || GOPSEngine.Hands.Length == 0 || GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1] == null)
                    return;

                bool GOPSHandsChanged = !Utils.EqualCardSets(previousCards, GOPSEngine.Hands);

                if (GOPSHandsChanged)
                {
                    if (GOPSEngine.seats[GOPSEngine.CurrentPlayer - 1] == GOPSEngine.PlayerType.local)
                        mainPlayer = GOPSEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < GOPSSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= GOPSSeats.Length)
                            opponentIndex -= GOPSSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (GOPSEngine.Hands == null || GOPSEngine.Hands.Length <= opponentIndex || GOPSEngine.Hands[opponentIndex] == null || GOPSEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                opponentHands[i][k].enabled = false;
                            }
                            else
                            {
                                opponentHands[i][k].enabled = true;
                            }
                        }

                        opponentNames[i].GetComponent<Text>().text = "Player " + (opponentIndex + 1) + ": " + GOPSEngine.Points[opponentIndex].ToString("0.00");
                    }


                    //applying any changes in current player's hand
                    for (int i = 0; i < currentPlayerHand.Length; ++i)
                    {
                        if (GOPSEngine.Hands[mainPlayer - 1].Count < i + 1)
                        {
                            currentPlayerHand[i].enabled = false;
                        }
                        else
                        {
                            int sprite = 52;
                            if (GOPSEngine.seats[mainPlayer - 1] == GOPSEngine.PlayerType.local)
                                sprite = GOPSEngine.Hands[mainPlayer - 1][i].sortValue() - 1;
                            currentPlayerHand[i].sprite = cardSprites[sprite];
                            currentPlayerHand[i].enabled = true;
                        }
                    }

                    currentPlayerName.GetComponent<Text>().text = "Player " + mainPlayer + ": " + GOPSEngine.Points[mainPlayer - 1].ToString("0.00");


                    previousPlayer = GOPSEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(GOPSEngine.Hands);
                }

                if (!Utils.EqualCardLists(previousTable, GOPSEngine.Table))
                {
                    statusChange = true;

                    if (tableCards.Length > 0 && GOPSEngine.Table.Count > 0)
                    {
                        tableCards[0].sprite = cardSprites[GOPSEngine.Table[0].sortValue() - 1];
                        tableCards[0].enabled = true;
                    }
                    else
                    {
                        tableCards[0].enabled = false;
                    }

                    previousTable = Utils.IndependentCopyList(GOPSEngine.Table);
                }

                //displaying the bids
                if (GOPSEngine.gameState == GOPSEngine.GameState.endOfTurn && usedCardSRs.Length > 0 && !usedCardSRs[0].enabled)
                {
                    for (int i = 0; i < usedCardSRs.Length; ++i)
                    {
                        usedCardSRs[i].sprite = cardSprites[GOPSEngine.Bids[i].sortValue() - 1];
                        usedCardSRs[i].enabled = true;
                    }
                }
                else if (GOPSEngine.gameState != GOPSEngine.GameState.endOfTurn && usedCardSRs.Length > 0 && usedCardSRs[0].enabled)
                {
                    for (int i = 0; i < usedCardSRs.Length; ++i)
                    {
                        usedCardSRs[i].enabled = false;
                    }
                }

                if (statusChange || previousGOPSState != GOPSEngine.gameState)
                {
                    switch (GOPSEngine.gameState)
                    {
                        case GOPSEngine.GameState.normalTurn:
                            gameStateText.GetComponent<Text>().text = "It is player " + GOPSEngine.CurrentPlayer + "'s turn to select a bid.";
                            break;

                        case GOPSEngine.GameState.endOfTurn:

                            int highestBid = 0;
                            string highestBidders = "";
                            for (int i = 0; i < GOPSEngine.Bids.Length; ++i)
                            {
                                if (GOPSEngine.Bids[i].value > highestBid)
                                {
                                    highestBid = GOPSEngine.Bids[i].value;
                                    highestBidders = "Player " + (i + 1);
                                }
                                else if (GOPSEngine.Bids[i].value == highestBid)
                                {
                                    highestBidders += ", Player " + (i + 1);
                                }
                            }

                            gameStateText.GetComponent<Text>().text = "Bid was won with " + highestBid + " by " + highestBidders + ".";
                            break;

                        case GOPSEngine.GameState.inactive:
                            string winners = "";
                            float winningPoints = 0;
                            for (int i = 0; i < GOPSEngine.Points.Length; ++i)
                            {
                                if (GOPSEngine.Points[i] > winningPoints)
                                {
                                    winningPoints = GOPSEngine.Points[i];
                                    winners = "Player " + (i + 1);
                                }
                                else if (GOPSEngine.Points[i] == winningPoints)
                                {
                                    winners += ", Player " + (i + 1);
                                }
                            }
                            gameStateText.GetComponent<Text>().text = "The game has ended. Winner: " + winners;
                            break;
                    }

                    previousGOPSState = GOPSEngine.gameState;
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
            GOPSSeats = (GOPSEngine.PlayerType[])parameters[0];
            GOPSEngine.seats = GOPSSeats;
            GOPSAI.AIType = (GOPSAI.GOPSAIType)parameters[1];
            CreateGOPSDemoScene();
            GOPSEngine.ResetGame();
        }

        /// <summary>
        /// Creates game objects in the scene that are relevant for GOPS
        /// </summary>
        private void CreateGOPSDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1].Count];
            opponentHands = new SpriteRenderer[GOPSSeats.Length - 1][];
            opponentNames = new GameObject[GOPSSeats.Length - 1];
            tableCards = new SpriteRenderer[1];
            usedCardSRs = new SpriteRenderer[GOPSSeats.Length];


            //current player hand
            for (int i = 0; i < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("hand card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                if (GOPSEngine.seats[GOPSEngine.CurrentPlayer - 1] == GOPSEngine.PlayerType.local)
                {
                    if (i < GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1].Count)
                    {
                        cardIndex = (int)(GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1][i].suit - 1) * 13 + GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1][i].value - 1;
                    }
                    else
                        sr.enabled = false;
                }
                Texture2D cardTexture = cardTextures[cardIndex];
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-0.5f * currentPlayerHand.Length + i, -4f, 0);
                card.transform.localScale = new Vector3(100f / cardTexture.width, 384f / 256f * 100f / cardTexture.height, 1);
                currentPlayerHand[i] = sr;
                //Handling the clicks on the tile
                card.AddComponent<BoxCollider2D>();
                ClickHandler ch = card.AddComponent<ClickHandler>();
                ch.index = i;
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
            nameText.text = "Player " + GOPSEngine.CurrentPlayer + ": " + GOPSEngine.Points[GOPSEngine.CurrentPlayer - 1].ToString("0.00");
            nameText.alignment = TextAnchor.MiddleCenter;

            //opponents' hands
            for (int i = 0; i < GOPSSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[13];

                for (int k = 0; k < opponentHands[i].Length; ++k)
                {
                    GameObject card = new GameObject("opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    Texture2D cardTexture = cardTextures[cardIndex];
                    sr.sprite = cardSprites[cardIndex];
                    sr.sortingOrder = k;
                    if (GOPSEngine.Hands[i + 1].Count <= k)
                        sr.enabled = false;
                    else
                        sr.enabled = true;
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-0.15f * opponentHands[i].Length - (GOPSSeats.Length - 3) * 0.1f + k * 0.15f - 5.3f + i % 2 * 3f, 3f - (i / 2) * 1.6f, 0);
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
                nameText.rectTransform.localPosition = new Vector3(216 * (i % 2) - 460f, 200f - 115 * (i / 2), 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2) + ": " + GOPSEngine.Points[i + 1].ToString("0.00");
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            GameObject tableCard = new GameObject("Table card");
            SpriteRenderer srTable = tableCard.AddComponent<SpriteRenderer>();
            int tableCardIndex = GOPSEngine.Table[0].sortValue() - 1;
            Texture2D tableCardTexture = cardTextures[tableCardIndex];
            srTable.sprite = cardSprites[tableCardIndex];
            srTable.sortingOrder = 0;
            srTable.enabled = true;
            tableCard.transform.parent = board.transform;
            tableCard.transform.localPosition = new Vector3(-0.5f, -1f, 0);
            tableCard.transform.localScale = new Vector3(100f / tableCardTexture.width, 384f / 256f * 100f / tableCardTexture.height, 1);
            tableCards[0] = srTable;


            //displaying the bids
            for (int i = 0; i < GOPSSeats.Length; ++i)
            {
                GameObject usedCard = new GameObject("used card");
                SpriteRenderer usedCardsr = usedCard.AddComponent<SpriteRenderer>();
                Texture2D usedCardTexture = cardTextures[52];
                usedCardsr.sprite = cardSprites[52];
                usedCardsr.sortingOrder = i;
                usedCardsr.enabled = false;
                usedCardSRs[i] = usedCardsr;
                usedCard.transform.parent = board.transform;
                usedCard.transform.localPosition = new Vector3(3.5f - 0.8f * (GOPSSeats.Length / 2 - i), -1f, 0);
                usedCard.transform.localScale = new Vector3(100f / usedCardTexture.width, 384f / 256f * 100f / usedCardTexture.height, 1);
            }


            previousCards = Utils.IndependentCopyMatrix(GOPSEngine.Hands);
            previousPlayer = GOPSEngine.CurrentPlayer;
            previousTable = Utils.IndependentCopyList(GOPSEngine.Table);
            previousGOPSState = GOPSEngine.GameState.inactive;

            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < GOPSSeats.Length; ++i)
            {
                if (GOPSSeats[i] == GOPSEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = GOPSEngine.CurrentPlayer;
        }
    }
}
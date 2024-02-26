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
    /// A class that handles the Sevens demo game
    /// </summary>
    public class SevensDemo : ICardGameDemo
    {
        public SevensEngine.PlayerType[] sevensSeats = new SevensEngine.PlayerType[4] { SevensEngine.PlayerType.local, SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (SevensEngine.Hands == null || SevensEngine.Hands.Length == 0 || SevensEngine.Hands[SevensEngine.CurrentPlayer - 1] == null)
                    return;

                bool sevensHandsChanged = !Utils.EqualCardSets(previousCards, SevensEngine.Hands);

                if (sevensHandsChanged)
                {
                    if (SevensEngine.seats[SevensEngine.CurrentPlayer - 1] == SevensEngine.PlayerType.local)
                        mainPlayer = SevensEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < sevensSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= sevensSeats.Length)
                            opponentIndex -= sevensSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (SevensEngine.Hands == null || SevensEngine.Hands.Length <= opponentIndex || SevensEngine.Hands[opponentIndex] == null || SevensEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                opponentHands[i][k].enabled = false;
                            }
                            else
                            {
                                opponentHands[i][k].enabled = true;
                            }
                        }

                        opponentNames[i].GetComponent<Text>().text = "Player " + (opponentIndex + 1);
                    }


                    //applying any changes in current player's hand
                    for (int i = 0; i < currentPlayerHand.Length; ++i)
                    {
                        if (SevensEngine.Hands[mainPlayer - 1].Count < i + 1)
                        {
                            currentPlayerHand[i].enabled = false;
                        }
                        else
                        {
                            int sprite = 52;
                            if (SevensEngine.seats[mainPlayer - 1] == SevensEngine.PlayerType.local)
                                sprite = SevensEngine.Hands[mainPlayer - 1][i].sortValue() - 1;
                            currentPlayerHand[i].sprite = cardSprites[sprite];
                            currentPlayerHand[i].enabled = true;
                        }
                    }

                    currentPlayerName.GetComponent<Text>().text = "Player " + mainPlayer;


                    previousPlayer = SevensEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(SevensEngine.Hands);
                }


                if (previousTable.Count != SevensEngine.Table.Count)
                {
                    statusChange = true;

                    //applying any changes in table cards
                    //display lowest, highest and 7 of each suit
                    for (int i = 0; i < tableCards.Length; ++i)
                    {
                        int row = (i / 4) - 1;
                        int suit = (i % 4) + 1;
                        if (row == -1)
                        {
                            int cardIndex = 52;
                            for (int k = 0; k < SevensEngine.Table.Count; ++k)
                            {
                                if ((int)SevensEngine.Table[k].suit == suit && SevensEngine.Table[k].sortValue() - 1 < cardIndex && SevensEngine.Table[k].value < 7)
                                    cardIndex = SevensEngine.Table[k].sortValue() - 1;
                            }

                            if (cardIndex < 52)
                            {
                                tableCards[i].sprite = cardSprites[cardIndex];
                                tableCards[i].enabled = true;
                            }
                            else
                                tableCards[i].enabled = false;
                        }
                        else if (row == 0)
                        {
                            int cardIndex = 52;
                            for (int k = 0; k < SevensEngine.Table.Count; ++k)
                            {
                                if ((int)SevensEngine.Table[k].suit == suit && SevensEngine.Table[k].value == 7)
                                {
                                    cardIndex = SevensEngine.Table[k].sortValue() - 1;
                                    break;
                                }
                            }

                            if (cardIndex < 52)
                            {
                                tableCards[i].sprite = cardSprites[cardIndex];
                                tableCards[i].enabled = true;
                            }
                            else
                                tableCards[i].enabled = false;
                        }
                        else if (row == 1)
                        {
                            int cardIndex = -1;
                            for (int k = 0; k < SevensEngine.Table.Count; ++k)
                            {
                                if ((int)SevensEngine.Table[k].suit == suit && SevensEngine.Table[k].sortValue() - 1 > cardIndex && SevensEngine.Table[k].value > 7)
                                {
                                    cardIndex = SevensEngine.Table[k].sortValue() - 1;
                                }
                            }

                            if (cardIndex >= 0)
                            {
                                tableCards[i].sprite = cardSprites[cardIndex];
                                tableCards[i].enabled = true;
                            }
                            else
                                tableCards[i].enabled = false;
                        }
                    }

                    previousTable = Utils.IndependentCopyList(SevensEngine.Table);
                }

                if (statusChange || previousSevensState != SevensEngine.gameState)
                {
                    switch (SevensEngine.gameState)
                    {
                        case SevensEngine.GameState.normalTurn:
                            gameStateText.GetComponent<Text>().text = "It is player " + SevensEngine.CurrentPlayer + "'s turn.";
                            break;

                        case SevensEngine.GameState.passedTurn:
                            gameStateText.GetComponent<Text>().text = "Player " + SevensEngine.CurrentPlayer + " passed.";
                            break;

                        case SevensEngine.GameState.inactive:
                            int winner = 1;
                            for (int i = 0; i < SevensEngine.Hands.Length; ++i)
                            {
                                if (SevensEngine.Hands[i].Count == 0)
                                {
                                    winner = i + 1;
                                    break;
                                }
                            }
                            gameStateText.GetComponent<Text>().text = "The game has ended. Winner: Player " + winner;
                            break;
                    }

                    previousSevensState = SevensEngine.gameState;
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
            sevensSeats = (SevensEngine.PlayerType[])parameters[0];
            SevensEngine.seats = sevensSeats;
            SevensAI.AIType = (SevensAI.SevensAIType)parameters[1];
            CreateSevensDemoScene();
            SevensEngine.ResetGame();
        }


        /// <summary>
        /// Creates game objects in the scene that are relevant for Sevens
        /// </summary>
        private void CreateSevensDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.parent = DemoScript.GlobalBtn.transform.parent.parent;
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[52 / sevensSeats.Length + 1];
            opponentHands = new SpriteRenderer[sevensSeats.Length - 1][];
            opponentNames = new GameObject[sevensSeats.Length - 1];
            tableCards = new SpriteRenderer[12];

            //current player hand
            for (int i = 0; i < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("hand card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                if (SevensEngine.seats[SevensEngine.CurrentPlayer - 1] == SevensEngine.PlayerType.local)
                {
                    if (i < SevensEngine.Hands[SevensEngine.CurrentPlayer - 1].Count)
                    {
                        cardIndex = (int)(SevensEngine.Hands[SevensEngine.CurrentPlayer - 1][i].suit - 1) * 13 + SevensEngine.Hands[SevensEngine.CurrentPlayer - 1][i].value - 1;
                    }
                    else
                        sr.enabled = false;
                }
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = i + 1;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-0.5f * currentPlayerHand.Length + i, -3.8f, 0);
                card.transform.localScale = new Vector3(0.3f, 0.3f, 1);
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
            nameText.fontSize = 64;
            nameText.color = Color.white;
            nameText.rectTransform.sizeDelta = new Vector2(200, 77);
            nameText.rectTransform.localPosition = new Vector3(-400, -260f, 0);
            nameText.rectTransform.localScale = new Vector3(1, 1, 1);
            nameText.text = "Player 1";
            nameText.alignment = TextAnchor.MiddleCenter;


            //opponents' hands
            for (int i = 0; i < sevensSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[52 / sevensSeats.Length + 1];

                for (int k = 0; k < opponentHands[i].Length; ++k)
                {
                    GameObject card = new GameObject("opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    sr.sprite = cardSprites[cardIndex];
                    sr.sortingOrder = k;
                    if (SevensEngine.Hands[i].Count >= k) sr.enabled = false;
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-0.15f * opponentHands[i].Length - (sevensSeats.Length - 3) * 0.1f + k * 0.15f - 4.2f + i % 2 * 2.4f, 2f - (i / 2) * 1.6f, 0);
                    card.transform.localScale = new Vector3(0.125f, 0.125f, 1);
                    opponentHands[i][k] = sr;
                }

                //Creating the name texts 
                opponentNames[i] = new GameObject("opponent name " + (i + 1));
                opponentNames[i].layer = 5; //default UI layer
                opponentNames[i].transform.parent = UICanvas.transform;
                nameText = opponentNames[i].AddComponent<Text>();
                nameText.font = defaultFont;
                nameText.fontSize = 64;
                nameText.color = Color.white;
                nameText.rectTransform.sizeDelta = new Vector2(200, 77);
                nameText.rectTransform.localPosition = new Vector3(260 * (i % 2) - 600f, 140f - 170 * (i / 2), 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2);
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            // 8  9  10 11
            // 4  5  6  7
            // 0  1  2  3
            for (int i = 0; i < tableCards.Length; ++i)
            {
                GameObject card = new GameObject("Table card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                sr.enabled = false;
                card.transform.parent = board.transform;
                int row = i / 4 - 1;
                card.transform.localPosition = new Vector3(-1f + (i % 4) * 1.2f, -0.9f + (i / 4 - 1) * 2f + 1f, 0);
                card.transform.localScale = new Vector3(0.27f, 0.27f, 1);
                tableCards[i] = sr;
            }

            //Pass turn -button
            DemoScript.GlobalBtn.onClick.AddListener(PassButtonOnClick);

            previousCards = Utils.IndependentCopyMatrix(SevensEngine.Hands);
            previousTable = Utils.IndependentCopyList(SevensEngine.Table);
            previousPlayer = SevensEngine.CurrentPlayer;
            previousSevensState = SevensEngine.GameState.inactive;

            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < sevensSeats.Length; ++i)
            {
                if (sevensSeats[i] == SevensEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = SevensEngine.CurrentPlayer;
        }

        /// <summary>
        /// Executed when the action button is clicked. This button can be "finish turn", "pass" or similar. Notifies the relevant game engine of the click.
        /// </summary>
        void PassButtonOnClick()
        {
            //passing the turn. actually "playing" a 7 of hearts, but that is obviously already played
            SevensEngine.LocalMove(new Card(Suit.heart, 7));
        }
    }
}
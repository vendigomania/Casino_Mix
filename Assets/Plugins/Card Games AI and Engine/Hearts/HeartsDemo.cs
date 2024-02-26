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
    /// A class that handles the Hearts demo game
    /// </summary>
    public class HeartsDemo : ICardGameDemo
    {
        public HeartsEngine.PlayerType[] heartsSeats = new HeartsEngine.PlayerType[4] { HeartsEngine.PlayerType.local, HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;

                if (HeartsEngine.Hands == null || HeartsEngine.Hands.Length == 0 || HeartsEngine.Hands[HeartsEngine.CurrentPlayer - 1] == null)
                    return;


                if (previousPlayer != HeartsEngine.CurrentPlayer || !Utils.EqualCardSets(previousCards, HeartsEngine.Hands))
                {
                    if (HeartsEngine.seats[HeartsEngine.CurrentPlayer - 1] == HeartsEngine.PlayerType.local)
                        mainPlayer = HeartsEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < heartsSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= heartsSeats.Length)
                            opponentIndex -= heartsSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (HeartsEngine.Hands == null || HeartsEngine.Hands.Length <= opponentIndex || HeartsEngine.Hands[opponentIndex] == null || HeartsEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                opponentHands[i][k].enabled = false;
                            }
                            else
                            {
                                opponentHands[i][k].enabled = true;
                            }
                        }

                        opponentNames[i].GetComponent<Text>().text = "Player " + (opponentIndex + 1) + "\n" + HeartsEngine.Score[opponentIndex];
                    }


                    //applying any changes in current player's hand
                    for (int i = 0; i < currentPlayerHand.Length; ++i)
                    {
                        if (HeartsEngine.Hands[mainPlayer - 1].Count < i + 1)
                        {
                            currentPlayerHand[i].enabled = false;
                        }
                        else
                        {
                            int sprite = 52;
                            if (HeartsEngine.seats[mainPlayer - 1] == HeartsEngine.PlayerType.local)
                                sprite = HeartsEngine.Hands[mainPlayer - 1][i].sortValue() - 1;
                            currentPlayerHand[i].sprite = cardSprites[sprite];
                            currentPlayerHand[i].enabled = true;
                        }
                    }

                    currentPlayerName.GetComponent<Text>().text = "Player " + mainPlayer + "\n" + HeartsEngine.Score[mainPlayer - 1];


                    previousPlayer = HeartsEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(HeartsEngine.Hands);
                }


                if (previousTable.Count != HeartsEngine.Table.Count)
                {
                    statusChange = true;

                    //applying any changes in table cards
                    for (int i = 0; i < tableCards.Length; ++i)
                    {
                        if (HeartsEngine.Table.Count < i + 1)
                        {
                            tableCards[i].enabled = false;
                        }
                        else
                        {
                            tableCards[i].sprite = cardSprites[HeartsEngine.Table[i].sortValue() - 1];
                            tableCards[i].enabled = true;
                        }
                    }

                    previousTable = Utils.IndependentCopyList(HeartsEngine.Table);
                }

                if (statusChange || previousHeartsState != HeartsEngine.gameState)
                {
                    switch (HeartsEngine.gameState)
                    {
                        case HeartsEngine.GameState.cardPassing:
                            int passedPlayer = HeartsEngine.CurrentPlayer + HeartsEngine.PassDifference;
                            if (passedPlayer > HeartsEngine.seats.Length)
                                passedPlayer -= HeartsEngine.seats.Length;
                            gameStateText.GetComponent<Text>().text = "Player " + HeartsEngine.CurrentPlayer + "'s turn to pass 3 cards to player " + passedPlayer + ".";
                            break;

                        case HeartsEngine.GameState.normalTurn:
                            gameStateText.GetComponent<Text>().text = "It is player " + HeartsEngine.CurrentPlayer + "'s turn.";
                            break;

                        case HeartsEngine.GameState.inactive:
                            gameStateText.GetComponent<Text>().text = "The game has ended.";
                            break;
                    }

                    previousHeartsState = HeartsEngine.gameState;
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
            heartsSeats = (HeartsEngine.PlayerType[])parameters[0];
            HeartsEngine.seats = heartsSeats;
            HeartsAI.AIType = (HeartsAI.HeartsAIType)parameters[1];
            CreateHeartsDemoScene();
            HeartsEngine.ResetGame();
        }

        /// <summary>
        /// Creates game objects in the scene that are relevant for Hearts
        /// </summary>
        private void CreateHeartsDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[52 / heartsSeats.Length];
            opponentHands = new SpriteRenderer[heartsSeats.Length - 1][];
            opponentNames = new GameObject[heartsSeats.Length - 1];
            tableCards = new SpriteRenderer[heartsSeats.Length];

            //current player hand
            for (int i = 0; i < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("hand card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                if (HeartsEngine.seats[HeartsEngine.CurrentPlayer - 1] == HeartsEngine.PlayerType.local)
                    cardIndex = (int)(HeartsEngine.Hands[HeartsEngine.CurrentPlayer - 1][i].suit - 1) * 13 + HeartsEngine.Hands[HeartsEngine.CurrentPlayer - 1][i].value - 1;
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
            nameText.text = "Player " + 1 + "\n" + HeartsEngine.Score[0];
            nameText.alignment = TextAnchor.MiddleCenter;


            //opponents' hands
            for (int i = 0; i < heartsSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[52 / heartsSeats.Length];

                for (int k = 0; k < HeartsEngine.Hands[i].Count; ++k)
                {
                    GameObject card = new GameObject("opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    Texture2D cardTexture = cardTextures[cardIndex];
                    sr.sprite = cardSprites[cardIndex];
                    sr.sortingOrder = k;
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-0.15f * (52 / heartsSeats.Length) - (heartsSeats.Length - 3) * 0.1f + k * 0.15f - 6 + i * 3f, 2f, 0);
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
                nameText.rectTransform.localPosition = new Vector3(216 * i - 490f, 110f, 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2) + "\n" + HeartsEngine.Score[i];
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            for (int i = 0; i < tableCards.Length; ++i)
            {
                //deifining the table cards
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
            }

            previousCards = Utils.IndependentCopyMatrix(HeartsEngine.Hands);
            previousTable = Utils.IndependentCopyList(HeartsEngine.Table);
            previousPlayer = HeartsEngine.CurrentPlayer;
            previousHeartsState = HeartsEngine.GameState.inactive;

            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < heartsSeats.Length; ++i)
            {
                if (heartsSeats[i] == HeartsEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = HeartsEngine.CurrentPlayer;
        }
    }
}
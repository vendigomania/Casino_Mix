using UnityEngine;
using UnityEngine.UI;
using static CGAIE.DemoBase;

/// <summary>
/// The namespace for all Card Game AI and Engine Pack -related elements.
/// </summary>
namespace CGAIE
{

    /// <summary>
    /// A class that handles the Blackjack demo game
    /// </summary>
    public class BlackjackDemo : ICardGameDemo
    {
        private BlackjackEngine.PlayerType[] BlackjackSeats = new BlackjackEngine.PlayerType[4] { BlackjackEngine.PlayerType.local, BlackjackEngine.PlayerType.AI, BlackjackEngine.PlayerType.AI, BlackjackEngine.PlayerType.AI };

        /// <summary>
        /// Called once per frame
        /// </summary>
        void ICardGameDemo.Update()
        {
            try
            {
                bool statusChange = false;
                if (BlackjackEngine.Hands == null || BlackjackEngine.Hands.Length == 0 || BlackjackEngine.Hands[BlackjackEngine.CurrentPlayer - 1] == null)
                    return;

                bool BlackjackHandsChanged = !Utils.EqualCardSets(previousCards, BlackjackEngine.Hands);

                bool handStateChanged = false;
                for (int i = 0; i < BlackjackEngine.HandStates.Length; ++i)
                {
                    if (previousHandStates[i] != BlackjackEngine.HandStates[i])
                    {
                        handStateChanged = true;
                        break;
                    }
                }
                for (int i = 0; i < BlackjackEngine.SecondHandStates.Length; ++i)
                {
                    if (previousSecondHandStates[i] != BlackjackEngine.SecondHandStates[i])
                    {
                        handStateChanged = true;
                        break;
                    }
                }


                if (BlackjackHandsChanged || handStateChanged || previousPlayer != BlackjackEngine.CurrentPlayer)
                {
                    mainPlayer = BlackjackEngine.CurrentPlayer;
                    statusChange = true;

                    //opponents' card
                    int opponentIndex = mainPlayer - 1;
                    for (int i = 0; i < BlackjackSeats.Length - 1; ++i)
                    {
                        opponentIndex++;
                        if (opponentIndex >= BlackjackSeats.Length)
                            opponentIndex -= BlackjackSeats.Length;

                        for (int k = 0; k < opponentHands[i].Length; ++k)
                        {
                            if (BlackjackEngine.Hands == null || BlackjackEngine.Hands.Length <= opponentIndex || BlackjackEngine.Hands[opponentIndex] == null || BlackjackEngine.Hands[opponentIndex].Count < k + 1)
                            {
                                //secondary hand after splitting
                                int secondHandIndex = k - BlackjackEngine.Hands[opponentIndex].Count - 1;
                                if (BlackjackEngine.SecondHands[opponentIndex].Count > secondHandIndex && k > BlackjackEngine.Hands[opponentIndex].Count)
                                {
                                    opponentHands[i][k].sprite = cardSprites[BlackjackEngine.SecondHands[opponentIndex][secondHandIndex].sortValue() - 1];
                                    opponentHands[i][k].enabled = true;
                                }
                                else
                                {
                                    opponentHands[i][k].enabled = false;
                                }
                            }
                            else
                            {
                                opponentHands[i][k].sprite = cardSprites[BlackjackEngine.Hands[opponentIndex][k].sortValue() - 1];
                                opponentHands[i][k].enabled = true;
                            }
                        }

                        opponentNames[i].GetComponent<Text>().text = "Player " + (opponentIndex + 1) + "\nchips: " + BlackjackEngine.Chips[opponentIndex];
                    }


                    //applying any changes in current player's hand
                    for (int i = 0; i < currentPlayerHand.Length; ++i)
                    {
                        if (BlackjackEngine.Hands[mainPlayer - 1].Count < i + 1)
                        {
                            //secondary hand after splitting
                            int secondHandIndex = i - BlackjackEngine.Hands[mainPlayer - 1].Count - 1;
                            if (BlackjackEngine.SecondHands[mainPlayer - 1].Count > secondHandIndex && i > BlackjackEngine.Hands[mainPlayer - 1].Count)
                            {
                                currentPlayerHand[i].sprite = cardSprites[BlackjackEngine.SecondHands[mainPlayer - 1][secondHandIndex].sortValue() - 1];
                                currentPlayerHand[i].enabled = true;
                            }
                            else
                            {
                                currentPlayerHand[i].enabled = false;
                            }
                        }
                        else
                        {
                            currentPlayerHand[i].sprite = cardSprites[BlackjackEngine.Hands[mainPlayer - 1][i].sortValue() - 1];
                            currentPlayerHand[i].enabled = true;
                        }
                    }

                    currentPlayerName.GetComponent<Text>().text = "Player " + mainPlayer + "\nchips: " + BlackjackEngine.Chips[mainPlayer - 1];

                    previousPlayer = BlackjackEngine.CurrentPlayer;
                    previousCards = Utils.IndependentCopyMatrix(BlackjackEngine.Hands);
                }

                if (BlackjackEngine.Table != null && !Utils.EqualCardLists(previousTable, BlackjackEngine.Table))
                {
                    //applying any changes in table cards
                    for (int i = 0; i < tableCards.Length; ++i)
                    {
                        if (BlackjackEngine.Table.Count < i + 1)
                        {
                            tableCards[i].enabled = false;
                        }
                        else
                        {
                            tableCards[i].sprite = cardSprites[BlackjackEngine.Table[i].sortValue() - 1];
                            tableCards[i].enabled = true;
                        }
                    }

                    previousTable = Utils.IndependentCopyList(BlackjackEngine.Table);
                }

                if (statusChange || previousBlackjackState != BlackjackEngine.gameState)
                {
                    BlackjackEngine.HandState mainHandState = BlackjackEngine.HandStates[BlackjackEngine.CurrentPlayer - 1];
                    BlackjackEngine.HandState secondHandState = BlackjackEngine.SecondHandStates[BlackjackEngine.CurrentPlayer - 1];
                    switch (BlackjackEngine.gameState)
                    {
                        case BlackjackEngine.GameState.normalTurn:
                            if (mainHandState == BlackjackEngine.HandState.fresh)
                                gameStateText.GetComponent<Text>().text = "It is player " + BlackjackEngine.CurrentPlayer + "'s turn to select an action.";
                            else if (mainHandState == BlackjackEngine.HandState.doubled)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has doubled their wager.";
                            else if (mainHandState == BlackjackEngine.HandState.split)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has split their hand.";
                            else if (mainHandState == BlackjackEngine.HandState.playing)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s turn continues.";
                            else if (mainHandState == BlackjackEngine.HandState.bust)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s hand is a bust.";
                            else if (mainHandState == BlackjackEngine.HandState.surrendered)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " surrendered.";
                            else if (mainHandState == BlackjackEngine.HandState.standing)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " is standing.";

                            if (mainHandState == BlackjackEngine.HandState.standing || mainHandState == BlackjackEngine.HandState.bust)
                            {
                                if (secondHandState == BlackjackEngine.HandState.split)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s turn to select an action for the split hand.";
                                else if (secondHandState == BlackjackEngine.HandState.doubled)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has doubled their wager on the split hand.";
                                if (secondHandState == BlackjackEngine.HandState.playing)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s turn continues with the split hand.";
                                if (secondHandState == BlackjackEngine.HandState.standing)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " is standing.";
                                if (secondHandState == BlackjackEngine.HandState.bust)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s hand is bust.";
                            }
                            break;

                        case BlackjackEngine.GameState.cardDealing:
                            if (BlackjackEngine.Table.Count > 1)
                                gameStateText.GetComponent<Text>().text = "Dealer is dealing house cards.";
                            else if (mainHandState == BlackjackEngine.HandState.doubled || secondHandState == BlackjackEngine.HandState.doubled)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has doubled.";
                            else if (mainHandState == BlackjackEngine.HandState.split)
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has split their hand.";
                            else
                                gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " has chosen hit.";
                            break;

                        case BlackjackEngine.GameState.endOfTurn:

                            if (BlackjackEngine.Table.Count > 1)
                            {
                                int houseTotal = BlackjackEngine.HandTotalValue(BlackjackEngine.Table);
                                if (houseTotal < 22)
                                    gameStateText.GetComponent<Text>().text = "House is standing with a total of " + houseTotal + ".";
                                else
                                    gameStateText.GetComponent<Text>().text = "House is bust.";
                            }
                            else
                            {
                                if (mainHandState == BlackjackEngine.HandState.bust)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s hand is a bust.";
                                else if (mainHandState == BlackjackEngine.HandState.surrendered)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " surrendered.";
                                else if (mainHandState == BlackjackEngine.HandState.standing)
                                    gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " is standing. with a total of " + BlackjackEngine.HandTotalValue(BlackjackEngine.Hands[BlackjackEngine.CurrentPlayer - 1]);

                                if (mainHandState == BlackjackEngine.HandState.standing || mainHandState == BlackjackEngine.HandState.bust)
                                {
                                    if (secondHandState == BlackjackEngine.HandState.standing)
                                        gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + " is standing with split hand with a total of " + BlackjackEngine.HandTotalValue(BlackjackEngine.SecondHands[BlackjackEngine.CurrentPlayer - 1]);
                                    else if (secondHandState == BlackjackEngine.HandState.bust)
                                        gameStateText.GetComponent<Text>().text = "Player " + BlackjackEngine.CurrentPlayer + "'s split hand is bust.";
                                }
                            }
                            break;

                        case BlackjackEngine.GameState.inactive:
                            gameStateText.GetComponent<Text>().text = "The game has not started";
                            //blackjack game does not end, if the state is inactive, it is only because the game has not started
                            break;
                    }

                    previousBlackjackState = BlackjackEngine.gameState;
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
            BlackjackSeats = (BlackjackEngine.PlayerType[])parameters[0];
            BlackjackEngine.seats = BlackjackSeats;
            BlackjackEngine.wager = (int)parameters[1];
            BlackjackEngine.startingChips = (int)parameters[2];
            BlackjackAI.AIType = (BlackjackAI.BlackjackAIType)parameters[3];
            CreateBlackjackDemoScene();
            BlackjackEngine.ResetGame();
        }

        /// <summary>
        /// Creates game objects in the scene that are relevant for Blackjack
        /// </summary>
        private void CreateBlackjackDemoScene()
        {
            GameObject board = new GameObject("board");
            board.transform.localPosition = new Vector3(0, 0, 0);
            currentPlayerHand = new SpriteRenderer[9];
            opponentHands = new SpriteRenderer[BlackjackSeats.Length - 1][];
            opponentNames = new GameObject[BlackjackSeats.Length - 1];
            tableCards = new SpriteRenderer[9];


            //current player hand
            for (int i = 0; i < currentPlayerHand.Length; ++i)
            {
                GameObject card = new GameObject("hand card " + (i + 1));
                SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                int cardIndex = 52;
                if (i < BlackjackEngine.Hands[BlackjackEngine.CurrentPlayer - 1].Count)
                {
                    cardIndex = (int)(BlackjackEngine.Hands[BlackjackEngine.CurrentPlayer - 1][i].suit - 1) * 13 + BlackjackEngine.Hands[BlackjackEngine.CurrentPlayer - 1][i].value - 1;
                }
                else
                    sr.enabled = false;

                Texture2D cardTexture = cardTextures[cardIndex];
                sr.sprite = cardSprites[cardIndex];
                sr.sortingOrder = 0;
                card.transform.parent = board.transform;
                card.transform.localPosition = new Vector3(-2 + -0.5f * currentPlayerHand.Length + i, -4f, 0);
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
            nameText.rectTransform.localPosition = new Vector3(-200, -320f, 0);
            nameText.rectTransform.localScale = new Vector3(1, 1, 1);
            nameText.text = "Player " + BlackjackEngine.CurrentPlayer + ": " + BlackjackEngine.Chips[BlackjackEngine.CurrentPlayer - 1];
            nameText.alignment = TextAnchor.MiddleCenter;

            //opponents' hands
            for (int i = 0; i < BlackjackSeats.Length - 1; ++i)
            {
                opponentHands[i] = new SpriteRenderer[9];

                for (int k = 0; k < opponentHands[i].Length; ++k)
                {
                    GameObject card = new GameObject("opponent " + (i + 1) + " card " + (k + 1));
                    SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
                    int cardIndex = 52;
                    sr.sortingOrder = k;
                    if (BlackjackEngine.Hands[i + 1].Count <= k)
                        sr.enabled = false;
                    else
                    {
                        cardIndex = BlackjackEngine.Hands[i + 1][k].sortValue();
                        sr.enabled = true;
                    }
                    Texture2D cardTexture = cardTextures[cardIndex];
                    sr.sprite = cardSprites[cardIndex];
                    card.transform.parent = board.transform;
                    card.transform.localPosition = new Vector3(-0.2f * opponentHands[i].Length - (BlackjackSeats.Length - 3) * 0.1f + k * 0.4f - 5.4f + i % 2 * 3f, 2.4f - (i / 2) * 1.6f, 0);
                    card.transform.localScale = new Vector3(60f / cardTexture.width, 384f / 256f * 60f / cardTexture.height, 1);
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
                nameText.rectTransform.localPosition = new Vector3(216 * (i % 2) - 490f, 150f - 115 * (i / 2), 0);
                nameText.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.text = "Player " + (i + 2) + ": " + BlackjackEngine.Chips[i + 1];
                nameText.alignment = TextAnchor.MiddleCenter;
            }

            //table
            for (int i = 0; i < tableCards.Length; ++i)
            {
                GameObject tableCard = new GameObject("Table card " + (i + 1));
                SpriteRenderer srTable = tableCard.AddComponent<SpriteRenderer>();
                int tableCardIndex = 52;
                if (BlackjackEngine.Table.Count > i)
                {
                    tableCardIndex = BlackjackEngine.Table[i].sortValue() - 1;
                    srTable.enabled = true;
                }
                else
                {
                    srTable.enabled = false;
                }
                Texture2D tableCardTexture = cardTextures[tableCardIndex];
                srTable.sprite = cardSprites[tableCardIndex];
                srTable.sortingOrder = i;
                tableCard.transform.parent = board.transform;
                tableCard.transform.localPosition = new Vector3(-1.5f + 0.7f * i, -1f, 0);
                tableCard.transform.localScale = new Vector3(100f / tableCardTexture.width, 384f / 256f * 100f / tableCardTexture.height, 1);
                tableCards[i] = srTable;
            }


            //action buttons

            //surrender -button
            GameObject surrenderb = new GameObject("Surrender");
            surrenderb.layer = 5; //default UI layer
            surrenderb.transform.parent = UICanvas.transform;
            Image surrenderbi = surrenderb.AddComponent<Image>();
            surrenderbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            surrenderbi.preserveAspect = true;
            Button surrenderbb = surrenderb.AddComponent<Button>();
            surrenderbb.targetGraphic = surrenderbi;
            surrenderbi.rectTransform.sizeDelta = new Vector2(100, 50);
            surrenderbi.rectTransform.localPosition = new Vector3(350, 150, 0);
            surrenderbi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject surrenderbt = new GameObject("Surrender Text");
            surrenderbt.layer = 5; //default UI layer
            surrenderbt.transform.parent = surrenderb.transform;
            Text surrenderbtt = surrenderbt.AddComponent<Text>();
            surrenderbtt.font = defaultFont;
            surrenderbtt.fontSize = 15;
            surrenderbtt.color = Color.white;
            surrenderbtt.rectTransform.sizeDelta = new Vector2(100, 50);
            surrenderbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            surrenderbtt.rectTransform.localScale = new Vector3(1, 1, 1);
            surrenderbtt.text = "Surrender";
            surrenderbtt.alignment = TextAnchor.MiddleCenter;
            surrenderbb.onClick.AddListener(BlackjackSurrenderOnClick);

            //stand -button
            GameObject standb = new GameObject("Stand");
            standb.layer = 5; //default UI layer
            standb.transform.parent = UICanvas.transform;
            Image standbi = standb.AddComponent<Image>();
            standbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            standbi.preserveAspect = true;
            Button standbb = standb.AddComponent<Button>();
            standbb.targetGraphic = standbi;
            standbi.rectTransform.sizeDelta = new Vector2(100, 50);
            standbi.rectTransform.localPosition = new Vector3(350, 50, 0);
            standbi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject standbt = new GameObject("Stand Text");
            standbt.layer = 5; //default UI layer
            standbt.transform.parent = standb.transform;
            Text standbtt = standbt.AddComponent<Text>();
            standbtt.font = defaultFont;
            standbtt.fontSize = 15;
            standbtt.color = Color.white;
            standbtt.rectTransform.sizeDelta = new Vector2(100, 50);
            standbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            standbtt.rectTransform.localScale = new Vector3(1, 1, 1);
            standbtt.text = "Stand";
            standbtt.alignment = TextAnchor.MiddleCenter;
            standbb.onClick.AddListener(BlackjackStandOnClick);

            //hit -button
            GameObject hitb = new GameObject("Hit");
            hitb.layer = 5; //default UI layer
            hitb.transform.parent = UICanvas.transform;
            Image hitbi = hitb.AddComponent<Image>();
            hitbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            hitbi.preserveAspect = true;
            Button hitbb = hitb.AddComponent<Button>();
            hitbb.targetGraphic = hitbi;
            hitbi.rectTransform.sizeDelta = new Vector2(100, 50);
            hitbi.rectTransform.localPosition = new Vector3(350, -50, 0);
            hitbi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject hitbt = new GameObject("Hit Text");
            hitbt.layer = 5; //default UI layer
            hitbt.transform.parent = hitb.transform;
            Text hitbtt = hitbt.AddComponent<Text>();
            hitbtt.font = defaultFont;
            hitbtt.fontSize = 15;
            hitbtt.color = Color.white;
            hitbtt.rectTransform.sizeDelta = new Vector2(100, 50);
            hitbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            hitbtt.rectTransform.localScale = new Vector3(1, 1, 1);
            hitbtt.text = "Hit";
            hitbtt.alignment = TextAnchor.MiddleCenter;
            hitbb.onClick.AddListener(BlackjackHitOnClick);

            //double -button
            GameObject doubleb = new GameObject("Double");
            doubleb.layer = 5; //default UI layer
            doubleb.transform.parent = UICanvas.transform;
            Image doublebi = doubleb.AddComponent<Image>();
            doublebi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            doublebi.preserveAspect = true;
            Button doublebb = doubleb.AddComponent<Button>();
            doublebb.targetGraphic = doublebi;
            doublebi.rectTransform.sizeDelta = new Vector2(100, 50);
            doublebi.rectTransform.localPosition = new Vector3(350, -150, 0);
            doublebi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject doublebt = new GameObject("Double Text");
            doublebt.layer = 5; //default UI layer
            doublebt.transform.parent = doubleb.transform;
            Text doublebtt = doublebt.AddComponent<Text>();
            doublebtt.font = defaultFont;
            doublebtt.fontSize = 15;
            doublebtt.color = Color.white;
            doublebtt.rectTransform.sizeDelta = new Vector2(100, 50);
            doublebtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            doublebtt.rectTransform.localScale = new Vector3(1, 1, 1);
            doublebtt.text = "Double";
            doublebtt.alignment = TextAnchor.MiddleCenter;
            doublebb.onClick.AddListener(BlackjackDoubleOnClick);

            //split -button
            GameObject splitb = new GameObject("Split");
            splitb.layer = 5; //default UI layer
            splitb.transform.parent = UICanvas.transform;
            Image splitbi = splitb.AddComponent<Image>();
            splitbi.sprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), new Vector2(0, 0));
            splitbi.preserveAspect = true;
            Button splitbb = splitb.AddComponent<Button>();
            splitbb.targetGraphic = splitbi;
            splitbi.rectTransform.sizeDelta = new Vector2(100, 50);
            splitbi.rectTransform.localPosition = new Vector3(350, -250, 0);
            splitbi.rectTransform.localScale = new Vector3(1, 1, 1);
            GameObject splitbt = new GameObject("Split Text");
            splitbt.layer = 5; //default UI layer
            splitbt.transform.parent = splitb.transform;
            Text splitbtt = splitbt.AddComponent<Text>();
            splitbtt.font = defaultFont;
            splitbtt.fontSize = 15;
            splitbtt.color = Color.white;
            splitbtt.rectTransform.sizeDelta = new Vector2(100, 50);
            splitbtt.rectTransform.localPosition = new Vector3(0, 0, 0);
            splitbtt.rectTransform.localScale = new Vector3(1, 1, 1);
            splitbtt.text = "Split";
            splitbtt.alignment = TextAnchor.MiddleCenter;
            splitbb.onClick.AddListener(BlackjackSplitOnClick);

            previousCards = Utils.IndependentCopyMatrix(BlackjackEngine.Hands);
            previousPlayer = BlackjackEngine.CurrentPlayer;
            previousTable = Utils.IndependentCopyList(BlackjackEngine.Table);
            previousBlackjackState = BlackjackEngine.GameState.inactive;

            previousHandStates = new BlackjackEngine.HandState[BlackjackSeats.Length];
            for (int i = 0; i < BlackjackEngine.HandStates.Length; ++i)
            {
                previousHandStates[i] = BlackjackEngine.HandStates[i];
            }
            previousSecondHandStates = new BlackjackEngine.HandState[BlackjackSeats.Length];
            for (int i = 0; i < BlackjackEngine.SecondHandStates.Length; ++i)
            {
                previousSecondHandStates[i] = BlackjackEngine.SecondHandStates[i];
            }


            //setting the main player as the first local player or if none, player 1
            mainPlayer = 0;
            for (int i = 0; i < BlackjackSeats.Length; ++i)
            {
                if (BlackjackSeats[i] == BlackjackEngine.PlayerType.local)
                    mainPlayer = i + 1;
            }
            if (mainPlayer == 0)
                mainPlayer = BlackjackEngine.CurrentPlayer;
        }

        void BlackjackSurrenderOnClick()
        {
            BlackjackEngine.LocalMove(BlackjackEngine.BlackjackMove.surrender);
        }
        void BlackjackStandOnClick()
        {
            BlackjackEngine.LocalMove(BlackjackEngine.BlackjackMove.stand);
        }
        void BlackjackHitOnClick()
        {
            BlackjackEngine.LocalMove(BlackjackEngine.BlackjackMove.hit);
        }
        void BlackjackDoubleOnClick()
        {
            BlackjackEngine.LocalMove(BlackjackEngine.BlackjackMove.doubleDown);
        }
        void BlackjackSplitOnClick()
        {
            BlackjackEngine.LocalMove(BlackjackEngine.BlackjackMove.split);
        }
    }
}
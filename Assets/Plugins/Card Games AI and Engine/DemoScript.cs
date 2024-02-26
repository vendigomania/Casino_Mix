using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CGAIE.DemoScript;

namespace CGAIE
{
    /// <summary>
    /// Unity component that includes a demo for all 6 card games
    /// </summary>
    public class DemoScript : MonoBehaviour
    {
        /// <summary>
        /// Enumeration for specifying the game.
        /// </summary>
        public enum DemoGame
        {
            Hearts = 1,
            Casino = 2,
            Sevens = 3,
            NinetyNine = 4,
            GOPS = 5,
            Blackjack = 6,
            TopTrumps = 7
        }

        /// <summary>
        /// Type of the game
        /// </summary>
        public DemoGame demoGame;

        public Button ResetBtn;
        public Text GameStateText;
        //for sevens
        public Button AdditionalBtn;
        public static Button GlobalBtn; 

        /// <summary>
        /// The texture used for highlighting the selected cards.
        /// </summary>
        public Texture2D selectedCardTexture;

        /// <summary>
        /// The font that is used throughout the demo.
        /// </summary>
        public Font defaultFont;

        /// <summary>
        /// The AI type for Hearts AI. Default: Hard
        /// </summary>
        public HeartsAI.HeartsAIType AITypeHearts = HeartsAI.HeartsAIType.hard;

        /// <summary>
        /// The player types for each seat in Hearts.
        /// </summary>
        public HeartsEngine.PlayerType[] heartsSeats = new HeartsEngine.PlayerType[4] { HeartsEngine.PlayerType.local, HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI };

        /// <summary>
        /// The AI type for Casino AI. Default: Hard
        /// </summary>
        public CasinoAI.CasinoAIType AITypeCasino = CasinoAI.CasinoAIType.hard;

        /// <summary>
        /// The player types for each seat in Casino.
        /// </summary>
        public CasinoEngine.PlayerType[] casinoSeats = new CasinoEngine.PlayerType[2] { CasinoEngine.PlayerType.local, CasinoEngine.PlayerType.AI };

        /// <summary>
        /// The AI type for Sevens AI. Default: Hard
        /// </summary>
        public SevensAI.SevensAIType AITypeSevens = SevensAI.SevensAIType.hard;

        /// <summary>
        /// The player types for each seat in Sevens.
        /// </summary>
        public SevensEngine.PlayerType[] sevensSeats = new SevensEngine.PlayerType[4] { SevensEngine.PlayerType.local, SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI };

        /// <summary>
        /// The AI type for Ninety Nine AI. Default: Hard
        /// </summary>
        public NinetyNineAI.NinetyNineAIType AITypeNinetyNine = NinetyNineAI.NinetyNineAIType.hard;

        /// <summary>
        /// The player types for each seat in Ninety Nine.
        /// </summary>
        public NinetyNineEngine.PlayerType[] ninetyNineSeats = new NinetyNineEngine.PlayerType[4] { NinetyNineEngine.PlayerType.local, NinetyNineEngine.PlayerType.AI, NinetyNineEngine.PlayerType.AI, NinetyNineEngine.PlayerType.AI };

        /// <summary>
        /// The AI type for GOPS AI. Default: Hard
        /// </summary>
        public GOPSAI.GOPSAIType AITypeGOPS = GOPSAI.GOPSAIType.hard;

        /// <summary>
        /// The player types for each seat in GOPS.
        /// </summary>
        public GOPSEngine.PlayerType[] GOPSSeats = new GOPSEngine.PlayerType[4] { GOPSEngine.PlayerType.local, GOPSEngine.PlayerType.AI, GOPSEngine.PlayerType.AI, GOPSEngine.PlayerType.AI };

        /// <summary>
        /// The AI type for Blackjack AI. Default: Hard
        /// </summary>
        public BlackjackAI.BlackjackAIType AITypeBlackjack = BlackjackAI.BlackjackAIType.hard;

        /// <summary>
        /// The player types for each seat in Blackjack.
        /// </summary>
        public BlackjackEngine.PlayerType[] BlackjackSeats = new BlackjackEngine.PlayerType[4] { BlackjackEngine.PlayerType.local, BlackjackEngine.PlayerType.AI, BlackjackEngine.PlayerType.AI, BlackjackEngine.PlayerType.AI };
        
        /// <summary>
        /// The base wager in blackjack.
        /// </summary>
        public int blackjackWager = 2;

        /// <summary>
        /// The amount of chips for every player at the start of a blackjack game.
        /// </summary>
        public int blackjackStartingChips = 10;

        /// <summary>
        /// The AI type for Blackjack AI. Default: Hard
        /// </summary>
        public TopTrumpsAI.TopTrumpsAIType AITypeTopTrumps = TopTrumpsAI.TopTrumpsAIType.hard;

        /// <summary>
        /// The player types for each seat in Blackjack.
        /// </summary>
        public TopTrumpsEngine.PlayerType[] TopTrumpsSeats = new TopTrumpsEngine.PlayerType[2] { TopTrumpsEngine.PlayerType.local, TopTrumpsEngine.PlayerType.AI };

        /// <summary>
        /// Are there tie-breakers in Top Trumps. If not, after a tie, everyone takes their cards back and puts them on the "unshuffled" cards.
        /// </summary>
        public bool TopTrumpsNoTieBreakers = false;

        /// <summary>
        /// Should tie-breakers only include players who tied in the first place.
        /// </summary>
        public bool TopTrumpsPrivateTieBreakers = true;

        /// <summary>
        /// If the lowest and highest possible value of the relevant attribute are both played during the same round, does the lowest value player win instead. For example, a two (lowest value) would win against the ace (highest value).
        /// </summary>
        public bool TopTrumpsLowestValueBeatsHighest = false;

        /// <summary>
        /// How many duplicates of each card there is in the demo deck. If 0, there is only 1 of each card.
        /// </summary>
        public uint TopTrumpsDuplicates = 1;

        /// <summary>
        /// Does the demo use custom cards with custom attributes, or classic playing cards with their value and the suit as attributes.
        /// <para>The "value" of suit is alphabetical: clubs, diamonds, hearts, spades</para>
        /// </summary>
        public bool TopTrumpsCustomCards = true;

        //private variables
        private Sprite[] cardSprites;
        private Canvas UICanvas;
        private bool missingValues = false;
        private ICardGameDemo demo;

        // Start is called before the first frame update
        void Start()
        {
            GlobalBtn = AdditionalBtn;

            CreateGeneralGameObjects();

            if (!missingValues)
            {
                //create the demo scene
                switch (demoGame)
                {
                    case DemoGame.Hearts:
                        demo = new HeartsDemo();
                        demo.StartDemo(heartsSeats, AITypeHearts);
                        break;

                    case DemoGame.Casino:
                        demo = new CasinoDemo();
                        demo.StartDemo(casinoSeats, AITypeCasino);
                        break;

                    case DemoGame.Sevens:
                        demo = new SevensDemo();
                        demo.StartDemo(sevensSeats, AITypeSevens);
                        break;

                    case DemoGame.NinetyNine:
                        demo = new NinetyNineDemo();
                        demo.StartDemo(ninetyNineSeats, AITypeNinetyNine);
                        break;

                    case DemoGame.GOPS:
                        demo = new GOPSDemo();
                        demo.StartDemo(GOPSSeats, AITypeGOPS);
                        break;

                    case DemoGame.Blackjack:
                        demo = new BlackjackDemo();
                        demo.StartDemo(BlackjackSeats, blackjackWager, blackjackStartingChips, AITypeBlackjack);
                        break;

                    case DemoGame.TopTrumps:
                        demo = new TopTrumpsDemo();
                        if (TopTrumpsCustomCards)
                            demo.StartDemo(TopTrumpsSeats, AITypeTopTrumps, TopTrumpsNoTieBreakers, TopTrumpsPrivateTieBreakers, TopTrumpsLowestValueBeatsHighest, TopTrumpsDuplicates, TopTrumpsCustomCards, new string[3] { "Corners", "Area", "Power" });
                        else
                            demo.StartDemo(TopTrumpsSeats, AITypeTopTrumps, TopTrumpsNoTieBreakers, TopTrumpsPrivateTieBreakers, TopTrumpsLowestValueBeatsHighest, TopTrumpsDuplicates, TopTrumpsCustomCards, new string[2] { "Value", "Suit" });
                        break;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (demo != null && !missingValues)
                demo.Update();
        }

        /// <summary>
        /// Creates game objects that are shared between each game, such as status text and reset-button
        /// </summary>
        private void CreateGeneralGameObjects()
        {
            try
            {
                //placing the main camera correctly for the demo
                Camera mainCamera = Camera.main;

                //Creating the UI canvas
                GameObject tcGO = new GameObject("TopUI");
                tcGO.transform.parent = transform;
                tcGO.layer = 5; //default UI layer
                Canvas tc = tcGO.AddComponent<Canvas>();
                tc.renderMode = RenderMode.ScreenSpaceCamera;
                tc.worldCamera = Camera.main;
                tc.planeDistance = 10;
                tc.sortingOrder = 5;
                UICanvas = tc;
                CanvasScaler tcs = tcGO.AddComponent<CanvasScaler>();
                tcs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                tcs.referenceResolution = new Vector2(1920, 1080);
                tcs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                tcs.matchWidthOrHeight = 1;
                tcGO.AddComponent<GraphicRaycaster>();

                //Creating the Reset-button
                ResetBtn.onClick.AddListener(ResetOnClick);

                //preloading card sprites
                var list = Resources.LoadAll<Sprite>("/").ToList();
                var temp = list[0];
                list.Remove(temp);
                list.Add(temp);

                if (demoGame == DemoGame.TopTrumps && TopTrumpsCustomCards)
                    cardSprites = new Sprite[11];
                else
                    cardSprites = new Sprite[53];
                for (int i = 0; i < cardSprites.Length; ++i)
                {
                    cardSprites[i] = list[i];
                }

                //initializing demobase variables
                DemoBase.demoGame = demoGame;
                DemoBase.selectedCardTexture = selectedCardTexture;
                DemoBase.defaultFont = defaultFont;
                DemoBase.cardSprites = cardSprites;
                DemoBase.UICanvas = UICanvas;
                DemoBase.gameStateText = GameStateText.gameObject;

                //making sure all textures are defined
                if (selectedCardTexture == null)
                    throw new System.NullReferenceException("Selected card texture is not defined.");

                if (defaultFont == null)
                    throw new System.NullReferenceException("Default font is not defined.");
            }
            catch
            {
                Debug.LogWarning("All textures for the demo have to be defined!\nCheck the Demo Script component for missing values.");
                missingValues = true;
            }
        }


        /// <summary>
        /// Executed when the Reset-button is clicked. Notifies the relevant game engine of the click
        /// </summary>
        void ResetOnClick()
        {

            switch (demoGame)
            {
                case DemoGame.Hearts:
                    HeartsEngine.ResetGame();
                    break;

                case DemoGame.Casino:
                    CasinoEngine.ResetGame();
                    break;

                case DemoGame.Sevens:
                    SevensEngine.ResetGame();
                    break;

                case DemoGame.NinetyNine:
                    NinetyNineEngine.ResetGame();
                    break;

                case DemoGame.GOPS:
                    GOPSEngine.ResetGame();
                    break;

                case DemoGame.Blackjack:
                    BlackjackEngine.ResetGame();
                    break;

                case DemoGame.TopTrumps:
                    TopTrumpsEngine.ResetGame();
                    break;
            }
        }

        /// <summary>
        /// Executed when the application is quit. Stops the relevant game's AI if needed
        /// </summary>
        void OnApplicationQuit()
        {
            switch (demoGame)
            {
                case DemoGame.Hearts:
                    HeartsEngine.StopAI();
                    break;

                case DemoGame.Casino:
                    CasinoEngine.StopAI();
                    break;

                case DemoGame.Sevens:
                    SevensEngine.StopAI();
                    break;

                case DemoGame.NinetyNine:
                    NinetyNineEngine.StopAI();
                    break;

                case DemoGame.GOPS:
                    GOPSEngine.StopAI();
                    break;

                case DemoGame.Blackjack:
                    BlackjackEngine.StopAI();
                    break;

                case DemoGame.TopTrumps:
                    TopTrumpsEngine.StopAI();
                    break;
            }
        }
    }

    /// <summary>
    /// Static class to handle all general demo game -related things such as clicks and textures.
    /// </summary>
    internal static class DemoBase
    {
        //internal variables
        internal static DemoGame demoGame;
        internal static Texture2D buttonTexture;
        internal static Texture2D selectedCardTexture;
        internal static Font defaultFont;
        internal static Texture2D[] cardTextures;
        internal static string cardTextureFolder = "Assets/Plugins/Card Games AI and Engine/Textures/Card Textures/";
        internal static SpriteRenderer[] currentPlayerHand;
        internal static SpriteRenderer[] currentPlayerHandHighlights;
        internal static GameObject currentPlayerName;
        internal static SpriteRenderer[][] opponentHands;
        internal static GameObject[] opponentNames;
        internal static SpriteRenderer[] tableCards;
        internal static SpriteRenderer[] tableCardsHighlights;
        internal static SpriteRenderer usedCardSR;
        internal static SpriteRenderer[] usedCardSRs;
        internal static List<Card>[] previousCards;
        internal static List<CustomCard>[] previousCustomCards;
        internal static List<CustomCard>[] previousUnShuffledCustomCards;
        internal static List<Card> previousTable;
        internal static List<CustomCard> previousCustomCardTable;
        internal static BlackjackEngine.HandState[] previousHandStates;
        internal static BlackjackEngine.HandState[] previousSecondHandStates;
        internal static int previousPlayer;
        internal static int mainPlayer = 1;
        internal static HeartsEngine.GameState previousHeartsState;
        internal static CasinoEngine.GameState previousCasinoState;
        internal static SevensEngine.GameState previousSevensState;
        internal static NinetyNineEngine.GameState previousNinetyNineState;
        internal static GOPSEngine.GameState previousGOPSState;
        internal static BlackjackEngine.GameState previousBlackjackState;
        internal static TopTrumpsEngine.GameState previousTopTrumpsState;
        internal static GameObject gameStateText;
        internal static Text deckText;
        internal static Text[] opponentDeckTexts;
        internal static Text tableValueText;
        internal static Sprite[] cardSprites;
        internal static Canvas UICanvas;
        internal static List<Card> selectedHandCards;
        internal static List<Card> selectedTableCards;
        internal static int[] handCardValues;
        internal static Text[] handCardValueTexts;
        internal static Card[] handCards;

        /// <summary>
        /// Clears all highlights of selected cards in either hand or the table
        /// </summary>
        internal static void ClearHighlights()
        {
            selectedHandCards.Clear();
            selectedTableCards.Clear();
            for (int i = 0; i < currentPlayerHandHighlights.Length; ++i)
            {
                currentPlayerHandHighlights[i].enabled = false;
            }
            for (int i = 0; i < tableCardsHighlights.Length; ++i)
            {
                tableCardsHighlights[i].enabled = false;
            }
        }

        /// <summary>
        /// Notifies the relevant game engine of clicks on the hand cards
        /// </summary>
        /// <param name="index"></param>
        internal static void NotifyClick(int index)
        {
            switch (demoGame)
            {
                case DemoGame.Hearts:
                    if (HeartsEngine.Hands[HeartsEngine.CurrentPlayer - 1].Count > index)
                        HeartsEngine.LocalMove(HeartsEngine.Hands[HeartsEngine.CurrentPlayer - 1][index]);
                    break;

                case DemoGame.Casino:
                    if (CasinoEngine.gameState == CasinoEngine.GameState.normalTurn && CasinoEngine.seats[CasinoEngine.CurrentPlayer - 1] == CasinoEngine.PlayerType.local)
                    {
                        if (index < 10 && CasinoEngine.Hands[CasinoEngine.CurrentPlayer - 1].Count > index)
                        {
                            if (selectedHandCards.Count == 0)
                            {
                                selectedHandCards.Add(CasinoEngine.Hands[CasinoEngine.CurrentPlayer - 1][index]);
                                currentPlayerHandHighlights[index].enabled = true;
                            }
                            else if (selectedHandCards[0] == CasinoEngine.Hands[CasinoEngine.CurrentPlayer - 1][index])
                            {
                                selectedHandCards.Clear();
                                currentPlayerHandHighlights[index].enabled = false;
                            }
                            else
                            {
                                selectedHandCards.Clear();
                                for (int i = 0; i < currentPlayerHandHighlights.Length; ++i)
                                {
                                    currentPlayerHandHighlights[i].enabled = false;
                                }

                                selectedHandCards.Add(CasinoEngine.Hands[CasinoEngine.CurrentPlayer - 1][index]);

                                currentPlayerHandHighlights[index].enabled = true;
                            }
                        }
                        else if (index >= 10 && CasinoEngine.Table.Count > index - 10)
                        {
                            if (selectedTableCards.Contains(CasinoEngine.Table[index - 10]))
                            {
                                selectedTableCards.Remove(CasinoEngine.Table[index - 10]);
                                tableCardsHighlights[index - 10].enabled = false;
                            }
                            else
                            {
                                selectedTableCards.Add(CasinoEngine.Table[index - 10]);
                                tableCardsHighlights[index - 10].enabled = true;
                            }
                        }
                    }
                    break;

                case DemoGame.Sevens:
                    if (SevensEngine.Hands[SevensEngine.CurrentPlayer - 1].Count > index)
                        SevensEngine.LocalMove(SevensEngine.Hands[SevensEngine.CurrentPlayer - 1][index]);
                    break;


                case DemoGame.NinetyNine:
                    if (handCards.Length > index)
                        NinetyNineEngine.LocalMove(handCards[index], handCardValues[index]);
                    break;

                case DemoGame.GOPS:
                    if (GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1].Count > index)
                        GOPSEngine.LocalMove(GOPSEngine.Hands[GOPSEngine.CurrentPlayer - 1][index]);
                    break;

                //nothing for blackjack or Top Trumps since clicking cards doesn't do anything there
            }
        }
    }

    /// <summary>
    /// Interface for a card game demo
    /// </summary>
    internal interface ICardGameDemo
    {
        /// <summary>
        /// Executed once each frame.
        /// </summary>
        internal void Update();

        /// <summary>
        /// Executed once at the start
        /// </summary>
        /// <param name="parameters">list of parameters relevant for the demo</param>
        internal void StartDemo(params object[] parameters);
    }
}
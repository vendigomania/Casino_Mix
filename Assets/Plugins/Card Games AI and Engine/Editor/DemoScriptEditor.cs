using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CGAIE
{
    /// <summary>
    /// Custom Inspector for card game AI & engine pack
    /// </summary>
    [CustomEditor(typeof(DemoScript))]
    public class DemoScriptEditor : Editor
    {
        SerializedProperty heartsSeats;
        SerializedProperty casinoSeats;
        SerializedProperty sevensSeats;
        SerializedProperty ninetyNineSeats;
        SerializedProperty gopsSeats;
        SerializedProperty blackjackSeats;
        SerializedProperty topTrumpsSeats;

        /// <summary>
        /// Initializing properties when the inspector is first loaded
        /// </summary>
        private void OnEnable()
        {
            heartsSeats = serializedObject.FindProperty("heartsSeats");
            casinoSeats = serializedObject.FindProperty("casinoSeats");
            sevensSeats = serializedObject.FindProperty("sevensSeats");
            ninetyNineSeats = serializedObject.FindProperty("ninetyNineSeats");
            gopsSeats = serializedObject.FindProperty("GOPSSeats");
            blackjackSeats = serializedObject.FindProperty("BlackjackSeats");
            topTrumpsSeats = serializedObject.FindProperty("TopTrumpsSeats");
        }

        public override void OnInspectorGUI()
        {
            DemoScript demoScript = (DemoScript)target;
            demoScript.demoGame = (DemoScript.DemoGame)EditorGUILayout.EnumPopup("Demo Type", demoScript.demoGame);

            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ResetBtn", GUILayout.Width(119), GUILayout.ExpandWidth(true));
            demoScript.ResetBtn = (Button)EditorGUILayout.ObjectField(demoScript.ResetBtn, typeof(Button), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AdditionalBtn", GUILayout.Width(119), GUILayout.ExpandWidth(true));
            demoScript.AdditionalBtn = (Button)EditorGUILayout.ObjectField(demoScript.AdditionalBtn, typeof(Button), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("StatText", GUILayout.Width(119), GUILayout.ExpandWidth(true));
            demoScript.GameStateText = (Text)EditorGUILayout.ObjectField(demoScript.GameStateText, typeof(Text), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Selected Card", GUILayout.Width(119), GUILayout.ExpandWidth(true));
            demoScript.selectedCardTexture = (Texture2D)EditorGUILayout.ObjectField(demoScript.selectedCardTexture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Font", GUILayout.Width(119), GUILayout.ExpandWidth(true));
            demoScript.defaultFont = (Font)EditorGUILayout.ObjectField(demoScript.defaultFont, typeof(Font), false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();

            switch (demoScript.demoGame)
            {
                case DemoScript.DemoGame.Hearts:
                    demoScript.AITypeHearts = (HeartsAI.HeartsAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeHearts);
                    EditorGUILayout.PropertyField(heartsSeats, true);
                    break;

                case DemoScript.DemoGame.Casino:
                    demoScript.AITypeCasino = (CasinoAI.CasinoAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeCasino);
                    EditorGUILayout.PropertyField(casinoSeats, true);
                    break;

                case DemoScript.DemoGame.Sevens:
                    demoScript.AITypeSevens = (SevensAI.SevensAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeSevens);
                    EditorGUILayout.PropertyField(sevensSeats, true);
                    break;

                case DemoScript.DemoGame.NinetyNine:
                    demoScript.AITypeNinetyNine = (NinetyNineAI.NinetyNineAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeNinetyNine);
                    EditorGUILayout.PropertyField(ninetyNineSeats, true);
                    break;

                case DemoScript.DemoGame.GOPS:
                    demoScript.AITypeGOPS = (GOPSAI.GOPSAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeGOPS);
                    EditorGUILayout.PropertyField(gopsSeats, true);
                    break;

                case DemoScript.DemoGame.Blackjack:
                    demoScript.AITypeBlackjack = (BlackjackAI.BlackjackAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeBlackjack);
                    demoScript.blackjackWager = EditorGUILayout.IntField("Wager", demoScript.blackjackWager);
                    demoScript.blackjackStartingChips = EditorGUILayout.IntField("Chips", demoScript.blackjackStartingChips);
                    EditorGUILayout.PropertyField(blackjackSeats, true);
                    break;

                case DemoScript.DemoGame.TopTrumps:
                    demoScript.TopTrumpsNoTieBreakers = EditorGUILayout.Toggle(new GUIContent("No tie-breakers", "Instead of tie-breakers, everyone takes their cards back to their unshuffled cards."), demoScript.TopTrumpsNoTieBreakers);
                    EditorGUI.BeginDisabledGroup(demoScript.TopTrumpsNoTieBreakers);
                    demoScript.TopTrumpsPrivateTieBreakers = EditorGUILayout.Toggle(new GUIContent("Private tie-breakers", "Only the players who tied are allowed to participate in a tie-breaker."), demoScript.TopTrumpsPrivateTieBreakers);
                    EditorGUI.EndDisabledGroup();
                    demoScript.TopTrumpsLowestValueBeatsHighest = EditorGUILayout.Toggle(new GUIContent("Lowest value beats highest", "If a round includes the lowest and highest possible values of an attribute, the lowest value wins instead. For example a two vs. an ace."), demoScript.TopTrumpsLowestValueBeatsHighest);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Deck", "The type of the deck. Either normal playing cards with value and suit as the attributes, or a custom demo deck."));
                    EditorGUILayout.BeginVertical();
                    demoScript.TopTrumpsCustomCards = !EditorGUILayout.Toggle("Normal", !demoScript.TopTrumpsCustomCards);
                    demoScript.TopTrumpsCustomCards = EditorGUILayout.Toggle("Custom", demoScript.TopTrumpsCustomCards);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    int duplicatesValue = EditorGUILayout.IntField(new GUIContent("Card duplicates", "Amount of duplicates of each card in the deck. 0 Means there's just 1 of each card."), (int)demoScript.TopTrumpsDuplicates);
                    if (duplicatesValue < 0)
                        duplicatesValue = 0;
                    demoScript.TopTrumpsDuplicates = (uint)duplicatesValue;
                    demoScript.AITypeTopTrumps = (TopTrumpsAI.TopTrumpsAIType)EditorGUILayout.EnumPopup("AI Type", demoScript.AITypeTopTrumps);
                    EditorGUILayout.PropertyField(topTrumpsSeats, true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(demoScript);
                EditorSceneManager.MarkSceneDirty(demoScript.gameObject.scene);

                //Applying seat limits in inspector. The same limits are present in the engine itself
                if (demoScript.heartsSeats.Length > 6)
                {
                    demoScript.heartsSeats = new HeartsEngine.PlayerType[6] { demoScript.heartsSeats[0], demoScript.heartsSeats[1], demoScript.heartsSeats[2], demoScript.heartsSeats[3], demoScript.heartsSeats[4], demoScript.heartsSeats[5] };
                }
                else if (demoScript.heartsSeats.Length < 3)
                {
                    if (demoScript.heartsSeats.Length == 2)
                        demoScript.heartsSeats = new HeartsEngine.PlayerType[3] { demoScript.heartsSeats[0], demoScript.heartsSeats[1], HeartsEngine.PlayerType.AI };
                    else if (demoScript.heartsSeats.Length == 1)
                        demoScript.heartsSeats = new HeartsEngine.PlayerType[3] { demoScript.heartsSeats[0], HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI };
                    else if (demoScript.heartsSeats.Length == 0)
                        demoScript.heartsSeats = new HeartsEngine.PlayerType[3] { HeartsEngine.PlayerType.local, HeartsEngine.PlayerType.AI, HeartsEngine.PlayerType.AI };
                }

                if (demoScript.casinoSeats.Length > 4)
                {
                    demoScript.casinoSeats = new CasinoEngine.PlayerType[4] { demoScript.casinoSeats[0], demoScript.casinoSeats[1], demoScript.casinoSeats[2], demoScript.casinoSeats[3] };
                }
                else if (demoScript.casinoSeats.Length < 2)
                {
                    if (demoScript.casinoSeats.Length == 1)
                        demoScript.casinoSeats = new CasinoEngine.PlayerType[2] { demoScript.casinoSeats[0], CasinoEngine.PlayerType.AI };
                    else if (demoScript.casinoSeats.Length == 0)
                        demoScript.casinoSeats = new CasinoEngine.PlayerType[2] { CasinoEngine.PlayerType.local, CasinoEngine.PlayerType.AI };
                }

                if (demoScript.sevensSeats.Length > 6)
                {
                    demoScript.sevensSeats = new SevensEngine.PlayerType[6] { demoScript.sevensSeats[0], demoScript.sevensSeats[1], demoScript.sevensSeats[2], demoScript.sevensSeats[3], demoScript.sevensSeats[4], demoScript.sevensSeats[5] };
                }
                else if (demoScript.sevensSeats.Length < 3)
                {
                    if (demoScript.sevensSeats.Length == 2)
                        demoScript.sevensSeats = new SevensEngine.PlayerType[3] { demoScript.sevensSeats[0], demoScript.sevensSeats[1], SevensEngine.PlayerType.AI };
                    else if (demoScript.sevensSeats.Length == 1)
                        demoScript.sevensSeats = new SevensEngine.PlayerType[3] { demoScript.sevensSeats[0], SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI };
                    else if (demoScript.sevensSeats.Length == 0)
                        demoScript.sevensSeats = new SevensEngine.PlayerType[3] { SevensEngine.PlayerType.local, SevensEngine.PlayerType.AI, SevensEngine.PlayerType.AI };
                }

                if (demoScript.ninetyNineSeats.Length > 6)
                {
                    demoScript.ninetyNineSeats = new NinetyNineEngine.PlayerType[6] { demoScript.ninetyNineSeats[0], demoScript.ninetyNineSeats[1], demoScript.ninetyNineSeats[2], demoScript.ninetyNineSeats[3], demoScript.ninetyNineSeats[4], demoScript.ninetyNineSeats[5] };
                }
                else if (demoScript.ninetyNineSeats.Length < 2)
                {
                    if (demoScript.ninetyNineSeats.Length == 1)
                        demoScript.ninetyNineSeats = new NinetyNineEngine.PlayerType[2] { demoScript.ninetyNineSeats[0], NinetyNineEngine.PlayerType.AI };
                    else if (demoScript.ninetyNineSeats.Length == 0)
                        demoScript.ninetyNineSeats = new NinetyNineEngine.PlayerType[2] { NinetyNineEngine.PlayerType.local, NinetyNineEngine.PlayerType.AI };
                }

                if (demoScript.GOPSSeats.Length > 6)
                {
                    demoScript.GOPSSeats = new GOPSEngine.PlayerType[6] { demoScript.GOPSSeats[0], demoScript.GOPSSeats[1], demoScript.GOPSSeats[2], demoScript.GOPSSeats[3], demoScript.GOPSSeats[4], demoScript.GOPSSeats[5] };
                }
                else if (demoScript.GOPSSeats.Length < 2)
                {
                    if (demoScript.GOPSSeats.Length == 1)
                        demoScript.GOPSSeats = new GOPSEngine.PlayerType[2] { demoScript.GOPSSeats[0], GOPSEngine.PlayerType.AI };
                    else if (demoScript.GOPSSeats.Length == 0)
                        demoScript.GOPSSeats = new GOPSEngine.PlayerType[2] { GOPSEngine.PlayerType.local, GOPSEngine.PlayerType.AI };
                }

                if (demoScript.BlackjackSeats.Length > 6)
                {
                    demoScript.BlackjackSeats = new BlackjackEngine.PlayerType[6] { demoScript.BlackjackSeats[0], demoScript.BlackjackSeats[1], demoScript.BlackjackSeats[2], demoScript.BlackjackSeats[3], demoScript.BlackjackSeats[4], demoScript.BlackjackSeats[5] };
                }
                else if (demoScript.BlackjackSeats.Length < 1)
                {
                    demoScript.BlackjackSeats = new BlackjackEngine.PlayerType[1] { BlackjackEngine.PlayerType.local};
                }

                if (demoScript.TopTrumpsSeats.Length > 6)
                {
                    demoScript.TopTrumpsSeats = new TopTrumpsEngine.PlayerType[6] { demoScript.TopTrumpsSeats[0], demoScript.TopTrumpsSeats[1], demoScript.TopTrumpsSeats[2], demoScript.TopTrumpsSeats[3], demoScript.TopTrumpsSeats[4], demoScript.TopTrumpsSeats[5] };
                }
                else if (demoScript.TopTrumpsSeats.Length < 2)
                {
                    if (demoScript.TopTrumpsSeats.Length == 1)
                        demoScript.TopTrumpsSeats = new TopTrumpsEngine.PlayerType[2] { demoScript.TopTrumpsSeats[0], TopTrumpsEngine.PlayerType.AI };
                    else if (demoScript.TopTrumpsSeats.Length == 0)
                        demoScript.TopTrumpsSeats = new TopTrumpsEngine.PlayerType[2] { TopTrumpsEngine.PlayerType.local, TopTrumpsEngine.PlayerType.AI };
                }
            }
        }
    }
}
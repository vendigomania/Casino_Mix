using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CasinoMixGame : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Text coinsText;

    [SerializeField] private GameObject rouletteGame;
    [SerializeField] private GameObject slotsGame;
    [SerializeField] private GameObject cardsGame;

    public static int Coins
    {
        get => Mathf.Max(PlayerPrefs.GetInt("Coins", 1000), 10);
        set => PlayerPrefs.SetInt("Coins", value);
    }

    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        coinsText.text = Coins.ToString();
    }

    public void Home() => ShowScreen(startScreen);

    public void Roulette() => ShowScreen(rouletteGame);

    public void Slots() => ShowScreen(slotsGame);

    public void Cards() => ShowScreen(cardsGame);

    private void ShowScreen(GameObject screen)
    {
        startScreen.SetActive(screen == startScreen);

        rouletteGame.SetActive(screen == rouletteGame);
        slotsGame.SetActive(screen == slotsGame);
        cardsGame.SetActive(screen == cardsGame);

        coinsText.text = Coins.ToString();

        SoundManager.Instance.PlayClick();
    }
}

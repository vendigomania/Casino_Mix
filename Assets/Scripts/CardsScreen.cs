using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardsScreen : MonoBehaviour
{
    [SerializeField] private Text coinsText;
    [SerializeField] private GameObject winScreen;

    private void Start()
    {
        CGAIE.SevensDemo.OnFinish += Finish;
    }

    private void OnEnable()
    {
        coinsText.text = CasinoMixGame.Coins.ToString();
    }

    private void Finish(bool win)
    {
        Debug.Log("Finish");
        if (win)
        {
            CasinoMixGame.Coins += 50;
            coinsText.text = CasinoMixGame.Coins.ToString();
            winScreen.SetActive(true);
        }
    }
}

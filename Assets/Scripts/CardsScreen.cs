using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsScreen : MonoBehaviour
{
    [SerializeField] private Text coinsText;

    private void OnEnable()
    {
        coinsText.text = CasinoMixGame.Coins.ToString();
    }
}

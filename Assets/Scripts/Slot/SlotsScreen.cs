using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotsScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Text coinsText;

    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private Text betLable;
    [SerializeField] private Text totalBetLable;
    [SerializeField] private Text winLable;

    int bet = 10;
    int totalBet;
    int win;

    private void Start()
    {
        slotMachine.OnRollEnd += OnRollEnd;
    }

    private void OnEnable()
    {
        UpdateUI();
        slotMachine.CancelRoll();
    }

    public void ChangeBet(int addValue)
    {
        bet = Mathf.Clamp(bet + addValue, 10, CasinoMixGame.Coins);

        UpdateUI();

        SoundManager.Instance.PlayClick();
    }

    public void Spin()
    {
        CasinoMixGame.Coins -= bet;
        totalBet += bet;

        UpdateUI();
        slotMachine.Roll();
        canvasGroup.interactable = false;

        SoundManager.Instance.PlayClick();
    }

    private void OnRollEnd(int prizeMultiplier)
    {
        var prize = bet * prizeMultiplier;

        CasinoMixGame.Coins += prize;
        win += prize;

        UpdateUI();

        canvasGroup.interactable = true;
        SoundManager.Instance.PlayWin();
    }

    private void UpdateUI()
    {
        betLable.text = bet.ToString();
        totalBetLable.text = totalBet.ToString();
        winLable.text = win.ToString();
        coinsText.text = CasinoMixGame.Coins.ToString();
    }
}

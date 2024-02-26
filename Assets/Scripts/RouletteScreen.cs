using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Text coinsText;

    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private Text[] roulettePrizLables;
    [SerializeField] private int[] prizesValues;

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameObject winTitle;
    [SerializeField] private GameObject loseTitle;
    [SerializeField] private Text resultText;

    float zVelocity;
    int mode = 0;

    private void Start()
    {
        for(var i = 0; i < prizesValues.Length; i++)
            roulettePrizLables[i].text = prizesValues[i].ToString();
    }

    private void OnEnable()
    {
        coinsText.text = CasinoMixGame.Coins.ToString();
        wheelTransform.rotation = Quaternion.identity;
        mode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (mode > 0)
        {
            wheelTransform.Rotate(Vector3.back * zVelocity * Time.deltaTime * 20f);

            if(mode == 2)
            {
                zVelocity -= Time.deltaTime * 5f;

                if (zVelocity <= 0f)
                {
                    mode = 0;
                    canvasGroup.interactable = true;

                    var resultId = Mathf.FloorToInt(360f - wheelTransform.rotation.eulerAngles.z - 22.5f) / 45;

                    CasinoMixGame.Coins += prizesValues[resultId];
                    coinsText.text = CasinoMixGame.Coins.ToString();

                    resultScreen.SetActive(true);
                    resultText.text = prizesValues[resultId].ToString();
                    winTitle.SetActive(prizesValues[resultId] > 0);
                    loseTitle.SetActive(prizesValues[resultId] < 0);

                    if(prizesValues[resultId] > 0)
                    {
                        SoundManager.Instance.PlayWin();
                    }
                    else
                    {
                        SoundManager.Instance.PlayLose();
                    }
                }
            }
        }
    }

    public void Spin()
    {
        mode = 1;
        zVelocity = 20f;

        SoundManager.Instance.PlayClick();
    }

    public void Stop()
    {
        if (mode == 0) return;

        canvasGroup.interactable = false;
        mode = 2;

        SoundManager.Instance.PlayClick();
    }

    public void Next()
    {
        resultScreen.SetActive(false);

        SoundManager.Instance.PlayClick();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SlotLine : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private GameObject staticPart;
    [SerializeField] private GameObject dynamicPart;

    [SerializeField] private Image[] slotImages;

    private Action onResponse;

    public int CurrentNum { get; private set; }

    public void Roll(float delay, Action callback)
    {
        onResponse = callback;

        dynamicPart.SetActive(true);
        staticPart.SetActive(false);

        CurrentNum = UnityEngine.Random.Range(0, sprites.Length);

        for (var i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].sprite = sprites[(CurrentNum + i - 1 + sprites.Length) % sprites.Length];
        }

        Invoke("Response", delay);
    }

    void Response()
    {
        dynamicPart.SetActive(false);
        staticPart.SetActive(true);
        onResponse?.Invoke();
    }

    public void CancelRoll()
    {
        CancelInvoke();
        dynamicPart.SetActive(false);
        staticPart.SetActive(true);
    }
}

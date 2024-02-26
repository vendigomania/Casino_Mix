using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SlotMachine : MonoBehaviour
{
    [SerializeField] private SlotLine[] lines;

    public Action OnRollStart;
    public Action<int> OnRollEnd;

    int[] currentNums;

    [HideInInspector] public bool IsReady = true;
    [HideInInspector] public bool Auto = false;

    private void Start()
    {
        currentNums = new int[lines.Length];
    }

    public void Roll()
    {
        if (!IsReady) return;

        IsReady = false;
        OnRollStart?.Invoke();

        for (var i = 0; i < lines.Length - 1; i++)
        {
            lines[i].Roll((i + 1) * 0.5f, null);
        }
        lines[4].Roll(4, Check);
    }

    private void Check()
    {
        var result = new int[8];
        foreach (var line in lines)
        {
            result[line.CurrentNum]++;
            result[(line.CurrentNum + 8 - 1) % 8]++;
            result[(line.CurrentNum + 8 + 1) % 8]++;
        }

        var prize = result.Max() >= 4 ? 4 : 0;

        prize = result[0] >= 4 ? result[0] * 4 : prize; //joker

        Debug.Log("Max combo " + result.Max());
        Debug.Log("Seven " + result[7]);

        OnRollEnd?.Invoke(prize);

        IsReady = true;
        if (Auto) Invoke("Roll", 1f);
    }
    public void CancelRoll()
    {
        CancelInvoke();
        Auto = false;
        foreach (var line in lines) line.CancelRoll();
    }
}
    

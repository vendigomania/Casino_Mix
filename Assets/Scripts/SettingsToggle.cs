using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField] private GameObject on;
    [SerializeField] private GameObject off;

    public void SetFlagsActive(bool isOn)
    {
        on.SetActive(isOn);
        off.SetActive(!isOn);
    }
}

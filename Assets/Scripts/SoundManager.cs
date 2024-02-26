using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource win;
    [SerializeField] private AudioSource lose;
    [SerializeField] private AudioSource click;

    public static SoundManager Instance;

    private void Start()
    {
        Instance = this;
    }

    public void PlayWin()
    {
        if (enabled) win.Play();
    }

    public void PlayLose()
    {
        if (enabled) lose.Play();
    }

    public void PlayClick()
    {
        if (enabled) click.Play();
    }
}

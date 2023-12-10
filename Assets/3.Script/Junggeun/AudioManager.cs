using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip Win;

    private void Awake()
    {
        TryGetComponent(out audioSource);
    }

    public void WinSound()
    {
        audioSource.PlayOneShot(Win);
    }


}

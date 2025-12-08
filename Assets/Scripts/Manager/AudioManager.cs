using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioType
    {
        ClearCell,
        CardEffect,
        CardUse,
        Gold,
        ShopDrop,
    }
    public static AudioManager Instance;
    public AudioClip clearCell;
    public AudioClip cardEffect;
    public AudioClip cardUse;
    public AudioClip gold;
    public AudioClip shopDrop;
    public AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaySound(AudioType type)
    {
        switch (type)
        {
            case AudioType.ClearCell:
                audioSource.PlayOneShot(clearCell);
                break;
            case AudioType.CardEffect:
                audioSource.PlayOneShot(cardEffect);
                break;
            case AudioType.CardUse:
                audioSource.PlayOneShot(cardUse);
                break;
            case AudioType.Gold:
                audioSource.PlayOneShot(gold);
                break;
            case AudioType.ShopDrop:
                audioSource.PlayOneShot(shopDrop);
                break;
        }
    }
    public void PlayClearSound()
    {
        audioSource.clip = clearCell;
        audioSource.Play();
    }
}

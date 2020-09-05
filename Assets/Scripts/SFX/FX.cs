using UnityEngine;
using System.Collections;

public class FX : MonoBehaviour
{
    public AudioSource sound;
    public float timeToDestroy = 1;

    void Start()
    {
        if (Game.sound.ON && sound != null) sound.Play();
        Destroy(gameObject, timeToDestroy);
    }
}

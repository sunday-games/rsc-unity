using UnityEngine;
using System.Collections;
using DG.Tweening;

public class SelfDestroy : MonoBehaviour
{
    public bool scaleBefore = false;
    public float timeToDestroy = 1f;

    void Start()
    {
        Invoke("GO", timeToDestroy);
    }

    void GO()
    {
        if (gameObject != null)
        {
            if (scaleBefore) transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => Destroy(gameObject));
            else Destroy(gameObject);
        }
    }
}

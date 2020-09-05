using UnityEngine;
using System.Collections;

public class ScaleInOutLoop : MonoBehaviour
{
    public float scale = 0.3f;
    public float time = 0.7f;

    void OnEnable()
    {
        transform.localScale = Vector3.one;
        iTween.ScaleAdd(gameObject, iTween.Hash(
            "x", scale, "y", scale, "easeType", "easeInOutQuad", "loopType", "pingPong", "time", time));
    }

    void OnDisable()
    {
        iTween.Stop(gameObject);
    }
}

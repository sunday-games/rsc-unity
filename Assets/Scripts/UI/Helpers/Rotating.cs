using UnityEngine;
using System.Collections;

public class Rotating : MonoBehaviour
{
    public Vector3 rotation;
    Transform _transform;

    void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        _transform.Rotate(rotation * Time.deltaTime);
    }

    void OnBecameVisible()
    {
        enabled = true;
    }
    void OnBecameInvisible()
    {
        enabled = false;
    }
}
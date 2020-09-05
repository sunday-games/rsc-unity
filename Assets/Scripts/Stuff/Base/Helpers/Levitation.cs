using UnityEngine;
using System.Collections;

public class Levitation : MonoBehaviour
{
    public GameObject levitationObject;
    public float delta = 0.2f;
    public float speed = 1.5f;

    void Start()
    {
        iTween.MoveAdd(levitationObject, iTween.Hash("y", delta, "easeType", "easeInOutQuad", "loopType", "pingPong", "time", speed));
    }
}

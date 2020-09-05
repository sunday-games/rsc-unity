using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FireworkRocket : Firework
{
    public Transform fxParent;
    public GameObject burningFuse;
    public float radius;
    public float speed;

    public void RotateToCenter(Vector3 position)
    {
        if (position.x > 0 && position.y > 0) t.Rotate(0, 0, Random.Range(110, 160));
        else if (position.x < 0 && position.y > 0) t.Rotate(0, 0, Random.Range(200, 250));
        else if (position.x > 0 && position.y < 0) t.Rotate(0, 0, Random.Range(20, 70));
        else if (position.x < 0 && position.y < 0) t.Rotate(0, 0, Random.Range(290, 340));
    }

    public override void Boom()
    {
        Destroy(burningFuse);
        if (activateFX != null)
        {
            var fx = Instantiate(activateFX, fxParent.position, fxParent.rotation) as GameObject;
            fx.transform.SetParent(t, true);
            fx.layer = 11;
        }

        Destroy(rb);
        shape.enabled = false;
        t.SetParent(ui.game.stuffFrontFront, true);

        StartCoroutine(Fly());
        StartCoroutine(PushOutCatsOnPath());
    }

    IEnumerator Fly()
    {
        while (true)
        {
            t.position += t.up * speed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator PushOutCatsOnPath()
    {
        List<Stuff> stuffToPushOut = new List<Stuff>();
        int pushedStuff = 0;
        int pumpkins = 0;

        if (smallScreen) radius *= 1.1f;
        float radiusSquare = radius * radius;

        while (t != null && 6.5f > t.position.y && t.position.y > -6.5f && 5f > t.position.x && t.position.x > -5f)
        {
            stuffToPushOut.Clear();
            foreach (Stuff stuff in Factory.LIVE_STUFF)
                if (stuff != null && DistanceTo(stuff) < radiusSquare) stuffToPushOut.Add(stuff);

            pushedStuff += stuffToPushOut.Count;

            foreach (Stuff stuff in stuffToPushOut) Factory.LIVE_STUFF.Remove(stuff);

            foreach (Stuff stuff in stuffToPushOut)
            {
                if (stuff is Pumpkin) ++pumpkins;

                stuff.Activate(t.anchoredPosition);
            }

            yield return new WaitForEndOfFrame();
        }

        gameplay.GetScores(t.anchoredPosition, countCats: pushedStuff - pumpkins, countPumpkins: pumpkins);

        Destroy(gameObject, 2.5f);
    }
}

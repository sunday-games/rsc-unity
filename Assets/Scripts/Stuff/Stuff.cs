using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Stuff : Core
{
    public Image image;
    public Collider2D shape = null;
    public AudioSource onClickSound;
    public AudioClip audioClip;

    [HideInInspector]
    public RectTransform t = null;

    [HideInInspector]
    public Rigidbody2D rb = null;

    void Awake()
    {
        Factory.STUFF.Add(this);
        Factory.LIVE_STUFF.Add(this);
        t = transform as RectTransform;
        rb = GetComponent<Rigidbody2D>();
    }

    void OnDestroy()
    {
        if (Factory.STUFF != null) Factory.STUFF.Remove(this);
        if (Factory.LIVE_STUFF != null) Factory.LIVE_STUFF.Remove(this);
    }

    public virtual void Setup()
    {
    }

    [HideInInspector]
    public bool isActivated = false;
    public virtual void Activate(Vector2 sourse)
    {
    }

    public void ShakeAndDestroy()
    {
        Destroy(rb);
        iTween.ScaleTo(gameObject, iTween.Hash("x", 0, "y", 0, "easeType", "easeInBack", "time", 0.6f));
        iTween.ShakePosition(gameObject, UI.tenthVector3, 0.6f);
        Destroy(gameObject, 1f);
    }

    protected void Destroy()
    {
        Destroy(gameObject);
    }

    public void Bomb(float radius)
    {
        float power = radius * (smallScreen ? 1.1f : 1f);
        power *= power;

        var stuffToBoom = new HashSet<Stuff>();
        foreach (var stuff in Factory.LIVE_STUFF)
            if (stuff != null && DistanceTo(stuff) < power) stuffToBoom.Add(stuff);

        foreach (var stuff in stuffToBoom) Factory.LIVE_STUFF.Remove(stuff);

        int pumpkins = 0;
        foreach (var stuff in stuffToBoom)
        {
            if (stuff is Pumpkin) ++pumpkins;

            stuff.Activate(t.anchoredPosition);
        }

        gameplay.GetScores(t.anchoredPosition, countCats: stuffToBoom.Count - pumpkins, countPumpkins: pumpkins);
    }

    public float DistanceTo(Stuff target)
    {
        return (t.position - target.t.position).sqrMagnitude;
    }
    public float DistanceTo(Vector3 target)
    {
        return (t.position - target).sqrMagnitude;
    }

    public void Punch(float time = 1f)
    {
        iTween.PunchScale(gameObject, halfVector3, time);
        //t.DOPunchScale(punchScale, time);
    }
    public void Punch(Vector3 scale, float time = 1f)
    {
        iTween.PunchScale(gameObject, scale, time);
        //t.DOPunchScale(scale, time);
    }

    public IEnumerator MoveTo(Transform target, float minDistance, System.Action<Stuff> callback)
    {
        shape.enabled = false;
        rb.gravityScale = 0;

        float timer = 0f;
        float speed = 0.4f * Random.Range(0.9f, 1.1f);
        while (target != null && DistanceTo(target.position) > minDistance)
        {
            timer += speed * Time.deltaTime;

            t.position = Vector3.Lerp(t.position, target.position, timer);

            yield return false;
        }

        shape.enabled = true;
        rb.gravityScale = 1.2f;

        callback(this);
    }

    public IEnumerator MoveTo(Vector3 position, float minDistance, System.Action<Stuff> callback)
    {
        shape.enabled = false;
        rb.gravityScale = 0;

        float timer = 0f;
        float speed = 0.4f * Random.Range(0.9f, 1.1f);
        while (DistanceTo(position) > minDistance)
        {
            timer += speed * Time.deltaTime;

            t.position = Vector3.Lerp(t.position, position, timer);

            yield return false;
        }

        shape.enabled = true;
        rb.gravityScale = 1.2f;

        callback(this);
    }

    public IEnumerator MoveToBlackHole(Transform target)
    {
        isActivated = true;

        if (shape != null) shape.enabled = false;
        if (rb != null) rb.gravityScale = 0;
        StopAllCoroutines();

        t.SetParent(ui.game.stuffFrontFront, false);

        t.DOMove(target.position, 0.5f);
        t.DOScale(Vector3.zero, 0.5f);
        yield return new WaitForSeconds(0.5f);

        if (this is CatBasic && !(this is CatJoker) && gameplay.isPlaying)
        {
            factory.CreateCatRandomBasic();
            (this as CatBasic).Reset();
        }
        else if (this is CatBasicRiki && !(this is CatJokerRiki) && gameplay.isPlaying)
        {
            factory.CreateCatRandomBasic();
            (this as CatBasicRiki).Reset();
        }
        else if (this is Pumpkin && gameplay.isPlaying)
        {
            factory.CreateCatRandomBasic();
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

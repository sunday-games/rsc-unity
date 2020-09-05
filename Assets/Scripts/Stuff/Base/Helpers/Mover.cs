using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Mover : Core
{
    public Image image;
    public float speed = 10;
    public ParticleSystem ps;

    public Image multiplierImage;
    public Text multiplierText;

    Transform t;

    // Летит рыбка к коту
    public static Mover CreateCoinForCat(Vector3 from, CatBasic cat)
    {
        Mover mover = Instantiate(ui.game.coinPrefab) as Mover;
        mover.t = mover.transform;
        mover.t.SetParent(ui.canvas[3].transform, false);
        mover.t.position = from;
        mover.t.Rotate(0f, 0f, Random.Range(0f, 360f));
        mover.speed *= Random.Range(0.9f, 1.1f);

        mover.StartCoroutine(mover.Fly(cat));

        return mover;
    }
    IEnumerator Fly(CatBasic cat)
    {
        Vector3 gravity = new Vector3(-0.4f, 0.4f, 0f);

        while (cat != null && t != null && cat.gameObject.activeSelf && !cat.isActivated && cat.DistanceTo(t.position) > 0.25f)
        {
            t.position += speed * (gravity + (cat.t.position - t.position).normalized) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (cat == null || t == null || !cat.gameObject.activeSelf)
        {
        }
        else if (cat.isActivated)
        {
            Create(ui.game.coinPrefab, ui.canvas[3].transform, t.position, gameplay.level.coinParent, 0.4f, target =>
            {
                gameplay.GetCoin();
            });
        }
        else if (cat.DistanceTo(t.position) < 0.25f)
        {
            cat.SetCoin();
            cat.Punch();
        }

        Destroy(gameObject);
    }

    // Летит что то куда то с колбеком
    public static Mover Create(Mover prefab, Transform parent, Vector3 from, MonoBehaviour target, float deviation, System.Action<MonoBehaviour> callback)
    {
        Mover mover = Instantiate(prefab) as Mover;
        mover.t = mover.transform;
        mover.t.SetParent(parent, false);
        mover.t.position = from;
        mover.t.Rotate(0, 0, Random.Range(0, 360));
        mover.speed *= Random.Range(0.9f, 1.1f);

        mover.StartCoroutine(mover.Fly(target, deviation, callback));

        return mover;
    }
    IEnumerator Fly(MonoBehaviour target, float deviation, System.Action<MonoBehaviour> callback)
    {
        Transform tTarget = target.transform;
        Vector3 gravity = new Vector3(-deviation, deviation, 0);

        while (target != null && target.gameObject.activeSelf && t != null && (t.position - tTarget.position).sqrMagnitude > 0.25f)
        {
            t.position += speed * (gravity + (tTarget.position - t.position).normalized) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (callback != null && target != null && target.gameObject.activeSelf)
            callback(target);

        if (ps != null)
        {
            ps.Stop();
            Destroy(gameObject, 1f);
        }
        else
            Destroy(gameObject);
    }

    // Летит что то куда то (рыбки в меню, например)
    public static Mover Create(Mover prefab, Transform parent, Vector3 from, Transform to, float deviation, float scale = 1f)
    {
        Mover mover = Instantiate(prefab) as Mover;
        mover.t = mover.transform;
        mover.t.SetParent(parent, false);
        mover.t.position = from;
        mover.t.Rotate(0, 0, Random.Range(0, 360));
        mover.speed *= Random.Range(0.9f, 1.1f);

        if (scale != 1f)
            mover.t.localScale = Vector3.one * scale;

        mover.StartCoroutine(mover.Fly(to, deviation));

        return mover;
    }
    public IEnumerator Fly(Transform target, float deviation)
    {
        Vector3 gravity = new Vector3(-deviation, deviation, 0);

        while ((t.position - target.position).sqrMagnitude > 0.25f)
        {
            t.position += speed * (gravity + (target.position - t.position).normalized) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
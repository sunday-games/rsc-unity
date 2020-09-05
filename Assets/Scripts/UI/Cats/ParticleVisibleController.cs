using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParticleVisibleController : Core
{
    public ParticleSystem[] particleSystems;

    RectTransform parent;
    RectTransform t;
    void Awake()
    {
        parent = ui.prepare.catItemsBackImage.transform as RectTransform;
        t = transform as RectTransform;
    }

    void Update()
    {
        if ((parent.position - t.position).sqrMagnitude > 14f)
            foreach (var ps in particleSystems) { if (!ps.isStopped) ps.Stop(); }
        else
            foreach (var ps in particleSystems) { if (!ps.isPlaying) ps.Play(); }
    }

    public void ON(bool watch)
    {
        if (watch)
        {
            enabled = true;
            if ((parent.position - t.position).sqrMagnitude > 14f)
                foreach (var ps in particleSystems) ps.Stop();
            else
                foreach (var ps in particleSystems) ps.Play();
        }
        else
            foreach (var ps in particleSystems) ps.Play();
    }
    public void OFF()
    {
        enabled = false;
        foreach (var ps in particleSystems) ps.Stop();
    }
}

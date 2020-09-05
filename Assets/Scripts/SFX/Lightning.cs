using UnityEngine;
using System.Collections;

public class Lightning : Core
{
    public ParticleSystem lightningBeam;

    public static void Create(Stuff target, Stuff source)
    {
        var lightning = Instantiate(factory.lightningPrefab, target.t.position, Quaternion.identity) as Lightning;
        lightning.transform.LookAt(source.t.position);
        lightning.lightningBeam.startSize = 0.075f * (source.t.position - target.t.position).magnitude;
        // source.DistanceTo(target)
    }
}

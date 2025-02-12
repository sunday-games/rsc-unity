using UnityEngine;
using System.Collections.Generic;

namespace SG
{
    [RequireComponent(typeof(Renderer))]
    public class InvisibleDisabler : MonoBehaviour
    {
        public List<MonoBehaviour> targets = new List<MonoBehaviour>();

        private void OnBecameVisible()
        {
            for (int i = 0; i < targets.Count; ++i)
                targets[i].enabled = true;
        }
        private void OnBecameInvisible()
        {
            for (int i = 0; i < targets.Count; ++i)
                targets[i].enabled = false;
        }
    }
}
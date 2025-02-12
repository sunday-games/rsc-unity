using UnityEngine;

namespace SG
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 rotation;
        public GameObject mainRenderer; // Most have Renderer

        public void SetRotation(Vector3 rotation, GameObject mainRenderer = null)
        {
            this.rotation = rotation;
            RotateManager.SetSpeed(transform, rotation);

            SetupInvisibleDisabler(mainRenderer);
        }

        private void Awake()
        {
            SetupInvisibleDisabler(mainRenderer);
        }

        private void SetupInvisibleDisabler(GameObject mainRenderer)
        {
            this.mainRenderer = mainRenderer;
            if (mainRenderer)
                mainRenderer.AddComponent<InvisibleDisabler>().targets.Add(this);
        }

        private void OnEnable() => RotateManager.Add(transform, rotation);
        private void OnDisable() => RotateManager.Remove(transform);

        private void OnDestroy() => RotateManager.Remove(transform);
    }
}
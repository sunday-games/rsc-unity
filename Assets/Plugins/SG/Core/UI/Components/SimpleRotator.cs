using UnityEngine;

namespace SG
{
    public class SimpleRotator : MonoBehaviour
    {
        public Vector3 speed;
        public Transform target;

        private void Update() => target.Rotate(speed * Time.deltaTime);
    }
}
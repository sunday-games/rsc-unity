using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    public class RotateManager : MonoBehaviour
    {
        static Dictionary<Transform, Vector3> rotators = new Dictionary<Transform, Vector3>();

        public static void Add(Transform transform, Vector3 speed)
        {
            if (!rotators.ContainsKey(transform))
                rotators.Add(transform, speed);
        }

        public static void Remove(Transform transform)
        {
            rotators.Remove(transform);
        }

        public static void SetSpeed(Transform transform, Vector3 speed)
        {
            rotators[transform] = speed;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            foreach (var pair in rotators)
                if (pair.Key != null)
                    pair.Key.Rotate(pair.Value * deltaTime);
        }
    }
}
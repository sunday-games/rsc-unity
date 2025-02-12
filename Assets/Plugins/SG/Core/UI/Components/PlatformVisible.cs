using UnityEngine;

namespace SG.UI
{
    public class PlatformVisible : MonoBehaviour
    {
        public RuntimePlatform[] platforms;

        void Start()
        {
            foreach (var platform in platforms)
                if (platform == Application.platform)
                    return;

            gameObject.SetActive(false);
        }
    }
}
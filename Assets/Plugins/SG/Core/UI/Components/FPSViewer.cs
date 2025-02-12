using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace SG.UI
{
    public class FPSViewer : MonoBehaviour
    {
        public Text FpsText;

        private float[] _fpsBuffer;
        private int _fpsBufferIndex;
        private int _fpsBufferCount;

        void Start()
        {
            _fpsBuffer = new float[Mathf.CeilToInt(3f / Time.fixedDeltaTime)];
            _fpsBufferIndex = 0;
            _fpsBufferCount = 0;
        }

        void Update()
        {
            float fps = 1.0f / Time.deltaTime;

            _fpsBuffer[_fpsBufferIndex] = fps;
            _fpsBufferIndex = (_fpsBufferIndex + 1) % _fpsBuffer.Length;

            if (_fpsBufferCount < _fpsBuffer.Length)
                _fpsBufferCount++;

            float averageFPS = 0.0f;
            for (int i = 0; i < _fpsBufferCount; i++)
                averageFPS += _fpsBuffer[i];

            averageFPS /= _fpsBufferCount;

            FpsText.text = Mathf.RoundToInt(averageFPS).ToString();
        }
    }
}
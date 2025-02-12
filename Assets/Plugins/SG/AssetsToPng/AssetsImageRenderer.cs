using UnityEngine;
using System;

namespace SG.AssetsToPng
{
    public class AssetsImageRenderer : MonoBehaviour
    {
        public Vector2 textureSize = new Vector2(512, 512);
        public Color backgroundColor = Color.black;
        public ThreeDimensional.UIObject3D uiObject3D_prefab;

        public void Render(Transform obj, Action<Texture2D> callback)
        {
            var uiObject3D = Instantiate(uiObject3D_prefab);
            uiObject3D.TextureSize = textureSize;
            uiObject3D.BackgroundColor = backgroundColor;
            uiObject3D.ObjectPrefab = obj;
            uiObject3D.onRenderFinished +=
                texture =>
                {
                    Destroy(uiObject3D.gameObject);

                    callback?.Invoke(texture);
                };

            uiObject3D.HardUpdateDisplay();
        }
    }
}
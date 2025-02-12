using UnityEngine;

namespace SG.UI
{
    public class CopyButton : MonoBehaviour
    {
        private TMPro.TMP_InputField _inputField;

        private void Awake()
        {
            _inputField = transform.parent.GetComponent<TMPro.TMP_InputField>();

            if (_inputField == null)
            {
                Log.Error("CopyButton - Can't find InputField component in parent object");
                gameObject.SetActive(false);
            }
        }

        public void Copy()
        {
            _inputField.onSelect?.Invoke(_inputField.text);
            ClipboardPlugin.Clipboard.Copy(_inputField.text);
        }
    }
}
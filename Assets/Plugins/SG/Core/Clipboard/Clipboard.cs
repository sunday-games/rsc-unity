using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace SG.ClipboardPlugin
{
    public class Clipboard : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameObject selected => UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        // TODO
        //void Update()
        //{
        //    if (!IsPress.copy) return;

        //    if (selected == null) return;

        //    var inputField_TMP = selected.GetComponent<TMPro.TMP_InputField>();
        //    if (inputField_TMP)
        //    {
        //        Copy(inputField_TMP.text);
        //        return;
        //    }

        //    var inputField = selected.GetComponent<TMPro.TMP_InputField>();
        //    if (inputField)
        //    {
        //        Copy(inputField.text);
        //        return;
        //    }
        //}

        public static void Copy(string text)
        {
            Log.Info("Clipboard - Copied: " + text);
            CopyToClipboard(text);
        }

        [DllImport("__Internal")] static extern void CopyToClipboard(string text);


        public static Action<string> onPaste;

        public void NotifyPaste(string text)
        {
            Log.Info("Clipboard - Pasted: " + text);

            onPaste?.Invoke(text);

            if (selected == null) return;

            var inputField_TMP = selected.GetComponent<TMPro.TMP_InputField>();
            if (inputField_TMP)
            {
                (string result, int caretPosition) = CalcPaste(inputField_TMP.text, text, inputField_TMP.selectionAnchorPosition, inputField_TMP.selectionFocusPosition);
                inputField_TMP.text = result;
                inputField_TMP.caretPosition = caretPosition;
                return;
            }

            var inputField = selected.GetComponent<TMPro.TMP_InputField>();
            if (inputField)
            {
                (string result, int caretPosition) = CalcPaste(inputField.text, text, inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
                inputField.text = result;
                inputField.caretPosition = caretPosition;
                return;
            }
        }

        (string result, int caretPosition) CalcPaste(string source, string paste, int anchorPosition, int focusPosition)
        {
            var from = Mathf.Min(anchorPosition, focusPosition);
            var to = Mathf.Max(anchorPosition, focusPosition);

            return (source.Substring(0, from) + paste + source.Substring(to, source.Length - to),
                from + paste.Length);
        }
#else
        public static void Copy(string text)
        {
            Log.Info("Clipboard - Copied: " + text);
            GUIUtility.systemCopyBuffer = text;
        }
#endif
    }
}
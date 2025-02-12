using System;
using UnityEngine;

namespace SG.UI
{
    /// <summary>
    /// This attribute can only be applied to fields because its associated PropertyDrawer only operates on fields
    /// (either public or tagged with the [SerializeField] attribute) in the target MonoBehaviour
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string MethodName;
        public ButtonAttribute(string methodName) { this.MethodName = methodName; }
    }
}
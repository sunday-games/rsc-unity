using UnityEngine;
using System;

namespace SG.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        public string conditionalSourceField;
        public bool hideInInspector = false;

        public ConditionalHideAttribute(string conditionalSourceField)
        {
            this.conditionalSourceField = conditionalSourceField;
            this.hideInInspector = false;
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
        {
            this.conditionalSourceField = conditionalSourceField;
            this.hideInInspector = hideInInspector;
        }
    }
}
using System;

namespace SG.UI
{
    public abstract class ConditionScreen : Screen
    {
        public abstract Func<bool> CanBeOpened { get; }

        protected virtual void Update()
        {
            if (!CanBeOpened.Invoke())
                Hide();
        }
    }
}
using UnityEngine;
using System.Collections;

public class ExceptionWatcher : Core
{
    string exceptionMsg = null;

    void Start()
    {
        Application.logMessageReceived += HandleException;
    }

    void Update()
    {
        if (exceptionMsg != null)
        {
            ExceptionView.Show(exceptionMsg);
            exceptionMsg = null;
        }
    }

    void HandleException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) exceptionMsg = condition + "\n\n" + stackTrace;
    }
}

using System;
using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityBlackboardLogger : IBlackboardLogger
    {
        public void Info(string message)
        {
            Debug.Log(message);
        }

        public void Warning(string message)
        {
            Debug.LogWarning(message);
        }

        public void Error(string message, Exception exception = null)
        {
            if (exception == null)
            {
                Debug.LogError(message);
                return;
            }

            Debug.LogError($"{message}\n{exception}");
        }
    }
}

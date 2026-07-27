using System;
using UnityEngine;

namespace GearEngine.Core.Actions
{
    internal sealed class LegacyCoroutineHandle : IDisposable
    {
        public LegacyCoroutineHandle(MonoBehaviour runner, Coroutine coroutine)
        {
            this.runner = runner;
            this.coroutine = coroutine;
        }

        private MonoBehaviour runner;
        private Coroutine coroutine;

        public void Dispose()
        {
            if (runner != null && coroutine != null)
            {
                runner.StopCoroutine(coroutine);
            }

            runner = null;
            coroutine = null;
        }
    }
}

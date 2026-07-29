using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Scaffold.VisualScripting.Unity
{
    [DisallowMultipleComponent]
    public sealed class UnityCoroutineRunner : MonoBehaviour
    {
        private IBlackboardLogger logger;

        [Inject]
        public void Construct(IBlackboardLogger blackboardLogger)
        {
            logger = blackboardLogger ?? throw new ArgumentNullException(nameof(blackboardLogger));
        }

        public IDisposable Run(IEnumerator routine)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            Coroutine coroutine = StartCoroutine(RunGuarded(routine));
            return new CoroutineHandle(this, coroutine);
        }

        private IEnumerator RunGuarded(IEnumerator routine)
        {
            while (TryMoveNext(routine, out object current))
            {
                yield return current;
            }
        }

        private bool TryMoveNext(IEnumerator routine, out object current)
        {
            try
            {
                bool hasNext = routine.MoveNext();
                current = hasNext ? routine.Current : null;
                return hasNext;
            }
            catch (Exception exception)
            {
                current = null;
                logger?.Error("A Blackboard coroutine failed.", exception);
                return false;
            }
        }

        private sealed class CoroutineHandle : IDisposable
        {
            public CoroutineHandle(UnityCoroutineRunner runner, Coroutine coroutine)
            {
                this.runner = runner;
                this.coroutine = coroutine;
            }

            private UnityCoroutineRunner runner;
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
}

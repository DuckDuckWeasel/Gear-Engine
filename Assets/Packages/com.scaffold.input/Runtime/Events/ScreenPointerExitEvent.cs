using System.Collections.Generic;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.Input.Events
{
    public sealed record ScreenPointerExitEvent : ContextEvent
    {
        public List<GameObject> Results { get; }
        public GameObject TopResult { get; }

        public ScreenPointerExitEvent(List<GameObject> results, GameObject topResult)
        {
            Results = results;
            TopResult = topResult;
        }
    }
}

using System.Collections.Generic;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.Input.Events
{
    public sealed record ScreenClickedEvent : ContextEvent
    {
        public List<GameObject> Results { get; }
        public GameObject TopResult { get; }

        public ScreenClickedEvent(List<GameObject> results, GameObject topResult)
        {
            Results = results;
            TopResult = topResult;
        }
    }
}

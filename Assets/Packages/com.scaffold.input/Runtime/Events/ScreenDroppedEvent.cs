using System.Collections.Generic;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.Input.Events
{
    public sealed record ScreenDroppedEvent : ContextEvent
    {
        public List<GameObject> DropResults { get; }
        public GameObject DropTopResult { get; }
        public List<GameObject> DragResults { get; }
        public GameObject DragTopResult { get; }

        public ScreenDroppedEvent(List<GameObject> dropResults, GameObject dropTopResult, List<GameObject> dragResults, GameObject dragTopResult)
        {
            DropResults = dropResults;
            DropTopResult = dropTopResult;
            DragResults = dragResults;
            DragTopResult = dragTopResult;
        }
    }
}

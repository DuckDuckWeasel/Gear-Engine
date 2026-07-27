using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class VariableCollection
    {
        public List<object> Items => items;

        [SerializeReference] private List<object> items = new List<object>();
    }
}

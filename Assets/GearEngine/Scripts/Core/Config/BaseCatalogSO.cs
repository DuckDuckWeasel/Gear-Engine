using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.Core.Config
{
    /// <summary>
    /// Base class for ScriptableObject catalogs containing a list of items of type T.
    /// Provides consistent dictionary lookups and runtime population.
    /// </summary>
    public abstract class BaseCatalogSO<T> : SerializedScriptableObject
    {
        [SerializeField, ListDrawerSettings(Expanded = true)]
        protected T[] items = Array.Empty<T>();

        private readonly Dictionary<string, T> _byId = new Dictionary<string, T>(StringComparer.Ordinal);

        protected virtual void OnEnable()
        {
            RebuildLookup();
        }

        public virtual void SetRuntimeEntries(T[] runtimeItems)
        {
            items = runtimeItems ?? Array.Empty<T>();
            RebuildLookup();
        }

        public IReadOnlyList<T> All => items ?? Array.Empty<T>();

        /// <summary>
        /// Implement to extract the unique ID for a given item.
        /// </summary>
        protected abstract string GetId(T item);

        protected void RebuildLookup()
        {
            _byId.Clear();
            if (items == null) return;

            foreach (T item in items)
            {
                if (item == null) continue;
                string id = GetId(item);
                if (string.IsNullOrEmpty(id)) continue;
                
                _byId[id] = item;
            }
        }

        public virtual T Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return default;
            return _byId.TryGetValue(id, out T item) ? item : default;
        }

        public virtual bool TryGet(string id, out T item)
        {
            if (string.IsNullOrEmpty(id))
            {
                item = default;
                return false;
            }
            return _byId.TryGetValue(id, out item);
        }
    }
}

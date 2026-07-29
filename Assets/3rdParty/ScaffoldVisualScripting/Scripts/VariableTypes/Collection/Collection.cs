
using System;
using System.Collections;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Provides a common serializable reference point for Scaffold collections.
    /// Scaffold.GenericCollection inherits from this.
    /// </summary>
    [System.Serializable]
    public abstract class Collection : IScaffoldCollection
    {
        public abstract int Capacity { get; set; }
        public abstract int Count { get; }
        public bool IsFixedSize { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return null; } }
        public string Name { get { return GetType().Name; } }
        public object this[int index] { get { return Get(index); } set { Set(index, value); } }

        public abstract int Add(object o);

        public abstract void Add(IScaffoldCollection rhsCol);

        public abstract void AddUnique(object o);

        public abstract void AddUnique(IScaffoldCollection rhsCol);

        public abstract void Clear();

        public abstract Type ContainedType();

        public abstract bool Contains(object o);

        public abstract bool ContainsAllOf(IScaffoldCollection rhsCol);

        public abstract bool ContainsAllOfOrdered(IScaffoldCollection rhsCol);

        public abstract bool ContainsAnyOf(IScaffoldCollection rhsCol);

        public abstract void CopyFrom(IScaffoldCollection rhsCol);

        public abstract void CopyFrom(System.Array array);

        public abstract void CopyFrom(System.Collections.IList list);

        public abstract void CopyTo(Array array, int index);

        public abstract void Exclusive(IScaffoldCollection rhsCol);

        public abstract object Get(int index);

        public abstract void Get(int index, ref Variable variable);

        public abstract IEnumerator GetEnumerator();

        public abstract int IndexOf(object o);

        public abstract void Insert(int index, object o);

        public abstract void Intersection(IScaffoldCollection rhsCol);

        public abstract bool IsCollectionCompatible(object o);

        public abstract bool IsElementCompatible(object o);

        public abstract int LastIndexOf(object o);

        public abstract int Occurrences(object o);

        public abstract void Remove(object o);

        public abstract void RemoveAll(IScaffoldCollection rhsCol);

        public abstract void RemoveAll(object o);

        public abstract void RemoveAt(int index);

        public abstract void Reserve(int count);

        public abstract void Resize(int count);

        public abstract void Reverse();

        public abstract void Set(int index, object o);

        public abstract void Shuffle();

        public abstract void Sort();

        public abstract void Unique();
    }
}

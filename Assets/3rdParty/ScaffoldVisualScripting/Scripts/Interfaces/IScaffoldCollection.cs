
namespace Scaffold
{
    /// <summary>
    /// Extension of IList for Scaffold collections and support for associated commands.
    ///
    /// Built upon objects being passed in and returned as the base starting point.
    /// The inherited classes may wish to provided typed access to underlying container,
    /// this is what the Scaffold.GenericCollection does.
    /// </summary>
    public interface IScaffoldCollection : System.Collections.IList
    {
        int Capacity { get; set; }
        string Name { get; }

        void Add(IScaffoldCollection rhsCol);

        void AddUnique(object o);

        void AddUnique(IScaffoldCollection rhsCol);

        System.Type ContainedType();

        bool ContainsAllOf(IScaffoldCollection rhsCol);

        bool ContainsAllOfOrdered(IScaffoldCollection rhsCol);

        bool ContainsAnyOf(IScaffoldCollection rhsCol);

        void CopyFrom(IScaffoldCollection rhsCol);

        void CopyFrom(System.Array array);

        void CopyFrom(System.Collections.IList list);

        void Exclusive(IScaffoldCollection rhsCol);

        object Get(int index);

        void Get(int index, ref Variable variable);

        void Intersection(IScaffoldCollection rhsCol);

        bool IsCollectionCompatible(object o);

        bool IsElementCompatible(object o);

        int LastIndexOf(object o);

        int Occurrences(object o);

        void RemoveAll(IScaffoldCollection rhsCol);

        void RemoveAll(object o);

        void Reserve(int count);

        void Resize(int count);

        void Reverse();

        void Set(int index, object o);

        void Shuffle();

        void Sort();

        void Unique();
    }
}
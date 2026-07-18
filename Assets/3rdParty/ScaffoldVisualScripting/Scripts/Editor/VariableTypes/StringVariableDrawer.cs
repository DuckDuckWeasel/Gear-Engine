using UnityEditor;

namespace Scaffold.EditorUtils
{
    [CustomPropertyDrawer(typeof(StringData))]
    public class StringDataDrawer : VariableDataDrawer<StringVariable> { }
}

using UnityEditor;

namespace Scaffold.EditorUtils
{
    [CustomPropertyDrawer(typeof(IntegerData))]
    public class IntegerDataDrawer : VariableDataDrawer<IntegerVariable> { }
}

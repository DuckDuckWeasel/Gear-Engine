using UnityEditor;

namespace Scaffold.EditorUtils
{
    [CustomPropertyDrawer(typeof(BooleanData))]
    public class BooleanDataDrawer : VariableDataDrawer<BooleanVariable> { }
}

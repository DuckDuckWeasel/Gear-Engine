using UnityEditor;

namespace Scaffold.EditorUtils
{
    [CustomPropertyDrawer(typeof(CharacterData))]
    public class CharacterDataDrawer : VariableDataDrawer<CharacterVariable>
    {
    }
}

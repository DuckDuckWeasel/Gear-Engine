using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardDefinitionWindowLauncher
    {
        [MenuItem("Window/Scaffold/Blackboard")]
        public static void OpenSelected()
        {
            Open(Selection.activeObject);
        }

        public static void Open(Object source)
        {
            BlackboardDefinitionWindow window = EditorWindow.GetWindow<BlackboardDefinitionWindow>("Blackboard");
            if (source is BlackboardDefinitionAsset || source is BlackboardBehaviour)
            {
                window.SetSource(source);
            }

            window.Show();
        }
    }
}

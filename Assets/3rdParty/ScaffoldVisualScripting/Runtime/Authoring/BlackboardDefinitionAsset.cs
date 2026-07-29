using Scaffold.VisualScripting;
using UnityEngine;

namespace Scaffold.VisualScripting.Authoring
{
    [CreateAssetMenu(
        fileName = "BlackboardDefinition",
        menuName = "Scaffold/Visual Scripting/Blackboard Definition")]
    public sealed class BlackboardDefinitionAsset : ScriptableObject
    {
        public BlackboardDefinition Definition
        {
            get => definition;
            set => definition = value;
        }

        [SerializeField]
        private BlackboardDefinition definition =
            new BlackboardDefinition();

        public BlackboardAuthoringMetadata AuthoringMetadata => authoringMetadata;

        [SerializeField]
        private BlackboardAuthoringMetadata authoringMetadata =
            new BlackboardAuthoringMetadata();
    }
}

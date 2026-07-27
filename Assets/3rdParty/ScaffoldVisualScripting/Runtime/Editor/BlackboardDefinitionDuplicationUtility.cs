using System;
using Scaffold.VisualScripting.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardDefinitionDuplicationUtility
    {
        public static BlackboardDefinition CloneWithNewIds(BlackboardDefinition source)
        {
            SerializedGraphCloner cloner = new SerializedGraphCloner();
            BlackboardDefinition clone = cloner.CloneGraph(source ?? throw new ArgumentNullException(nameof(source)));
            new DefinitionIdRegenerator().Regenerate(clone);
            return clone;
        }

        public static BlackboardDefinitionAsset DuplicateAsset(BlackboardDefinitionAsset source, string destinationPath)
        {
            ValidateDestination(source, destinationPath);
            BlackboardDefinitionAsset duplicate = ScriptableObject.CreateInstance<BlackboardDefinitionAsset>();
            duplicate.Definition = CloneWithNewIds(source.Definition);
            CopyMetadata(source, duplicate);
            AssetDatabase.CreateAsset(duplicate, destinationPath);
            AssetDatabase.SaveAssets();
            return duplicate;
        }

        private static void ValidateDestination(BlackboardDefinitionAsset source, string destinationPath)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(destinationPath) || !destinationPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The duplicate destination must be a project-relative Assets path.", nameof(destinationPath));
            }
        }

        private static void CopyMetadata(BlackboardDefinitionAsset source, BlackboardDefinitionAsset destination)
        {
            string json = EditorJsonUtility.ToJson(source.AuthoringMetadata);
            EditorJsonUtility.FromJsonOverwrite(json, destination.AuthoringMetadata);
        }
    }
}

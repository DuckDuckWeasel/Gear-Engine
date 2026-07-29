using System;
using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardAuthoringTargetResolver
    {
        public BlackboardAuthoringTarget Resolve(Object source)
        {
            if (source is BlackboardDefinitionAsset asset)
            {
                return ResolveAsset(asset);
            }

            if (source is BlackboardBehaviour behaviour)
            {
                HashSet<EntityId> visited = new HashSet<EntityId>();
                return ResolveBehaviour(behaviour, visited);
            }

            throw new InvalidOperationException("Select a BlackboardBehaviour or BlackboardDefinitionAsset.");
        }

        private BlackboardAuthoringTarget ResolveAsset(BlackboardDefinitionAsset asset)
        {
            if (asset.Definition == null)
            {
                throw new InvalidOperationException($"Blackboard definition asset '{asset.name}' has no definition.");
            }

            return new BlackboardAuthoringTarget(asset, asset.Definition, asset.AuthoringMetadata, asset.name);
        }

        private BlackboardAuthoringTarget ResolveBehaviour(BlackboardBehaviour behaviour, ISet<EntityId> visited)
        {
            EnsureUnvisited(behaviour, visited);
            BlackboardDefinitionReference reference = behaviour.DefinitionReference;
            if (reference.Source == BlackboardDefinitionSource.Direct)
            {
                return ResolveDirect(behaviour, reference);
            }

            return reference.Source == BlackboardDefinitionSource.ScriptableObject
                ? ResolveAssetReference(reference)
                : ResolveVariableReference(behaviour, reference, visited);
        }

        private void EnsureUnvisited(BlackboardBehaviour behaviour, ISet<EntityId> visited)
        {
            if (behaviour == null)
            {
                throw new InvalidOperationException("The Blackboard authoring source is missing.");
            }

            if (!visited.Add(behaviour.GetEntityId()))
            {
                throw new InvalidOperationException("The Blackboard authoring source contains a wrapper-reference cycle.");
            }
        }

        private BlackboardAuthoringTarget ResolveDirect(BlackboardBehaviour behaviour, BlackboardDefinitionReference reference)
        {
            BlackboardDefinition definition = reference.DirectDefinition;
            if (definition == null)
            {
                throw new InvalidOperationException($"BlackboardBehaviour '{behaviour.name}' has no Direct definition.");
            }

            return new BlackboardAuthoringTarget(behaviour, definition, behaviour.AuthoringMetadata, behaviour.name);
        }

        private BlackboardAuthoringTarget ResolveAssetReference(BlackboardDefinitionReference reference)
        {
            BlackboardDefinitionAsset asset = reference.DefinitionAsset;
            return asset != null ? ResolveAsset(asset) : throw new InvalidOperationException("The Blackboard definition asset is missing.");
        }

        private BlackboardAuthoringTarget ResolveVariableReference(BlackboardBehaviour behaviour, BlackboardDefinitionReference reference, ISet<EntityId> visited)
        {
            BlackboardAuthoringTarget source = ResolveBehaviour(behaviour.SourceBehaviour, visited);
            BlackboardDefinition definition = FindVariableDefinition(source.Definition, reference.VariableId);
            string displayName = $"{source.DisplayName}/{reference.VariableId}";
            return new BlackboardAuthoringTarget(source.Owner, definition, source.Metadata, displayName);
        }

        private BlackboardDefinition FindVariableDefinition(BlackboardDefinition source, DefinitionId variableId)
        {
            foreach (VariableDefinitionBase variable in source.Variables)
            {
                if (variable is BlackboardDefinitionVariable definitionVariable && variable.DefinitionId == variableId)
                {
                    return definitionVariable.Value ?? throw new InvalidOperationException($"Blackboard definition variable '{variableId}' has no template.");
                }
            }

            throw new InvalidOperationException($"Blackboard definition variable '{variableId}' was not found in the source template.");
        }
    }
}

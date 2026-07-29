using System;
using Scaffold.VisualScripting;
using UnityEngine;

namespace Scaffold.VisualScripting.Authoring
{
    [Serializable]
    public sealed class BlackboardDefinitionReference
    {
        public BlackboardDefinitionSource Source => source;

        [SerializeField] private BlackboardDefinitionSource source;

        public BlackboardDefinition DirectDefinition => directDefinition;

        [SerializeField] private BlackboardDefinition directDefinition = new BlackboardDefinition();

        public BlackboardDefinitionAsset DefinitionAsset => definitionAsset;

        [SerializeField] private BlackboardDefinitionAsset definitionAsset;

        public DefinitionId VariableId => variableId;

        [SerializeField] private DefinitionId variableId;

        public void SetDirect(BlackboardDefinition definition)
        {
            source = BlackboardDefinitionSource.Direct;
            directDefinition = definition;
        }

        public void SetScriptableObject(BlackboardDefinitionAsset asset)
        {
            source = BlackboardDefinitionSource.ScriptableObject;
            definitionAsset = asset;
        }

        public void SetBlackboardVariable(DefinitionId definitionVariableId)
        {
            source = BlackboardDefinitionSource.BlackboardVariable;
            variableId = definitionVariableId;
        }

        public BlackboardDefinitionClone Instantiate(SerializedGraphCloner cloner, BlackboardDefinitionValidator validator, IBlackboardDefinitionVariableSource variableSource = null)
        {
            if (cloner == null)
            {
                throw new ArgumentNullException(nameof(cloner));
            }

            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            BlackboardDefinition definition = ResolveDefinition(variableSource);
            BlackboardDefinitionClone clone = cloner.Clone(definition);
            validator.ValidateOrThrow(clone.Definition);
            return clone;
        }

        public BlackboardDefinition ResolveDefinition(IBlackboardDefinitionVariableSource variableSource = null)
        {
            switch (source)
            {
                case BlackboardDefinitionSource.Direct:
                    return directDefinition ?? throw new BlackboardDefinitionResolutionException("The Direct Blackboard definition is missing.");
                case BlackboardDefinitionSource.ScriptableObject:
                    return ResolveAsset();
                case BlackboardDefinitionSource.BlackboardVariable:
                    return ResolveVariable(variableSource);
                default:
                    throw new BlackboardDefinitionResolutionException($"Unsupported Blackboard definition source '{source}'.");
            }
        }

        private BlackboardDefinition ResolveAsset()
        {
            if (definitionAsset == null)
            {
                throw new BlackboardDefinitionResolutionException("The Blackboard definition asset is missing.");
            }

            return definitionAsset.Definition ?? throw new BlackboardDefinitionResolutionException($"The Blackboard definition asset '{definitionAsset.name}' has no definition.");
        }

        private BlackboardDefinition ResolveVariable(IBlackboardDefinitionVariableSource variableSource)
        {
            ValidateVariableRequest(variableSource);
            if (!variableSource.TryGetBlackboardDefinition(variableId, out BlackboardDefinition definition) || definition == null)
            {
                throw new BlackboardDefinitionResolutionException($"Blackboard definition variable '{variableId}' could not be resolved.");
            }

            return definition;
        }

        private void ValidateVariableRequest(IBlackboardDefinitionVariableSource variableSource)
        {
            if (variableSource == null)
            {
                throw new BlackboardDefinitionResolutionException("A BlackboardVariable source requires an already-running source Blackboard.");
            }

            if (variableId.IsEmpty)
            {
                throw new BlackboardDefinitionResolutionException("The Blackboard definition variable ID is missing.");
            }
        }
    }
}

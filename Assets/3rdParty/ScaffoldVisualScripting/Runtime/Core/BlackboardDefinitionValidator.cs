using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardDefinitionValidator
    {
        public BlackboardDefinitionValidator()
        {
            visitDefinitionsCallback = VisitDefinitions;
            validateNestedTemplatesCallback = ValidateNestedTemplates;
        }

        private readonly Action<object, string, ISet<object>, IDictionary<DefinitionId, string>, ICollection<BlackboardValidationIssue>> visitDefinitionsCallback;
        private readonly Action<BlackboardDefinition, string, ISet<BlackboardDefinition>, ICollection<BlackboardValidationIssue>> validateNestedTemplatesCallback;

        public void ValidateOrThrow(BlackboardDefinition definition)
        {
            IReadOnlyList<BlackboardValidationIssue> issues = Validate(definition);
            if (issues.Count > 0)
            {
                throw new BlackboardValidationException(issues);
            }
        }

        public IReadOnlyList<BlackboardValidationIssue> Validate(BlackboardDefinition definition)
        {
            List<BlackboardValidationIssue> issues = new List<BlackboardValidationIssue>();
            if (definition == null)
            {
                issues.Add(new BlackboardValidationIssue("Blackboard", "The definition is missing."));
                return issues;
            }

            ValidateRequiredGraph(definition, issues);
            ValidateDefinitionIds(definition, issues);
            HashSet<BlackboardDefinition> active = new HashSet<BlackboardDefinition>(ReferenceEqualityComparer.Instance);
            ValidateNestedTemplates(definition, "Blackboard", active, issues);
            return issues;
        }

        private void ValidateRequiredGraph(BlackboardDefinition definition, ICollection<BlackboardValidationIssue> issues)
        {
            ValidateBlocks(definition, issues);
            ValidateVariables(definition, issues);
        }

        private void ValidateBlocks(BlackboardDefinition definition, ICollection<BlackboardValidationIssue> issues)
        {
            for (int index = 0; index < definition.Blocks.Count; index++)
            {
                string path = $"Blackboard.Blocks[{index}]";
                ValidateBlock(definition.Blocks[index], path, issues);
            }
        }

        private void ValidateBlock(BlockDefinition block, string path, ICollection<BlackboardValidationIssue> issues)
        {
            if (block == null)
            {
                issues.Add(new BlackboardValidationIssue(path, "The Block definition is null."));
                return;
            }

            ValidateTracks(block, path, issues);
        }

        private void ValidateTracks(BlockDefinition block, string blockPath, ICollection<BlackboardValidationIssue> issues)
        {
            for (int index = 0; index < block.Tracks.Count; index++)
            {
                string path = $"{blockPath}.Tracks[{index}]";
                ValidateTrack(block.Tracks[index], path, issues);
            }
        }

        private void ValidateTrack(ActionTrackDefinition track, string path, ICollection<BlackboardValidationIssue> issues)
        {
            if (track == null)
            {
                issues.Add(new BlackboardValidationIssue(path, "The Action Track definition is null."));
                return;
            }

            if (track.ActionList == null)
            {
                issues.Add(new BlackboardValidationIssue($"{path}.ActionList", "The Action List definition is null."));
                return;
            }

            ValidateActions(track.ActionList, path, issues);
        }

        private void ValidateActions(ActionListDefinition actionList, string trackPath, ICollection<BlackboardValidationIssue> issues)
        {
            for (int index = 0; index < actionList.Actions.Count; index++)
            {
                if (actionList.Actions[index] == null)
                {
                    string path = $"{trackPath}.ActionList.Actions[{index}]";
                    issues.Add(new BlackboardValidationIssue(path, "The Action definition is null."));
                }
            }
        }

        private void ValidateVariables(BlackboardDefinition definition, ICollection<BlackboardValidationIssue> issues)
        {
            for (int index = 0; index < definition.Variables.Count; index++)
            {
                if (definition.Variables[index] == null)
                {
                    string path = $"Blackboard.Variables[{index}]";
                    issues.Add(new BlackboardValidationIssue(path, "The Variable definition is null."));
                }
            }
        }

        private void ValidateDefinitionIds(BlackboardDefinition definition, ICollection<BlackboardValidationIssue> issues)
        {
            HashSet<object> visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            Dictionary<DefinitionId, string> pathsById = new Dictionary<DefinitionId, string>();
            VisitDefinitions(definition, "Blackboard", visited, pathsById, issues);
        }

        private void VisitDefinitions(object value, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            if (ShouldSkipDefinitionValue(value))
            {
                return;
            }

            Type type = value.GetType();
            if (!type.IsValueType && !visited.Add(value))
            {
                return;
            }

            CheckDefinitionId(value, path, pathsById, issues);
            VisitDefinitionMembers(value, type, path, visited, pathsById, issues);
        }

        private bool ShouldSkipDefinitionValue(object value)
        {
            if (value == null || value is Object)
            {
                return true;
            }

            Type type = value.GetType();
            return type.IsPrimitive || type.IsEnum || type == typeof(string);
        }

        private void CheckDefinitionId(object value, string path, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            if (!(value is IDefinitionNode node))
            {
                return;
            }

            if (node.DefinitionId.IsEmpty)
            {
                issues.Add(new BlackboardValidationIssue(path, "The definition ID is missing."));
                return;
            }

            RegisterDefinitionId(node, path, pathsById, issues);
        }

        private void RegisterDefinitionId(IDefinitionNode node, string path, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            if (pathsById.TryGetValue(node.DefinitionId, out string ownerPath))
            {
                string message = $"Definition ID '{node.DefinitionId}' is already used by {ownerPath}.";
                issues.Add(new BlackboardValidationIssue(path, message));
                return;
            }

            pathsById.Add(node.DefinitionId, path);
        }

        private void VisitDefinitionMembers(object value, Type type, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            if (value is IDictionary dictionary)
            {
                VisitDictionary(dictionary, path, visited, pathsById, issues);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                VisitEnumerable(enumerable, path, visited, pathsById, issues);
                return;
            }

            VisitFields(value, type, path, visited, pathsById, issues);
        }

        private void VisitDictionary(IDictionary dictionary, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            int index = 0;
            foreach (DictionaryEntry entry in dictionary)
            {
                visitDefinitionsCallback(entry.Key, $"{path}.Keys[{index}]", visited, pathsById, issues);
                visitDefinitionsCallback(entry.Value, $"{path}.Values[{index}]", visited, pathsById, issues);
                index++;
            }
        }

        private void VisitEnumerable(IEnumerable enumerable, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            int index = 0;
            foreach (object item in enumerable)
            {
                visitDefinitionsCallback(item, $"{path}[{index}]", visited, pathsById, issues);
                index++;
            }
        }

        private void VisitFields(object value, Type type, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                VisitDeclaredFields(value, current, path, visited, pathsById, issues);
            }
        }

        private void VisitDeclaredFields(object value, Type type, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                VisitDefinitionField(value, field, path, visited, pathsById, issues);
            }
        }

        private void VisitDefinitionField(object value, FieldInfo field, string path, ISet<object> visited, IDictionary<DefinitionId, string> pathsById, ICollection<BlackboardValidationIssue> issues)
        {
            if (field.IsStatic || typeof(Delegate).IsAssignableFrom(field.FieldType))
            {
                return;
            }

            object fieldValue = field.GetValue(value);
            char firstCharacter = char.ToUpperInvariant(field.Name[0]);
            string suffix = field.Name.Substring(1);
            string fieldName = firstCharacter + suffix;
            visitDefinitionsCallback(fieldValue, $"{path}.{fieldName}", visited, pathsById, issues);
        }

        private void ValidateNestedTemplates(BlackboardDefinition definition, string path, ISet<BlackboardDefinition> active, ICollection<BlackboardValidationIssue> issues)
        {
            if (!active.Add(definition))
            {
                issues.Add(new BlackboardValidationIssue(path, "Blackboard definition variables contain a reference cycle."));
                return;
            }

            for (int index = 0; index < definition.Variables.Count; index++)
            {
                string variablePath = $"{path}.Variables[{index}]";
                ValidateNestedVariable(definition.Variables[index], variablePath, active, issues);
            }

            active.Remove(definition);
        }

        private void ValidateNestedVariable(VariableDefinition definition, string path, ISet<BlackboardDefinition> active, ICollection<BlackboardValidationIssue> issues)
        {
            if (!(definition is BlackboardDefinitionVariable variable))
            {
                return;
            }

            if (variable.Value == null)
            {
                issues.Add(new BlackboardValidationIssue(path, "The Blackboard definition value is missing."));
                return;
            }

            validateNestedTemplatesCallback(variable.Value, $"{path}.Value", active, issues);
        }
    }
}

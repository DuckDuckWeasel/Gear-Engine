using System;
using System.Collections.Generic;
using System.Linq;

namespace Scaffold.Entities
{
    /// <summary>
    /// Runtime variable storage for an entity instance.
    /// Stores base values keyed by VariableSO and supports stackable modifiers.
    /// </summary>
    public abstract partial class EntityInstance<TDefinition> : IDisposable where TDefinition : IEntityDefinition
    {
        public TDefinition Definition { get; }

        private readonly Dictionary<VariableSO, float> baseValues = new Dictionary<VariableSO, float>();
        private readonly Dictionary<VariableSO, List<(ModifierId Id, VariableModifier Modifier)>> modifierStacks
            = new Dictionary<VariableSO, List<(ModifierId, VariableModifier)>>();

        protected EntityInstance(TDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Adds or overwrites a base variable value for the given key.
        /// </summary>
        public void AddVariable(VariableSO key, float initialValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            baseValues[key] = initialValue;
        }

        /// <summary>
        /// Tries to get the effective value of a variable (base + all modifiers applied).
        /// Returns false if the variable has not been added.
        /// </summary>
        public bool TryGetVariable(VariableSO key, out float value)
        {
            if (key == null || !baseValues.TryGetValue(key, out float baseVal))
            {
                value = 0f;
                return false;
            }

            value = baseVal;
            if (modifierStacks.TryGetValue(key, out List<(ModifierId Id, VariableModifier Modifier)> stack))
            {
                foreach ((ModifierId _, VariableModifier mod) in stack)
                {
                    value = mod.Apply(value);
                }
            }
            return true;
        }

        /// <summary>
        /// Adds a modifier to the specified variable and returns a ModifierId for later removal.
        /// </summary>
        public ModifierId AddModifier(VariableSO key, VariableModifier modifier)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            ModifierId id = ModifierId.New();
            if (!modifierStacks.TryGetValue(key, out List<(ModifierId Id, VariableModifier Modifier)> stack))
            {
                stack = new List<(ModifierId, VariableModifier)>();
                modifierStacks[key] = stack;
            }

            int insertAt = 0;
            while (insertAt < stack.Count && stack[insertAt].Modifier.Order <= modifier.Order)
            {
                insertAt++;
            }
            stack.Insert(insertAt, (id, modifier));
            return id;
        }

        /// <summary>
        /// Removes a specific modifier by its ID from the specified variable.
        /// </summary>
        public bool RemoveModifier(VariableSO key, ModifierId id)
        {
            if (key == null || !modifierStacks.TryGetValue(key, out List<(ModifierId Id, VariableModifier Modifier)> stack))
            {
                return false;
            }
            return stack.RemoveAll(entry => entry.Id == id) > 0;
        }

        /// <summary>
        /// Removes all modifiers from all variables.
        /// </summary>
        public void ClearModifiers()
        {
            modifierStacks.Clear();
        }

        public virtual void Dispose() { }
    }
}

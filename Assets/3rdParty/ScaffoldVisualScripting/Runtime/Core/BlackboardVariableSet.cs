using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardVariableSet : IDisposable, IBlackboardDefinitionVariableSource
    {
        public BlackboardVariableSet(BlackboardRuntimeInstanceId runtimeInstanceId, IEnumerable<VariableDefinitionBase> definitions, IPublicVariableRegistry publicRegistry, IGlobalVariableStore globalStore)
        {
            RuntimeInstanceId = runtimeInstanceId;
            this.publicRegistry = publicRegistry ?? throw new ArgumentNullException(nameof(publicRegistry));
            this.globalStore = globalStore ?? throw new ArgumentNullException(nameof(globalStore));
            AddDefinitions(definitions ?? throw new ArgumentNullException(nameof(definitions)));
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public IVariableStore LocalVariables => localVariables;

        public IVariableStore PublicVariables => publicVariables;

        public IReadOnlyList<VariableCellBase> Cells => CreateCellSnapshot();

        private readonly VariableStore localVariables = new VariableStore();
        private readonly VariableStore publicVariables = new VariableStore();
        private readonly VariableStore globalBindings = new VariableStore();
        private readonly IPublicVariableRegistry publicRegistry;
        private readonly IGlobalVariableStore globalStore;
        private bool disposed;

        public VariableCell<T> Get<T>(VariableReference reference)
        {
            VariableCellBase cell = Resolve(reference);
            if (cell is VariableCell<T> typedCell)
            {
                return typedCell;
            }

            throw new VariableTypeMismatchException(cell.DefinitionId, typeof(T), cell.ValueType);
        }

        public VariableCellBase Resolve(VariableReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (reference.DefinitionId.IsEmpty)
            {
                throw new InvalidOperationException("The variable reference has no definition ID.");
            }

            return ResolveByScope(reference);
        }

        public bool TryGet(DefinitionId definitionId, out VariableCellBase cell)
        {
            return TryResolveOwnVariable(definitionId, out cell);
        }

        public bool TryGetBlackboardDefinition(DefinitionId variableId, out BlackboardDefinition definition)
        {
            definition = null;
            if (!TryResolveOwnVariable(variableId, out VariableCellBase cell))
            {
                return false;
            }

            if (!(cell is VariableCell<BlackboardDefinition> definitionCell))
            {
                return false;
            }

            definition = definitionCell.Value;
            return definition != null;
        }

        public void Reset()
        {
            ResetStore(localVariables);
            ResetStore(publicVariables);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            publicRegistry.Unregister(RuntimeInstanceId);
        }

        private void AddDefinitions(IEnumerable<VariableDefinitionBase> definitions)
        {
            foreach (VariableDefinitionBase definition in definitions)
            {
                AddDefinition(definition);
            }
        }

        private IReadOnlyList<VariableCellBase> CreateCellSnapshot()
        {
            List<VariableCellBase> cells = new List<VariableCellBase>();
            cells.AddRange(localVariables.Cells);
            cells.AddRange(publicVariables.Cells);
            cells.AddRange(globalBindings.Cells);
            return cells;
        }

        private void AddDefinition(VariableDefinitionBase definition)
        {
            if (definition == null)
            {
                throw new InvalidOperationException("A Blackboard variable definition is null.");
            }

            if (definition.Scope == VariableScope.Local)
            {
                AddLocalDefinition(definition);
                return;
            }

            if (definition.Scope == VariableScope.Public)
            {
                AddPublicDefinition(definition);
                return;
            }

            AddGlobalDefinition(definition);
        }

        private void AddLocalDefinition(VariableDefinitionBase definition)
        {
            localVariables.Add(definition.CreateCell());
        }

        private void AddPublicDefinition(VariableDefinitionBase definition)
        {
            VariableCellBase cell = definition.CreateCell();
            publicVariables.Add(cell);
            VariableAddress address = new VariableAddress(RuntimeInstanceId, definition.DefinitionId);
            publicRegistry.Register(address, cell);
        }

        private void AddGlobalDefinition(VariableDefinitionBase definition)
        {
            if (definition.Scope != VariableScope.InjectedGlobal)
            {
                throw new InvalidOperationException($"Unsupported variable scope '{definition.Scope}'.");
            }

            globalBindings.Add(globalStore.GetOrAdd(definition));
        }

        private VariableCellBase ResolveByScope(VariableReference reference)
        {
            switch (reference.Scope)
            {
                case VariableScope.Local:
                    return GetRequired(localVariables, reference.DefinitionId);
                case VariableScope.Public:
                    return ResolvePublic(reference);
                case VariableScope.InjectedGlobal:
                    return GetRequired(globalBindings, reference.DefinitionId);
                default:
                    throw new InvalidOperationException($"Unsupported variable scope '{reference.Scope}'.");
            }
        }

        private VariableCellBase ResolvePublic(VariableReference reference)
        {
            BlackboardRuntimeInstanceId sourceId = reference.SourceRuntimeInstanceId;
            if (sourceId.IsEmpty || sourceId == RuntimeInstanceId)
            {
                return GetRequired(publicVariables, reference.DefinitionId);
            }

            VariableAddress address = new VariableAddress(sourceId, reference.DefinitionId);
            if (publicRegistry.TryGet(address, out VariableCellBase cell))
            {
                return cell;
            }

            throw new KeyNotFoundException($"Public variable '{address}' is not registered.");
        }

        private bool TryResolveOwnVariable(DefinitionId definitionId, out VariableCellBase cell)
        {
            return localVariables.TryGet(definitionId, out cell) || publicVariables.TryGet(definitionId, out cell) || globalBindings.TryGet(definitionId, out cell);
        }

        private VariableCellBase GetRequired(IVariableStore store, DefinitionId definitionId)
        {
            if (store.TryGet(definitionId, out VariableCellBase cell))
            {
                return cell;
            }

            throw new KeyNotFoundException($"Variable '{definitionId}' is not registered for Blackboard '{RuntimeInstanceId}'.");
        }

        private void ResetStore(IVariableStore store)
        {
            foreach (VariableCellBase cell in store.Cells)
            {
                cell.Reset();
            }
        }
    }
}

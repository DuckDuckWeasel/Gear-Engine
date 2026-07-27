using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scaffold.VisualScripting
{
    public sealed class Blackboard : IBlackboardHandle, IDisposable
    {
        public Blackboard(BlackboardRuntimeInstanceId runtimeInstanceId, BlackboardVariableSet variables, IFrameScheduler scheduler, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger) : this(new BlackboardDefinitionClone(new BlackboardDefinition(), runtimeInstanceId), variables, BlackboardRuntimeServices.CreateExecutionOnly(scheduler, timeSource, eventBus, saveService, logger), new SystemRandomSource(), false)
        {
        }

        internal Blackboard(BlackboardDefinitionClone definitionClone, BlackboardVariableSet variables, BlackboardRuntimeServices services, IRandomSource randomSource, bool registerRuntime)
        {
            if (definitionClone == null)
            {
                throw new ArgumentNullException(nameof(definitionClone));
            }

            Definition = definitionClone.Definition;
            RuntimeInstanceId = definitionClone.RuntimeInstanceId;
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            isEnabled = true;
            BuildRuntimeGraph();
            RegisterRuntime(registerRuntime);
        }

        public BlackboardDefinition Definition { get; }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public BlackboardVariableSet Variables { get; }

        public IReadOnlyList<Block> Blocks => blocks;

        public IFrameScheduler Scheduler => Services.Scheduler;

        public ITimeSource TimeSource => Services.TimeSource;

        public IBlackboardEventBus EventBus => Services.EventBus;

        public IBlackboardSaveService SaveService => Services.SaveService;

        public IBlackboardLogger Logger => Services.Logger;

        public bool HasStarted { get; private set; }

        public bool IsEnabled => isEnabled;

        public bool IsDisposed => disposed;

        internal BlackboardRuntimeServices Services { get; }

        private readonly IRandomSource randomSource;
        private readonly List<Block> blocks = new List<Block>();
        private readonly List<ITriggerBinding> triggerBindings = new List<ITriggerBinding>();
        private readonly Dictionary<DefinitionId, Block> blocksById = new Dictionary<DefinitionId, Block>();
        private bool isEnabled;
        private bool disposed;
        private bool registered;

        public void Start()
        {
            ThrowIfDisposed();
            if (HasStarted)
            {
                return;
            }

            HasStarted = true;
            if (!isEnabled)
            {
                return;
            }

            EnableTriggerBindings();
            PublishEnabled();
            EventBus.Publish(new BlackboardStartedEvent(RuntimeInstanceId));
        }

        public void Enable()
        {
            ThrowIfDisposed();
            if (isEnabled)
            {
                return;
            }

            isEnabled = true;
            if (!HasStarted)
            {
                return;
            }

            EnableTriggerBindings();
            PublishEnabled();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (isEnabled)
            {
                Disable();
            }

            DisposeTriggerBindings();
            DisposeBlocks();
            Variables.Dispose();
            UnregisterRuntime();
            disposed = true;
        }

        public void Disable()
        {
            ThrowIfDisposed();
            if (!isEnabled)
            {
                return;
            }

            if (HasStarted)
            {
                EventBus.Publish(new BlackboardDisabledEvent(RuntimeInstanceId));
            }

            isEnabled = false;
            DisableTriggerBindings();
            StopAllInternal();
        }

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
            }

            if (!HasStarted || !isEnabled)
            {
                return;
            }

            Scheduler.Tick(deltaTime);
            TickTriggers();
            TickBlocks();
        }

        public bool ExecuteBlock(DefinitionId definitionId, Action<ActionExecutionStatus> onComplete = null)
        {
            ThrowIfDisposed();
            return blocksById.TryGetValue(definitionId, out Block block) && ExecuteBlock(block, onComplete);
        }

        public bool ExecuteBlock(string blockName, Action<ActionExecutionStatus> onComplete = null)
        {
            ThrowIfDisposed();
            Block block = FindBlock(blockName);
            return block != null && ExecuteBlock(block, onComplete);
        }

        public bool ExecuteBlock(Block block, Action<ActionExecutionStatus> onComplete = null)
        {
            ThrowIfDisposed();
            if (!CanExecute(block))
            {
                return false;
            }

            block.Execute(onComplete ?? IgnoreCompletion);
            return true;
        }

        public bool StopBlock(DefinitionId definitionId)
        {
            ThrowIfDisposed();
            return blocksById.TryGetValue(definitionId, out Block block) &&
                StopBlock(block);
        }

        public bool StopBlock(string blockName)
        {
            ThrowIfDisposed();
            Block block = FindBlock(blockName);
            return block != null && StopBlock(block);
        }

        public void StopAll()
        {
            ThrowIfDisposed();
            StopAllInternal();
        }

        public void Reset()
        {
            ThrowIfDisposed();
            StopAllInternal();
            Variables.Reset();
            foreach (Block block in blocks)
            {
                block.ResetExecutionFeedback();
            }
        }

        public VariableCell<T> GetVariable<T>(VariableReference reference)
        {
            ThrowIfDisposed();
            return Variables.Get<T>(reference);
        }

        public bool TryGetVariable(string key, out VariableCellBase cell)
        {
            ThrowIfDisposed();
            return Variables.TryGet(key, out cell);
        }

        public string Substitute(string input)
        {
            ThrowIfDisposed();
            return Services.TextSubstitution.Substitute(input, Variables);
        }

        public void SendMessage(string name, object payload = null)
        {
            SendMessage(RuntimeInstanceId, name, payload);
        }

        public void SendMessage(BlackboardRuntimeInstanceId targetRuntimeInstanceId, string name, object payload = null)
        {
            ThrowIfDisposed();
            EnsureRunning();
            EventBus.Publish(new BlackboardMessage(RuntimeInstanceId, name, payload, targetRuntimeInstanceId));
        }

        public void BroadcastMessage(string name, object payload = null)
        {
            ThrowIfDisposed();
            EnsureRunning();
            EventBus.Publish(new BlackboardMessage(RuntimeInstanceId, name, payload));
        }

        public async Task SaveAsync(string slot, CancellationToken cancellationToken = default)
        {
            try
            {
                ThrowIfDisposed();
                BlackboardSaveData data = Services.VariablePersistence.Capture(Variables);
                await SaveService.SaveAsync(slot, data, cancellationToken);
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to save Blackboard '{RuntimeInstanceId}'.", exception);
                throw;
            }
        }

        public async Task LoadAsync(string slot, CancellationToken cancellationToken = default)
        {
            try
            {
                ThrowIfDisposed();
                BlackboardSaveData data = await SaveService.LoadAsync(slot, RuntimeInstanceId, cancellationToken);
                ApplyLoadedData(slot, data);
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to load Blackboard '{RuntimeInstanceId}'.", exception);
                throw;
            }
        }

        public async Task DeleteSaveAsync(string slot, CancellationToken cancellationToken = default)
        {
            try
            {
                ThrowIfDisposed();
                await SaveService.DeleteAsync(slot, RuntimeInstanceId, cancellationToken);
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to delete save data for Blackboard '{RuntimeInstanceId}'.", exception);
                throw;
            }
        }

        private void BuildRuntimeGraph()
        {
            try
            {
                CreateBlocks();
                CreateTriggerBindings();
            }
            catch
            {
                DisposeTriggerBindings();
                DisposeBlocks();
                throw;
            }
        }

        private void CreateBlocks()
        {
            foreach (BlockDefinition definition in Definition.Blocks)
            {
                Block block = new Block(this, definition, randomSource.NextValue);
                blocks.Add(block);
                blocksById.Add(definition.DefinitionId, block);
            }
        }

        private void CreateTriggerBindings()
        {
            foreach (Block block in blocks)
            {
                TriggerDefinition trigger = block.Definition.Trigger;
                if (trigger == null || !trigger.Enabled)
                {
                    continue;
                }

                TriggerExecutionContext context = new TriggerExecutionContext(this, block);
                ITriggerBinding binding = trigger.CreateBinding(context);
                triggerBindings.Add(binding ?? throw new InvalidOperationException($"Trigger '{trigger.GetType().Name}' returned a null binding."));
            }
        }

        private void RegisterRuntime(bool registerRuntime)
        {
            if (!registerRuntime)
            {
                return;
            }

            Services.Registry.Register(this);
            registered = true;
        }

        private void UnregisterRuntime()
        {
            if (!registered)
            {
                return;
            }

            Services.Registry.Unregister(RuntimeInstanceId);
            registered = false;
        }

        private void EnableTriggerBindings()
        {
            foreach (ITriggerBinding binding in triggerBindings)
            {
                binding.Enable();
            }
        }

        private void DisableTriggerBindings()
        {
            foreach (ITriggerBinding binding in triggerBindings)
            {
                binding.Disable();
            }
        }

        private void TickTriggers()
        {
            foreach (ITriggerBinding binding in triggerBindings)
            {
                binding.Tick();
            }
        }

        private void TickBlocks()
        {
            foreach (Block block in blocks)
            {
                if (block.State == BlockExecutionState.Executing)
                {
                    block.Tick();
                }
            }
        }

        private void PublishEnabled()
        {
            EventBus.Publish(new BlackboardEnabledEvent(RuntimeInstanceId));
        }

        private bool CanExecute(Block block)
        {
            if (!HasStarted || !isEnabled || !Owns(block))
            {
                return false;
            }

            if (block.State != BlockExecutionState.Executing)
            {
                return true;
            }

            Logger.Warning($"Block '{block.Definition.Name}' is already executing.");
            return false;
        }

        private bool StopBlock(Block block)
        {
            if (!Owns(block) || block.State != BlockExecutionState.Executing)
            {
                return false;
            }

            block.Stop();
            return true;
        }

        internal bool Owns(Block block)
        {
            return block != null && blocksById.TryGetValue(block.Definition.DefinitionId, out Block owned) && ReferenceEquals(block, owned);
        }

        private Block FindBlock(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName))
            {
                return null;
            }

            foreach (Block block in blocks)
            {
                if (string.Equals(block.Definition.Name, blockName, StringComparison.Ordinal))
                {
                    return block;
                }
            }

            return null;
        }

        private void StopAllInternal()
        {
            foreach (Block block in blocks)
            {
                if (block.State == BlockExecutionState.Executing)
                {
                    block.Stop();
                }
            }
        }

        private void DisposeTriggerBindings()
        {
            foreach (ITriggerBinding binding in triggerBindings)
            {
                binding.Dispose();
            }

            triggerBindings.Clear();
        }

        private void DisposeBlocks()
        {
            foreach (Block block in blocks)
            {
                block.Dispose();
            }

            blocks.Clear();
            blocksById.Clear();
        }

        private void EnsureRunning()
        {
            if (!HasStarted || !isEnabled)
            {
                throw new InvalidOperationException("The Blackboard must be started and enabled.");
            }
        }

        private void ApplyLoadedData(string slot, BlackboardSaveData data)
        {
            if (data == null)
            {
                throw new InvalidOperationException($"Save slot '{slot}' has no data for Blackboard '{RuntimeInstanceId}'.");
            }

            Services.VariablePersistence.Apply(data, Variables);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(Blackboard));
            }
        }

        private void IgnoreCompletion(ActionExecutionStatus status)
        {
        }
    }
}

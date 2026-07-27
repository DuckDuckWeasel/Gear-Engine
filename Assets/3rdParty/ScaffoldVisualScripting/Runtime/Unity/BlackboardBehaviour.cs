using System;
using Scaffold.VisualScripting.Authoring;
using UnityEngine;
using VContainer;

namespace Scaffold.VisualScripting.Unity
{
    [DisallowMultipleComponent]
    public sealed class BlackboardBehaviour : MonoBehaviour
    {
        public BlackboardDefinitionReference DefinitionReference => definitionReference;

        [SerializeField] private BlackboardDefinitionReference definitionReference = new BlackboardDefinitionReference();

        public BlackboardBehaviour SourceBehaviour
        {
            get => sourceBehaviour;
            set => sourceBehaviour = value;
        }

        [SerializeField] private BlackboardBehaviour sourceBehaviour;

        public Blackboard Runtime { get; private set; }

        public bool IsRuntimeAvailable => Runtime != null && !Runtime.IsDisposed;

        private BlackboardFactory factory;

        [Inject]
        public void Construct(BlackboardFactory runtimeFactory)
        {
            factory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
        }

        public bool ExecuteBlock(string blockName)
        {
            return IsRuntimeAvailable && Runtime.ExecuteBlock(blockName);
        }

        public bool StopBlock(string blockName)
        {
            return IsRuntimeAvailable && Runtime.StopBlock(blockName);
        }

        public bool TrySendBlackboardMessage(string messageName, object payload = null)
        {
            if (!IsRuntimeAvailable || !Runtime.HasStarted || !Runtime.IsEnabled)
            {
                return false;
            }

            Runtime.SendMessage(messageName, payload);
            return true;
        }

        private void Awake()
        {
            try
            {
                InitializeRuntime();
            }
            catch (Exception exception)
            {
                HandleInitializationFailure(exception);
            }
        }

        private void OnEnable()
        {
            Runtime?.Enable();
        }

        private void Start()
        {
            Runtime?.Start();
        }

        private void Update()
        {
            Runtime?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (IsRuntimeAvailable)
            {
                Runtime.Disable();
            }
        }

        private void OnDestroy()
        {
            try
            {
                Runtime?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BlackboardBehaviour] Failed to dispose '{name}': {exception}");
            }
        }

        private void InitializeRuntime()
        {
            if (factory == null)
            {
                throw new InvalidOperationException("BlackboardFactory was not injected before Awake.");
            }

            IBlackboardDefinitionVariableSource variableSource = ResolveVariableSource();
            BlackboardDefinition template = definitionReference.ResolveDefinition(variableSource);
            Runtime = factory.Create(template);
        }

        private IBlackboardDefinitionVariableSource ResolveVariableSource()
        {
            if (definitionReference.Source != BlackboardDefinitionSource.BlackboardVariable)
            {
                return null;
            }

            if (sourceBehaviour == null || !sourceBehaviour.IsRuntimeAvailable || !sourceBehaviour.Runtime.HasStarted)
            {
                throw new BlackboardDefinitionResolutionException("A BlackboardVariable wrapper source must reference an already-running BlackboardBehaviour.");
            }

            return sourceBehaviour.Runtime;
        }

        private void HandleInitializationFailure(Exception exception)
        {
            Runtime?.Dispose();
            Runtime = null;
            Debug.LogError($"[BlackboardBehaviour] Failed to initialize '{name}': {exception}");
            enabled = false;
        }
    }
}

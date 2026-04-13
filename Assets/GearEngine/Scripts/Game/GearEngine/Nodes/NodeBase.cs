using UnityEngine;
using Scaffold.Events.Contracts;

namespace GearEngine.GearEngine.Nodes
{
    public abstract class NodeBase : IGridNode
    {
        protected NodeBase(IGridManager grid, IEventBus eventBus)
        {
            this.grid = grid;
            this.eventBus = eventBus;
        }

        public Vector2Int Position { get; set; }

        public float CurrentRotation
        {
            get => currentRotation;
            protected set
            {
                if (currentRotation != value)
                {
                    LastRotationDelta = Mathf.DeltaAngle(currentRotation, value);
                    currentRotation = value;
                }
            }
        }

        private float currentRotation;

        public float LastRotationDelta { get; private set; }
        public GearConfigData ConfigData { get; private set; }
        public float LocalSpeedMultiplier { get; set; } = 1.0f;
        public bool IsActive { get; set; } = true;
        public bool IsInteractable { get; private set; } = true;

        public Scaffold.Events.Contracts.IEventBus EventBus => eventBus;

        protected readonly IGridManager grid;
        protected readonly Scaffold.Events.Contracts.IEventBus eventBus;

        private readonly System.Collections.Generic.List<RuntimeAbility> activeAbilities = new System.Collections.Generic.List<RuntimeAbility>();

        public virtual void Initialize(Vector2Int position, GearConfigData configData)
        {
            Position = position;
            ConfigData = configData;
            IsActive = true;
            IsInteractable = configData?.IsInteractable ?? true;

            if (configData?.Abilities != null)
            {
                foreach (var ability in configData.Abilities)
                {
                    AddAbility(ability, -1f);
                }
            }
        }

        public abstract void NodeUpdate(float deltaTime, float speedModifier);

        public virtual void WindDownUpdate(float deltaTime, float speedModifier)
        {
            float target90 = ComputeWindDownSnapTarget();
            CurrentRotation = Mathf.LerpAngle(CurrentRotation, target90, deltaTime * 5f);
            TickAbilities(deltaTime);
        }

        private float ComputeWindDownSnapTarget()
        {
            if (LastRotationDelta > 0)
            {
                return Mathf.Ceil(CurrentRotation / 90f) * 90f;
            }

            if (LastRotationDelta < 0)
            {
                return Mathf.Floor(CurrentRotation / 90f) * 90f;
            }

            return Mathf.Round(CurrentRotation / 90f) * 90f;
        }

        public void AddAbility(GearAbilitySO ability, float duration = -1f)
        {
            if (ability == null)
            {
                return;
            }

            var runtimeStatus = new RuntimeAbility(ability, duration);
            activeAbilities.Add(runtimeStatus);
            ability.OnActive(this);
        }

        public void RemoveAbility(GearAbilitySO ability)
        {
            for (int i = activeAbilities.Count - 1; i >= 0; i--)
            {
                if (activeAbilities[i].AbilityDef == ability)
                {
                    ability.OnDeactive(this);
                    activeAbilities.RemoveAt(i);
                }
            }
        }

        protected void TickAbilities(float deltaTime)
        {
            for (int i = activeAbilities.Count - 1; i >= 0; i--)
            {
                var runInfo = activeAbilities[i];
                runInfo.AbilityDef.Tick(this, deltaTime);

                if (!runInfo.IsPermanent)
                {
                    runInfo.DurationRemaining -= deltaTime;
                    if (runInfo.DurationRemaining <= 0)
                    {
                        runInfo.AbilityDef.OnDeactive(this);
                        activeAbilities.RemoveAt(i);
                    }
                }
            }
        }

        protected void ExecuteAbilities()
        {
            if (activeAbilities.Count == 0)
            {
                Debug.Log($"<color=#ff99cc>[NodeBase]</color> {Position} Attempted to execute abilities, but none were attached.");
                return;
            }

            string abilityNames = "";
            foreach (var runInfo in activeAbilities)
            {
                abilityNames += runInfo.AbilityDef.name + ", ";
                runInfo.AbilityDef.Execute(this);
            }

            abilityNames = abilityNames.TrimEnd(',', ' ');
            Debug.Log($"<color=#ff99cc>[NodeBase]</color> {Position} Executed {activeAbilities.Count} abilities: [ {abilityNames} ]");
        }

        public virtual void Dispose()
        {
        }
    }
}

using GearEngine.GearEngine;
using System;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Events;
using Scaffold.Entities;
using UnityEngine;
using Scaffold.Events.Contracts;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "MartyrGear", menuName = "GearEngine/Abilities/Group A/Martyr Gear")]
    public sealed class MartyrGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [Header("Sacrifice Reward")]
        [SerializeField] private VariableSO targetVariable;
        [SerializeField] private float martyrBuffValue = 50f;

        private IGridNode currentOwner;
        private Action<GearDestroyedEvent> destructionCallback;
        private bool isDead;

        public override void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            base.Initialize(state, gearEngine);
            isDead = false;
            destructionCallback = OnGearDestroyed;
        }

        public override void OnActive(IGridNode owner)
        {
            base.OnActive(owner);
            currentOwner = owner;
            owner.EventBus?.AddListener(destructionCallback);
        }

        public override void OnDeactive(IGridNode owner)
        {
            base.OnDeactive(owner);
            owner.EventBus?.RemoveListener(destructionCallback);
            currentOwner = null;
        }

        private void OnGearDestroyed(GearDestroyedEvent evt)
        {
            if (isDead || currentOwner == null || RaceContext == null) return;

            // If some OTHER gear died
            if (evt.Position != currentOwner.Position)
            {
                Debug.Log($"[MartyrGear] Witnessed destruction of gear at {evt.Position}. Committing sympathy sacrifice!");
                ApplyModifier(currentOwner, targetVariable, martyrBuffValue); // applied permanently!
                isDead = true;
                currentOwner.EventBus?.Raise(new GearDestroyedEvent(currentOwner.Position));
            }
        }
    }
}

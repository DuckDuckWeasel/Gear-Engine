using UnityEngine;

namespace Game.GearEngine
{
    public abstract class GearAbilitySO : ScriptableObject
    {
        public virtual void OnActive(IGridNode owner) { }
        public virtual void Tick(IGridNode owner, float deltaTime) { }
        public virtual void OnDeactive(IGridNode owner) { }
        public abstract void Execute(IGridNode owner);
    }
}

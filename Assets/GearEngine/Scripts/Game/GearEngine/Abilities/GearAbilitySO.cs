using System.Linq;
using System.Reflection;
using System.Text;
using GearEngine.GearEngine.Services.Inventory;
using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    public abstract class GearAbilitySO : ScriptableObject, IDescribable
    {
        public virtual void OnActive(IGridNode owner) { }
        public virtual void Tick(IGridNode owner, float deltaTime) { }
        public virtual void OnDeactive(IGridNode owner) { }
        public abstract void Execute(IGridNode owner);

        public virtual string GetRichTextDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<size=11><b><i>{this.GetType().Name.Replace("AbilitySO", "").Replace("Gear", "")}</i></b></size>");
            
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null || f.IsPublic);
            
            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                string valStr = val != null ? val.ToString() : "None";
                sb.AppendLine($"<color=#aaaaaa>{f.Name}:</color> <color=#eeeeee>{valStr}</color>");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
                if (val == null) continue;

                string readableName = FormatFieldName(f.Name);
                string formattedValue = FormatFieldValue(f.Name, val);

                sb.AppendLine($"<color=#aaaaaa>{readableName}:</color> {formattedValue}");
            }
            return sb.ToString().TrimEnd();
        }

        private string FormatFieldName(string fieldName)
        {
            if (fieldName == "targ" || fieldName == "targetVariable" || fieldName == "burstTarget") return "Target Stat";
            
            var text = Regex.Replace(fieldName, "([A-Z])", " $1").Trim();
            if (string.IsNullOrEmpty(text)) return fieldName;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private string FormatFieldValue(string fieldName, object val)
        {
            if (val is ScriptableObject so)
            {
                return $"<color=#A335EE>{so.name.Replace("VariableSO", "").Replace("Variable", "").Trim()}</color>";
            }

            string lowerName = fieldName.ToLower();
            bool isTime = lowerName.Contains("duration") || lowerName.Contains("interval") || lowerName.Contains("time");

            if (val is float fVal)
            {
                if (isTime) return $"<color=#00CCFF>{fVal:F1}s</color>";
                if (fVal > 0 && !lowerName.Contains("threshold") && !lowerName.Contains("capacity")) return $"<color=#1EFF00>+{fVal:F0}</color>";
                if (fVal < 0) return $"<color=#FF8C00>{fVal:F0}</color>";
                return $"<color=#00CCFF>{fVal:F0}</color>";
            }

            if (val is int iVal)
            {
                if (isTime) return $"<color=#00CCFF>{iVal}s</color>";
                if (iVal > 0 && !lowerName.Contains("threshold") && !lowerName.Contains("capacity") && !lowerName.Contains("count")) return $"<color=#1EFF00>+{iVal}</color>";
                if (iVal < 0) return $"<color=#FF8C00>{iVal}</color>";
                return $"<color=#00CCFF>{iVal}</color>";
            }
            
            return $"<color=#EEEEEE>{val}</color>";
        }
    }
}

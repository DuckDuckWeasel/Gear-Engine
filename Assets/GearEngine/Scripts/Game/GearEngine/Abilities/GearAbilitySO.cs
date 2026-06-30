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
            var values = new System.Collections.Generic.List<string>();
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null || f.IsPublic).ToList();
            
            // Find target names if any
            string mainTargetName = "";
            foreach (var f in fields)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(f.FieldType))
                {
                    var targetSo = f.GetValue(this) as ScriptableObject;
                    if (targetSo != null && !string.IsNullOrEmpty(targetSo.name))
                    {
                        mainTargetName = targetSo.name.Replace("VariableSO", "").Replace("Variable", "").Trim();
                        break;
                    }
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) && f.FieldType.IsGenericType && typeof(ScriptableObject).IsAssignableFrom(f.FieldType.GetGenericArguments()[0]))
                {
                    var list = f.GetValue(this) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            var targetSo = item as ScriptableObject;
                            if (targetSo != null && !string.IsNullOrEmpty(targetSo.name))
                            {
                                mainTargetName = "Cycle Stats"; // Or targetSo.name if we only want the first one
                                break;
                            }
                        }
                    }
                }
            }

            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val == null) continue;

                // Ignore ScriptableObjects like variables (they are usually targets/types, not the value itself)
                if (val is ScriptableObject) continue;

                if (!ShouldShowInDescription(f.Name)) continue;

                // Skip empty enumerables to avoid "Cycle Stats: <color><i></i></color>"
                if (val is System.Collections.IEnumerable enumerable && !(val is string))
                {
                    bool isEmpty = true;
                    foreach (var item in enumerable) { isEmpty = false; break; }
                    if (isEmpty) continue;
                }

                string readableName = FormatFieldName(f.Name);
                
                // Inject mainTargetName into generic names if it exists
                if (!string.IsNullOrEmpty(mainTargetName))
                {
                    if (readableName.Contains("Buff Val")) readableName = readableName.Replace("Buff Val", $"{mainTargetName} Buff");
                    else if (readableName.Contains("Buff Value")) readableName = readableName.Replace("Buff Value", $"{mainTargetName} Buff");
                    else if (readableName.Contains("Penalty Amount")) readableName = readableName.Replace("Penalty Amount", $"{mainTargetName} Penalty");
                    else if (readableName == "Amount" || readableName == "Value" || readableName == "Val") readableName = $"{mainTargetName} {readableName}";
                }
                else
                {
                    readableName = readableName.Replace(" Value", "").Replace(" Val", "").Replace(" Amount", "").Replace(" Multiplier", "").Replace("Value", "").Replace("Val", "").Replace("Amount", "").Replace("Multiplier", "");
                }

                string formattedValue = FormatFieldValue(f.Name, val);
                
                if (!string.IsNullOrEmpty(formattedValue))
                {
                    if (val is System.Collections.IEnumerable && !(val is string))
                    {
                        values.Add($"{readableName}: {formattedValue}");
                    }
                    else
                    {
                        values.Add($"{formattedValue} {readableName}");
                    }
                }
            }
            return string.Join("\n", values);
        }

        public virtual string GetFloatingTextDescription()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null || f.IsPublic).ToList();
            
            // Find target names if any
            string mainTargetName = "";
            foreach (var f in fields)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(f.FieldType))
                {
                    var targetSo = f.GetValue(this) as ScriptableObject;
                    if (targetSo != null && !string.IsNullOrEmpty(targetSo.name))
                    {
                        mainTargetName = targetSo.name.Replace("VariableSO", "").Replace("Variable", "").Trim();
                        break;
                    }
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) && f.FieldType.IsGenericType && typeof(ScriptableObject).IsAssignableFrom(f.FieldType.GetGenericArguments()[0]))
                {
                    var list = f.GetValue(this) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            var targetSo = item as ScriptableObject;
                            if (targetSo != null && !string.IsNullOrEmpty(targetSo.name))
                            {
                                mainTargetName = "Cycle Stats"; // Or targetSo.name if we only want the first one
                                break;
                            }
                        }
                    }
                }
            }

            string primaryValueStr = "";
            var otherStats = new System.Collections.Generic.List<string>();

            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val == null) continue;
                if (val is ScriptableObject) continue;

                if (!ShouldShowInDescription(f.Name)) continue;

                // Skip empty enumerables to avoid "Cycle Stats: <color><i></i></color>"
                if (val is System.Collections.IEnumerable enumerable && !(val is string))
                {
                    bool isEmpty = true;
                    foreach (var item in enumerable) { isEmpty = false; break; }
                    if (isEmpty) continue;
                }

                string formattedValue = FormatFieldValue(f.Name, val);
                if (string.IsNullOrEmpty(formattedValue)) continue;

                string lowerName = f.Name.ToLower();
                string readableName = FormatFieldName(f.Name);

                bool isDuration = lowerName.Contains("duration") || lowerName.Contains("time") || lowerName.Contains("interval");
                bool isPrimaryValue = lowerName.Contains("val") || lowerName.Contains("amount") || lowerName.Contains("multiplier") || lowerName.Contains("boost");

                if (isDuration)
                {
                    // For floating text, we don't include duration string, because it's baked into float animation
                    continue;
                }
                else if (isPrimaryValue && string.IsNullOrEmpty(primaryValueStr))
                {
                    primaryValueStr = formattedValue;
                    if (string.IsNullOrEmpty(mainTargetName))
                    {
                        mainTargetName = readableName.Replace(" Value", "").Replace(" Val", "").Replace(" Amount", "").Replace(" Multiplier", "").Replace("Value", "").Replace("Val", "").Replace("Amount", "").Replace("Multiplier", "").Trim();
                        if (string.IsNullOrEmpty(mainTargetName)) mainTargetName = readableName;
                    }
                }
                else
                {
                    if (val is System.Collections.IEnumerable && !(val is string))
                    {
                        otherStats.Add($"{readableName}: {formattedValue}");
                    }
                    else
                    {
                        otherStats.Add($"{formattedValue} {readableName}");
                    }
                }
            }

            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(primaryValueStr))
                sb.Append(primaryValueStr);
                
            if (!string.IsNullOrEmpty(mainTargetName))
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(mainTargetName);
            }

            foreach(var stat in otherStats)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(stat);
            }

            return sb.ToString().Trim();
        }

        public virtual float GetDuration()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in fields)
            {
                string lowerName = f.Name.ToLower();
                if (lowerName.Contains("duration") || lowerName.Contains("time") || lowerName.Contains("interval"))
                {
                    var val = f.GetValue(this);
                    if (val is float fVal) return fVal;
                    if (val is int iVal) return iVal;
                }
            }
            return 0f;
        }

        protected virtual bool ShouldShowInDescription(string fieldName)
        {
            string lowerName = fieldName.ToLower();
            if (lowerName.Contains("rate") || 
                lowerName.Contains("threshold") || 
                lowerName.Contains("time") && !lowerName.Contains("duration") ||
                lowerName.Contains("max") || 
                lowerName.Contains("min") || 
                lowerName == "targ" || 
                lowerName == "targetvariable" || 
                lowerName == "bursttarget") 
            {
                return false;
            }
            return true;
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
                return $"<color=#A335EE><i>{so.name.Replace("VariableSO", "").Replace("Variable", "").Trim()}</i></color>";
            }

            if (val is System.Collections.IEnumerable enumerable && !(val is string))
            {
                var stringList = new System.Collections.Generic.List<string>();
                foreach (var item in enumerable)
                {
                    if (item is ScriptableObject s)
                    {
                        stringList.Add(s.name.Replace("VariableSO", "").Replace("Variable", "").Trim());
                    }
                    else if (item != null)
                    {
                        stringList.Add(item.ToString());
                    }
                }
                return $"<color=#A335EE><i>{string.Join(", ", stringList)}</i></color>";
            }

            string lowerName = fieldName.ToLower();
            bool isTime = lowerName.Contains("duration") || lowerName.Contains("interval") || lowerName.Contains("time");

            if (val is float fVal)
            {
                if (isTime) return $"<color=#00CCFF><i>{fVal:F1}s</i></color>";
                if (fVal > 0 && !lowerName.Contains("threshold") && !lowerName.Contains("capacity")) return $"<color=#1EFF00><i>+{fVal:F0}</i></color>";
                if (fVal < 0) return $"<color=#FF0000><i>{fVal:F0}</i></color>";
                return $"<color=#00CCFF><i>{fVal:F0}</i></color>";
            }

            if (val is int iVal)
            {
                if (isTime) return $"<color=#00CCFF><i>{iVal}s</i></color>";
                if (iVal > 0 && !lowerName.Contains("threshold") && !lowerName.Contains("capacity") && !lowerName.Contains("count")) return $"<color=#1EFF00><i>+{iVal}</i></color>";
                if (iVal < 0) return $"<color=#FF0000><i>{iVal}</i></color>";
                return $"<color=#00CCFF><i>{iVal}</i></color>";
            }
            
            return $"<color=#EEEEEE><i>{val}</i></color>";
        }
    }
}

using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityVariableValueSerializer : IVariableValueSerializer
    {
        public UnityVariableValueSerializer(IBlackboardLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly IBlackboardLogger logger;
        private readonly string nullValue = "__BLACKBOARD_NULL__";

        public string Serialize(Type valueType, object value)
        {
            try
            {
                return SerializeValue(valueType ?? throw new ArgumentNullException(nameof(valueType)), value);
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to serialize Blackboard value type '{valueType}'.", exception);
                throw;
            }
        }

        public object Deserialize(Type valueType, string serializedValue)
        {
            try
            {
                return DeserializeValue(valueType ?? throw new ArgumentNullException(nameof(valueType)), serializedValue);
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to deserialize Blackboard value type '{valueType}'.", exception);
                throw;
            }
        }

        private string SerializeValue(Type valueType, object value)
        {
            if (value == null)
            {
                return nullValue;
            }

            RejectUnityObject(valueType);
            if (valueType == typeof(string))
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes((string)value));
            }

            return IsInvariantValue(valueType) ? SerializeInvariant(valueType, value) : JsonUtility.ToJson(value);
        }

        private object DeserializeValue(Type valueType, string serializedValue)
        {
            if (serializedValue == nullValue)
            {
                return null;
            }

            RejectUnityObject(valueType);
            if (valueType == typeof(string))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(serializedValue));
            }

            return IsInvariantValue(valueType) ? DeserializeInvariant(valueType, serializedValue) : JsonUtility.FromJson(serializedValue, valueType);
        }

        private void RejectUnityObject(Type valueType)
        {
            if (typeof(Object).IsAssignableFrom(valueType))
            {
                throw new NotSupportedException($"Unity object variable type '{valueType.FullName}' requires a project-specific persistence service.");
            }
        }

        private bool IsInvariantValue(Type valueType)
        {
            return valueType.IsPrimitive || valueType.IsEnum || valueType == typeof(decimal) || valueType == typeof(DateTime) || valueType == typeof(Guid);
        }

        private string SerializeInvariant(Type valueType, object value)
        {
            if (valueType == typeof(DateTime))
            {
                return ((DateTime)value).ToString("O", CultureInfo.InvariantCulture);
            }

            return valueType == typeof(Guid) ? value.ToString() : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private object DeserializeInvariant(Type valueType, string value)
        {
            if (valueType.IsEnum)
            {
                return Enum.Parse(valueType, value);
            }

            if (valueType == typeof(DateTime))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            return valueType == typeof(Guid) ? Guid.Parse(value) : Convert.ChangeType(value, valueType, CultureInfo.InvariantCulture);
        }
    }
}

using System;

namespace Scaffold.VisualScripting
{
    public interface IVariableValueSerializer
    {
        string Serialize(Type valueType, object value);

        object Deserialize(Type valueType, string serializedValue);
    }
}

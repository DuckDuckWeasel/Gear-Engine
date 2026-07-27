using UnityEngine;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Implemented by IActions that require a MonoBehaviour context to function 
    /// (e.g. for starting Coroutines or managing Unity lifecycles).
    /// </summary>
    public interface IMonoBehaviourConsumer
    {
        void SetHost(MonoBehaviour host);
    }
}

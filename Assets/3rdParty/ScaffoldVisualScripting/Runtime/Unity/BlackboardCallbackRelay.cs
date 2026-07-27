using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public abstract class BlackboardCallbackRelay : MonoBehaviour
    {
        public BlackboardBehaviour Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField] private BlackboardBehaviour target;

        protected void Forward(string messageName, object payload = null)
        {
            if (target != null)
            {
                target.TrySendBlackboardMessage(messageName, payload);
            }
        }
    }
}

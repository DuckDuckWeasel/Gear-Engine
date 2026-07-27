using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class RenderCallbackRelay : BlackboardCallbackRelay
    {
        [SerializeField] private string becameVisibleMessage = "BecameVisible";
        [SerializeField] private string becameInvisibleMessage = "BecameInvisible";
        [SerializeField] private string willRenderMessage = "WillRenderObject";

        private void OnBecameVisible()
        {
            Forward(becameVisibleMessage);
        }

        private void OnBecameInvisible()
        {
            Forward(becameInvisibleMessage);
        }

        private void OnWillRenderObject()
        {
            Forward(willRenderMessage, Camera.current);
        }
    }
}

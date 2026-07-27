using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class PhysicsCallbackRelay : BlackboardCallbackRelay
    {
        [SerializeField] private string collisionEnterMessage = "CollisionEnter";
        [SerializeField] private string collisionStayMessage = "CollisionStay";
        [SerializeField] private string collisionExitMessage = "CollisionExit";
        [SerializeField] private string triggerEnterMessage = "TriggerEnter";
        [SerializeField] private string triggerStayMessage = "TriggerStay";
        [SerializeField] private string triggerExitMessage = "TriggerExit";
        [SerializeField] private string collisionEnter2DMessage = "CollisionEnter2D";
        [SerializeField] private string collisionStay2DMessage = "CollisionStay2D";
        [SerializeField] private string collisionExit2DMessage = "CollisionExit2D";
        [SerializeField] private string triggerEnter2DMessage = "TriggerEnter2D";
        [SerializeField] private string triggerStay2DMessage = "TriggerStay2D";
        [SerializeField] private string triggerExit2DMessage = "TriggerExit2D";

        private void OnCollisionEnter(Collision collision)
        {
            Forward(collisionEnterMessage, collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            Forward(collisionStayMessage, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            Forward(collisionExitMessage, collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            Forward(triggerEnterMessage, other);
        }

        private void OnTriggerStay(Collider other)
        {
            Forward(triggerStayMessage, other);
        }

        private void OnTriggerExit(Collider other)
        {
            Forward(triggerExitMessage, other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Forward(collisionEnter2DMessage, collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            Forward(collisionStay2DMessage, collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Forward(collisionExit2DMessage, collision);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Forward(triggerEnter2DMessage, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Forward(triggerStay2DMessage, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Forward(triggerExit2DMessage, other);
        }
    }
}


using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// The block will execute when the player is dragging an object which starts touching the target object.
    ///
    /// ExecuteAlways used to get the Compatibility that we need, use of ISerializationCallbackReceiver is error prone
    /// when used on Unity controlled objects as it runs on threads other than main thread.
    /// </summary>
    [EventHandlerInfo("Sprite",
                      "Drag Entered",
                      "The block will execute when the player is dragging an object which starts touching the target object.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class DragEntered : EventHandler
    {
        public class DragEnteredEvent
        {
            public Draggable2D DraggableObject;
            public Collider2D TargetCollider;

            public DragEnteredEvent(Draggable2D draggableObject, Collider2D targetCollider)
            {
                DraggableObject = draggableObject;
                TargetCollider = targetCollider;
            }
        }

        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable draggableRef;

        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable targetRef;

        [SerializeField] protected List<Draggable2D> draggableObjects = new List<Draggable2D>();

        [SerializeField] protected List<Collider2D> targetObjects = new List<Collider2D>();

        protected EventDispatcher eventDispatcher;

        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
            {
                eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

                eventDispatcher.AddListener<DragEnteredEvent>(OnDragEnteredEvent);
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                eventDispatcher.RemoveListener<DragEnteredEvent>(OnDragEnteredEvent);

                eventDispatcher = null;
            }
        }

        private void OnDragEnteredEvent(DragEnteredEvent evt)
        {
            OnDragEntered(evt.DraggableObject, evt.TargetCollider);
        }



        #region Public members

        /// <summary>
        /// Called by the Draggable2D object when the the drag enters the drag target.
        /// </summary>
        public virtual void OnDragEntered(Draggable2D draggableObject, Collider2D targetObject)
        {
            if (draggableObject.BeingDragged &&
                this.targetObjects != null && this.draggableObjects != null &&
                this.draggableObjects.Contains(draggableObject) &&
                this.targetObjects.Contains(targetObject))
            {
                if (draggableRef != null)
                {
                    draggableRef.Value = draggableObject.gameObject;
                }
                if (targetRef != null)
                {
                    targetRef.Value = targetObject.gameObject;
                }
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            string summary = "Draggable: ";
            if (this.draggableObjects != null && this.draggableObjects.Count != 0)
            {
                for (int i = 0; i < this.draggableObjects.Count; i++)
                {
                    if (draggableObjects[i] != null)
                    {
                        summary += draggableObjects[i].name + ",";
                    }
                }
            }

            summary += "\nTarget: ";
            if (this.targetObjects != null && this.targetObjects.Count != 0)
            {
                for (int i = 0; i < this.targetObjects.Count; i++)
                {
                    if (targetObjects[i] != null)
                    {
                        summary += targetObjects[i].name + ",";
                    }
                }
            }

            if (summary.Length == 0)
            {
                return "None";
            }

            return summary;
        }

        #endregion Public members
    }
}
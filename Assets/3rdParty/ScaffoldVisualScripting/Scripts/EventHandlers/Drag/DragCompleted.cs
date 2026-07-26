
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// The block will execute when the player drags an object and successfully drops it on a target object.
    ///
    /// ExecuteAlways used to get the Compatibility that we need, use of ISerializationCallbackReceiver is error prone
    /// when used on Unity controlled objects as it runs on threads other than main thread.
    /// </summary>
    [EventHandlerInfo("Sprite",
                      "Drag Completed",
                      "The block will execute when the player drags an object and successfully drops it on a target object.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class DragCompleted : EventHandler
    {
        public class DragCompletedEvent
        {
            public Draggable2D DraggableObject;

            public DragCompletedEvent(Draggable2D draggableObject)
            {
                DraggableObject = draggableObject;
            }
        }

        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable draggableRef;

        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable targetRef;

        [SerializeField] protected List<Draggable2D> draggableObjects = new List<Draggable2D>();

        [SerializeField] protected List<Collider2D> targetObjects = new List<Collider2D>();

        // There's no way to poll if an object is touching another object, so
        // we have to listen to the callbacks and track the touching state ourselves.
        protected bool overTarget = false;

        protected Collider2D targetCollider = null;

        protected EventDispatcher eventDispatcher;

        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
            {
                eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

                eventDispatcher.AddListener<DragCompletedEvent>(OnDragCompletedEvent);
                eventDispatcher.AddListener<DragEntered.DragEnteredEvent>(OnDragEnteredEvent);
                eventDispatcher.AddListener<DragExited.DragExitedEvent>(OnDragExitedEvent);

                foreach (Draggable2D dragObj in draggableObjects)
                {
                    dragObj.RegisterHandler(this);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                eventDispatcher.RemoveListener<DragCompletedEvent>(OnDragCompletedEvent);
                eventDispatcher.RemoveListener<DragEntered.DragEnteredEvent>(OnDragEnteredEvent);
                eventDispatcher.RemoveListener<DragExited.DragExitedEvent>(OnDragExitedEvent);

                foreach (Draggable2D dragObj in draggableObjects)
                {
                    dragObj.UnregisterHandler(this);
                }

                eventDispatcher = null;
            }
        }

        private void OnDragCompletedEvent(DragCompletedEvent evt)
        {
            OnDragCompleted(evt.DraggableObject);
        }

        private void OnDragEnteredEvent(DragEntered.DragEnteredEvent evt)
        {
            OnDragEntered(evt.DraggableObject, evt.TargetCollider);
        }

        private void OnDragExitedEvent(DragExited.DragExitedEvent evt)
        {
            OnDragExited(evt.DraggableObject, evt.TargetCollider);
        }



        #region Public members

        /// <summary>
        /// Gets the draggable object.
        /// </summary>
        public virtual List<Draggable2D> DraggableObjects { get { return draggableObjects; } }

        /// <summary>
        /// Returns true if the draggable object is over the drag target object.
        /// </summary>
        public virtual bool IsOverTarget()
        {
            return overTarget;
        }

        /// <summary>
        /// Called by the Draggable2D object when the it enters the drag target.
        /// </summary>
        public virtual void OnDragEntered(Draggable2D draggableObject, Collider2D targetObject)
        {
            if (draggableObject.BeingDragged &&
                this.targetObjects != null && this.draggableObjects != null &&
                this.draggableObjects.Contains(draggableObject) &&
                this.targetObjects.Contains(targetObject))
            {
                overTarget = true;
                targetCollider = targetObject;
            }
        }

        /// <summary>
        /// Called by the Draggable2D object when the it exits the drag target.
        /// </summary>
        public virtual void OnDragExited(Draggable2D draggableObject, Collider2D targetObject)
        {
            if (draggableObject.BeingDragged &&
                this.targetObjects != null && this.draggableObjects != null &&
                this.draggableObjects.Contains(draggableObject) &&
                this.targetObjects.Contains(targetObject))
            {
                overTarget = false;
                targetCollider = null;
            }
        }

        /// <summary>
        /// Called by the Draggable2D object when the the drag ends over the drag target.
        /// </summary>
        public virtual void OnDragCompleted(Draggable2D draggableObject)
        {
            if (this.draggableObjects.Contains(draggableObject) &&
                overTarget)
            {
                // Assume that the player will have to do perform another drag and drop operation
                // to complete the drag again. This is necessary because we don't get an OnDragExited if the
                // draggable object is set to be inactive.
                if (draggableRef != null)
                {
                    draggableRef.Value = draggableObject.gameObject;
                }
                if (targetRef != null)
                {
                    targetRef.Value = targetCollider.gameObject;
                }

                overTarget = false;
                targetCollider = null;

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
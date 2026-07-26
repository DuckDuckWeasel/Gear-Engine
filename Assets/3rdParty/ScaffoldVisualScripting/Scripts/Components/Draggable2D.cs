
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Detects drag and drop interactions on a Game Object, and sends events to all Blackboard event handlers in the scene.
    /// The Game Object must have Collider2D & RigidBody components attached. 
    /// The Collider2D must have the Is Trigger property set to true.
    /// The RigidBody would typically have the Is Kinematic property set to true, unless you want the object to move around using physics.
    /// Use in conjunction with the Drag Started, Drag Completed, Drag Cancelled, Drag Entered & Drag Exited event handlers.
    /// </summary>
    public class Draggable2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Is object dragging enabled")]
        [SerializeField] protected bool dragEnabled = true;

        [Tooltip("Move object back to its starting position when drag is cancelled")]
        [FormerlySerializedAs("returnToStartPos")]
        [SerializeField] protected bool returnOnCancelled = true;

        [Tooltip("Move object back to its starting position when drag is completed")]
        [SerializeField] protected bool returnOnCompleted = true;

        [Tooltip("Time object takes to return to its starting position")]
        [SerializeField] protected float returnDuration = 1f;

        [Tooltip("Mouse texture to use when hovering mouse over object")]
        [SerializeField] protected Texture2D hoverCursor;



        [SerializeField] protected bool beingDragged;

        public virtual bool BeingDragged
        {
            get { return beingDragged; }
            set { beingDragged = value; }
        }

        protected Vector3 startingPosition;
        protected bool updatePosition = false;
        protected Vector3 newPosition;
        protected Vector3 delta = Vector3.zero;

        #region DragCompleted handlers
        protected List<DragCompleted> dragCompletedHandlers = new List<DragCompleted>();

        public void RegisterHandler(DragCompleted handler)
        {
            dragCompletedHandlers.Add(handler);
        }

        public void UnregisterHandler(DragCompleted handler)
        {
            if (dragCompletedHandlers.Contains(handler))
            {
                dragCompletedHandlers.Remove(handler);
            }
        }
        #endregion

        protected virtual void LateUpdate()
        {
            // iTween will sometimes override the object position even if it should only be affecting the scale, rotation, etc.
            // To make sure this doesn't happen, we force the position change to happen in LateUpdate.
            if (updatePosition)
            {
                transform.position = newPosition;
                updatePosition = false;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!dragEnabled)
            {
                return;
            }

            EventDispatcher eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

            eventDispatcher.Raise(new DragEntered.DragEnteredEvent(this, other));
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!dragEnabled)
            {
                return;
            }

            EventDispatcher eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

            eventDispatcher.Raise(new DragExited.DragExitedEvent(this, other));
        }

        protected virtual void DoBeginDrag()
        {
            beingDragged = true;

            // Offset the object so that the drag is anchored to the exact point where the user clicked it
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            delta = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 10f)) - transform.position;
            delta.z = 0f;

            startingPosition = transform.position;

            EventDispatcher eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

            eventDispatcher.Raise(new DragStarted.DragStartedEvent(this));
        }

        protected virtual void DoDrag()
        {
            if (!dragEnabled)
            {
                return;
            }

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            float z = transform.position.z;

            newPosition = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 10f)) - delta;
            newPosition.z = z;
            updatePosition = true;
        }

        protected virtual void DoEndDrag()
        {
            if (!dragEnabled)
            {
                return;
            }

            EventDispatcher eventDispatcher = ScaffoldManager.Instance.EventDispatcher;
            bool dragCompleted = false;

            for (int i = 0; i < dragCompletedHandlers.Count; i++)
            {
                DragCompleted handler = dragCompletedHandlers[i];
                if (handler != null && handler.DraggableObjects.Contains(this))
                {
                    if (handler.IsOverTarget())
                    {
                        dragCompleted = true;

                        eventDispatcher.Raise(new DragCompleted.DragCompletedEvent(this));
                    }
                }
            }

            if (!dragCompleted)
            {
                eventDispatcher.Raise(new DragCancelled.DragCancelledEvent(this));

                if (returnOnCancelled)
                {
                    LeanTween.move(gameObject, startingPosition, returnDuration).setEase(LeanTweenType.easeOutExpo);
                }
            }
            else if (returnOnCompleted)
            {
                LeanTween.move(gameObject, startingPosition, returnDuration).setEase(LeanTweenType.easeOutExpo);
            }

            beingDragged = false;
        }

        protected virtual void DoPointerEnter()
        {
            ChangeCursor(hoverCursor);
        }

        protected virtual void DoPointerExit()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, UnityEngine.CursorMode.Auto);
        }

        protected virtual void ChangeCursor(Texture2D cursorTexture)
        {
            if (!dragEnabled)
            {
                return;
            }

            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }



        #region Public members

        /// <summary>
        /// Is object drag and drop enabled.
        /// </summary>
        /// <value><c>true</c> if drag enabled; otherwise, <c>false</c>.</value>
        public virtual bool DragEnabled { get { return dragEnabled; } set { dragEnabled = value; } }

        #endregion

        #region IBeginDragHandler implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            DoBeginDrag();
        }

        #endregion

        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            DoDrag();
        }

        #endregion

        #region IEndDragHandler implementation

        public void OnEndDrag(PointerEventData eventData)
        {
            DoEndDrag();
        }

        #endregion

        #region IPointerEnterHandler implementation

        public void OnPointerEnter(PointerEventData eventData)
        {
            DoPointerEnter();
        }

        #endregion

        #region IPointerExitHandler implementation

        public void OnPointerExit(PointerEventData eventData)
        {
            DoPointerExit();
        }

        #endregion
    }
}

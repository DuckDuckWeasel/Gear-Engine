
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scaffold
{
    /// <summary>
    /// Detects mouse clicks and touches on a Game Object, and sends an event to all Blackboard event handlers in the scene.
    /// The Game Object must have a Collider or Collider2D component attached.
    /// Use in conjunction with the ObjectClicked Blackboard event handler.
    /// </summary>
    public class Clickable2D : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Is object clicking enabled")]
        [SerializeField] protected bool clickEnabled = true;

        [Tooltip("Mouse texture to use when hovering mouse over object")]
        [SerializeField] protected Texture2D hoverCursor;



        protected virtual void ChangeCursor(Texture2D cursorTexture)
        {
            if (!clickEnabled)
            {
                return;
            }

            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }

        protected virtual void DoPointerClick()
        {
            if (!clickEnabled)
            {
                return;
            }

            EventDispatcher eventDispatcher = ScaffoldManager.Instance.EventDispatcher;

            eventDispatcher.Raise(new ObjectClicked.ObjectClickedEvent(this));
        }

        protected virtual void DoPointerEnter()
        {
            ChangeCursor(hoverCursor);
        }

        protected virtual void DoPointerExit()
        {
            // Always reset the mouse cursor to be on the safe side
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, UnityEngine.CursorMode.Auto);
        }



        #region Public members

        /// <summary>
        /// Is object clicking enabled.
        /// </summary>
        public bool ClickEnabled { set { clickEnabled = value; } }

        #endregion

        #region IPointerClickHandler implementation

        public void OnPointerClick(PointerEventData eventData)
        {
            DoPointerClick();
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


using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Used by the Blackboard window to serialize the currently active Blackboard object
    /// so that the same Blackboard can be displayed while editing & playing.
    /// </summary>
    [AddComponentMenu("")]
    public class ScaffoldState : MonoBehaviour
    {
        [SerializeField] protected Blackboard selectedBlackboard;

        #region Public members

        /// <summary>
        /// The currently selected Blackboard.
        /// </summary>
        public virtual Blackboard SelectedBlackboard { get { return selectedBlackboard; } set { selectedBlackboard = value; } }

        #endregion
    }
}
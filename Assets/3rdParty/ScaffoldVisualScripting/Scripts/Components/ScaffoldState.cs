
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Used by the Flowchart window to serialize the currently active Flowchart object
    /// so that the same Flowchart can be displayed while editing & playing.
    /// </summary>
    [AddComponentMenu("")]
    public class ScaffoldState : MonoBehaviour
    {
        [SerializeField] protected Flowchart selectedFlowchart;

        #region Public members

        /// <summary>
        /// The currently selected Flowchart.
        /// </summary>
        public virtual Flowchart SelectedFlowchart { get { return selectedFlowchart; } set { selectedFlowchart = value; } }

        #endregion
    }
}
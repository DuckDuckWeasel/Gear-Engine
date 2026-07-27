using Scaffold;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Container for a TutorialController variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct TutorialProgressControllerData
    {
        [SerializeField]
        public TutorialProgressController tutorialControllerVal;

        [SerializeField]
        [VariableProperty("<Value>", typeof(TutorialProgressControllerVariable))]
        public TutorialProgressControllerVariable tutorialControllerRef;

        public TutorialProgressControllerData(TutorialProgressController v)
        {
            tutorialControllerVal = v;
            tutorialControllerRef = null;
        }

        public TutorialProgressController Value
        {
            get { return (tutorialControllerRef == null) ? tutorialControllerVal : tutorialControllerRef.Value; }
            set { if (tutorialControllerRef == null) { tutorialControllerVal = value; } else { tutorialControllerRef.Value = value; } }
        }

        public static implicit operator TutorialProgressController(TutorialProgressControllerData tutorialProgressControllerData)
        {
            return tutorialProgressControllerData.Value;
        }

        public string GetDescription()
        {
            if (tutorialControllerRef == null)
            {
                return tutorialControllerVal != null ? tutorialControllerVal.ToString() : "Null";
            }
            else
            {
                return tutorialControllerRef.Key;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.Tutorial.Data
{
    [CreateAssetMenu(fileName = "Tutorial", menuName = "Scaffold/Tutorial")]
    public class TutorialSO : ScriptableObject
    {
        [SerializeField]
        private string id;
        public string Id => id;

        [SerializeField]
        private Controllers.TutorialProgressController tutorialProgressController;
        public Controllers.TutorialProgressController TutorialProgressController => tutorialProgressController;

        [SerializeField]
        private TutorialSO nextTutorial;
        public TutorialSO NextTutorial => nextTutorial;

        [SerializeField]
        private List<TutorialSO> unlockTutorials = new List<TutorialSO>();
        public IReadOnlyList<TutorialSO> UnlockTutorials => unlockTutorials;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                string[] splitedName = name.Split('_');
                id = splitedName.Length > 0 ? splitedName[0] : name;
            }
        }
    }
}

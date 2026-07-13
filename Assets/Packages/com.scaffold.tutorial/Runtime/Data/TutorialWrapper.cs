using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.Tutorial.Data
{
    [CreateAssetMenu(fileName = "TutorialWrapper", menuName = "Scaffold/TutorialWrapper")]
    public class TutorialWrapper : ScriptableObject
    {
        [SerializeField]
        private List<TutorialSO> tutorials = new List<TutorialSO>();
        public IReadOnlyList<TutorialSO> Tutorials => tutorials;

        [SerializeField]
        public List<TutorialSO> StartTutorials = new List<TutorialSO>();

        [SerializeField]
        public List<TutorialSO> BattleTutorials = new List<TutorialSO>();

        public TutorialSO GetTutorialReference(string id)
        {
            return tutorials.Find(x => x.Id == id);
        }
    }
}

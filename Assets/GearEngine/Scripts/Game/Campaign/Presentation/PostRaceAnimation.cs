using OM.Animora.Runtime;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class PostRaceAnimation : MonoBehaviour
    {
        [SerializeField] private AnimoraPlayer[] players = System.Array.Empty<AnimoraPlayer>();

        public void Play()
        {
            foreach (AnimoraPlayer player in players)
            {
                if (player == null || !player.gameObject.activeInHierarchy)
                {
                    continue;
                }

                player.StopAnimation();
                player.PlayAnimation();
                player.Evaluate(0f);
            }
        }

        public void Stop()
        {
            foreach (AnimoraPlayer player in players)
            {
                if (player != null)
                {
                    player.StopAnimation();
                }
            }
        }

        private void OnDisable() => Stop();
    }
}

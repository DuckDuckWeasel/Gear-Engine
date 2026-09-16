using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultStandingRowView : MonoBehaviour
    {
        public RaceStandingEntry Entry { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Image background;
        [SerializeField] private Sprite playerBackground;
        [SerializeField] private Sprite rivalBackground;
        public void Bind(RaceStandingEntry entry, int position)
        {
            Entry = entry;
            nameText.text = entry.Name;
            timeText.text = entry.FormattedTime;
            background.sprite = entry.IsPlayer ? playerBackground : rivalBackground;
            background.color = Color.white;
            SetPosition(position);
        }

        public void SetPosition(int position)
        {
            positionText.text = position.ToString();
        }
    }
}

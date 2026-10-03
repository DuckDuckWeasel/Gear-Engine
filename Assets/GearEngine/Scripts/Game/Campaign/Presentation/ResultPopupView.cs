using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupView : View<ResultPopupViewModel>
    {
        private const string k_starFillName = "ProgressFill";

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text trackText;
        [SerializeField] private Image[] stars;
        [SerializeField] private TMP_Text[] starTargetLabels;
        [SerializeField] private Sprite earnedStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button continueButton;
        [SerializeField] private ResultStandingsView standings;
        [SerializeField] private PostRaceAnimation screenAnimation;
        private UiPatternParallax backgroundParallax;

        protected override void OnBind()
        {
            titleText.text = viewModel.VictoryTitle;
            scoreText.text = viewModel.Score.ToString();
            trackText.text = "FINAL POSITIONS";
            standings.Bind(viewModel.Standings);
            UpdateStarFills();
            UpdateStarTargetScores();

            continueButton.onClick.AddListener(viewModel.Continue);
        }

        protected override void OnUnbind()
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            if (backgroundParallax != null)
            {
                backgroundParallax.enabled = false;
            }
            screenAnimation.Stop();
            standings.Stop();
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);

            base.OnUnbind();
        }

        protected override void OnOpen(bool wasHidden)
        {
            backgroundParallax ??= UiPatternParallax.Attach(transform);
            if (backgroundParallax != null)
            {
                backgroundParallax.enabled = true;
            }
            continueButton.onClick.RemoveListener(viewModel.Continue);
            continueButton.onClick.AddListener(viewModel.Continue);
            screenAnimation.Play();
            standings.Play();
        }

        protected override void OnClose(bool hiding)
        {
            if (backgroundParallax != null)
            {
                backgroundParallax.enabled = false;
            }
            continueButton.onClick.RemoveListener(viewModel.Continue);
            screenAnimation.Stop();
            standings.Stop();
        }

        private void OnDestroy()
        {
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);
        }

        private void UpdateStarTargetScores()
        {
            for (int i = 0; i < starTargetLabels.Length; i++)
            {
                bool hasTarget = i < viewModel.StarTargetScores.Count && viewModel.StarTargetScores[i] > 0;
                starTargetLabels[i].gameObject.SetActive(hasTarget);
                starTargetLabels[i].text = hasTarget ? viewModel.StarTargetScores[i].ToString() : string.Empty;
            }
        }

        private void UpdateStarFills()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                Image star = stars[i];
                bool hasTarget = i < viewModel.StarTargetScores.Count && viewModel.StarTargetScores[i] > 0;
                star.gameObject.SetActive(hasTarget);
                if (!hasTarget)
                {
                    continue;
                }

                star.sprite = emptyStar;
                star.preserveAspect = true;
                star.raycastTarget = false;
                Image fill = GetOrCreateStarFill(star);
                fill.sprite = earnedStar;
                fill.fillAmount = StarProgressionMath.Fill(viewModel.StarTargetScores, i, viewModel.Score);
            }
        }

        private static Image GetOrCreateStarFill(Image star)
        {
            Transform existing = star.transform.Find(k_starFillName);
            if (existing != null && existing.TryGetComponent(out Image existingFill))
            {
                return existingFill;
            }

            GameObject fillObject = new GameObject(k_starFillName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform fillTransform = (RectTransform)fillObject.transform;
            fillTransform.SetParent(star.rectTransform, false);
            fillTransform.anchorMin = Vector2.zero;
            fillTransform.anchorMax = Vector2.one;
            fillTransform.anchoredPosition = Vector2.zero;
            fillTransform.sizeDelta = Vector2.zero;

            Image fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.preserveAspect = true;
            fill.raycastTarget = false;
            return fill;
        }
    }
}

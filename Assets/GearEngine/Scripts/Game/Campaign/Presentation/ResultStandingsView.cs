using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultStandingsView : MonoBehaviour
    {
        public int DisplayedPlayerPosition { get; private set; }
        public bool IsAnimating => animation != null;

        [SerializeField] private ResultStandingRowView[] rows;
        [SerializeField] private float rowSpacing = 187f;
        [SerializeField] private float swapDuration = 0.4f;
        private RaceStandingsModel standings;
        private List<ResultStandingRowView> order;
        private Coroutine animation;

        public void Bind(RaceStandingsModel model)
        {
            standings = model;
        }

        public void Play()
        {
            Stop();
            ResetRows();
            DisplayedPlayerPosition = standings.PreviousPosition;
            if (DisplayedPlayerPosition == standings.PlayerPosition)
            {
                ShowFinalRows();
                return;
            }
            animation = StartCoroutine(AnimatePlacement());
        }

        public void ShowCurrentPlacement()
        {
            Stop();
            order = rows.ToList();
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].Bind(standings.Entries[i], i + 1);
                rows[i].Rect.anchoredPosition = Position(i);
            }
            DisplayedPlayerPosition = standings.PlayerPosition;
            ShowFinalRows();
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Stop()
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }

            animation = null;
        }

        private void ResetRows()
        {
            List<RaceStandingEntry> initial = standings.Entries.Where(entry => !entry.IsPlayer).ToList();
            initial.Insert(standings.PreviousPosition - 1, standings.Player);
            order = rows.ToList();
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].gameObject.SetActive(true);
                rows[i].Bind(initial[i], i + 1);
                rows[i].Rect.anchoredPosition = Position(i);
            }
        }

        private IEnumerator AnimatePlacement()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            while (DisplayedPlayerPosition != standings.PlayerPosition)
            {
                int from = DisplayedPlayerPosition - 1;
                int to = from + (DisplayedPlayerPosition > standings.PlayerPosition ? -1 : 1);
                yield return AnimateSwap(from, to);
                CompleteSwap(from, to);
            }
            ShowFinalRows();
            animation = null;
        }

        private IEnumerator AnimateSwap(int from, int to)
        {
            ResultStandingRowView player = order[from];
            ResultStandingRowView displaced = order[to];
            player.transform.SetAsLastSibling();
            float elapsed = 0f;
            while (elapsed < swapDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float blend = Mathf.SmoothStep(0f, 1f, elapsed / swapDuration);
                player.Rect.anchoredPosition = Vector2.Lerp(Position(from), Position(to), blend);
                displaced.Rect.anchoredPosition = Vector2.Lerp(Position(to), Position(from), blend);
                yield return null;
            }
        }

        private void CompleteSwap(int from, int to)
        {
            ResultStandingRowView player = order[from];
            order[from] = order[to];
            order[to] = player;
            player.SetPosition(to + 1);
            order[from].SetPosition(from + 1);
            DisplayedPlayerPosition = to + 1;
        }

        private Vector2 Position(int index)
        {
            return new Vector2(0f, -83.5f - index * rowSpacing);
        }

        private void ShowFinalRows()
        {
            for (int i = 0; i < order.Count; i++)
            {
                order[i].gameObject.SetActive(i < standings.VisibleRowCount);
            }
        }
    }
}

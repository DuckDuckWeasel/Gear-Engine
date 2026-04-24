using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using Unity.Services.DeploymentApi.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>
    /// Unified LiveOps config editor: list builders, edit inline, sync <c>.rc</c>, deploy to UGS.
    /// </summary>
    public sealed class LiveOpsConfigsWindow : EditorWindow
    {
        /// <summary>Created in <see cref="CreateGUI"/> — must not be constructed in field initializers (EditorWindow is a ScriptableObject).</summary>
        private ConfigDetailView _detailView;

        private IRemoteDeployer _deployer;

        private Label _duplicateBanner;

        private Label _envLabel;

        private List<LiveOpsConfigDiscovery.Row> _rows = new List<LiveOpsConfigDiscovery.Row>();

        private ListView _listView;

        private Toolbar _toolbar;

        /// <summary>For tests: replace the default deployer.</summary>
        internal IRemoteDeployer DeployerOverride
        {
            set => _deployer = value ?? new RemoteDeployer();
        }

        [MenuItem("Window/LiveOps/Configs")]
        public static void OpenConfigs()
        {
            LiveOpsConfigsWindow w = GetWindow<LiveOpsConfigsWindow>();
            w.titleContent = new GUIContent("LiveOps Configs");
            w.minSize = new Vector2(720f, 480f);
        }

        [MenuItem("Window/LiveOps/Config Deployment")]
        public static void OpenLegacyMenu()
        {
            OpenConfigs();
        }

        public void CreateGUI()
        {
            _deployer ??= new RemoteDeployer();

            rootVisualElement.style.flexGrow = 1;

            _duplicateBanner = new Label
            {
                style =
                {
                    display = DisplayStyle.None,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new Color(0.9f, 0.3f, 0.2f),
                    marginLeft = 6,
                    marginRight = 6,
                    marginTop = 4,
                    marginBottom = 4,
                },
            };
            rootVisualElement.Add(_duplicateBanner);

            _toolbar = new Toolbar();

            _toolbar.Add(
                new ToolbarButton(RefreshAll)
                {
                    text = "Refresh",
                    tooltip = "Rescan the project for config builder assets and refresh sync status in the list.",
                });

            _toolbar.Add(
                new ToolbarButton(PullAll)
                {
                    text = "Pull All",
                    tooltip = "Run Pull on every builder from its local .rc file (skipped when duplicate ConfigKeys exist).",
                });

            _toolbar.Add(
                new ToolbarButton(() => _ = DeploySelectedAsync())
                {
                    text = "Deploy Selected",
                    tooltip = "Save, regenerate .rc from the selected builder, then deploy that file to the current UGS environment.",
                });

            _toolbar.Add(
                new ToolbarButton(() => _ = DeployAllAsync())
                {
                    text = "Deploy All",
                    tooltip = "Save, regenerate every builder’s .rc, then deploy all of those files (skipped when duplicate ConfigKeys exist).",
                });

            _envLabel = new Label { style = { marginLeft = 8, unityTextAlign = TextAnchor.MiddleLeft } };
            _toolbar.Add(_envLabel);

            rootVisualElement.Add(_toolbar);

            var split = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1 },
            };

            _listView = new ListView
            {
                selectionType = SelectionType.Single,
                fixedItemHeight = 24,
                style = { flexGrow = 1 },
            };

            _listView.makeItem = () =>
            {
                var rowEl = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        paddingLeft = 4,
                        paddingRight = 4,
                    },
                };
                var light = new Image { name = "list-status-light" };
                light.style.width = 14;
                light.style.height = 14;
                light.style.marginRight = 6;
                light.style.flexShrink = 0;
                light.scaleMode = ScaleMode.ScaleToFit;
                var nameLabel = new Label { name = "list-config-name", style = { flexGrow = 1 } };
                rowEl.Add(light);
                rowEl.Add(nameLabel);
                return rowEl;
            };
            _listView.bindItem = (element, index) =>
            {
                var rowEl = (VisualElement)element;
                Image light = rowEl.Q<Image>("list-status-light");
                Label nameLabel = rowEl.Q<Label>("list-config-name");
                if (index < 0 || index >= _rows.Count)
                {
                    if (light != null)
                    {
                        light.image = null;
                        light.tooltip = string.Empty;
                    }

                    if (nameLabel != null)
                    {
                        nameLabel.text = string.Empty;
                        nameLabel.tooltip = string.Empty;
                    }

                    return;
                }

                LiveOpsConfigDiscovery.Row row = _rows[index];
                RowStatus st = RcSyncService.GetStatus(row.Builder);
                bool dup = row.IsDuplicateConfigKey;
                if (light != null)
                {
                    LiveOpsConfigStatusLights.ApplyToImage(light, st, dup);
                }

                if (nameLabel != null)
                {
                    nameLabel.text = row.Builder.ConfigKey;
                    nameLabel.tooltip = LiveOpsConfigStatusLights.StatusTooltip(st, dup);
                }
            };

            _listView.itemsSource = _rows;
            _listView.selectionChanged += _ => RebindDetail();

            _detailView = new ConfigDetailView();

            split.Add(_listView);
            split.Add(_detailView);

            rootVisualElement.Add(split);

            RefreshAll();
        }

        private void OnEnable()
        {
            if (_listView == null)
            {
                return;
            }

            RefreshAll();
        }

        internal void RefreshAll()
        {
            _rows = LiveOpsConfigDiscovery.DiscoverAllRows();
            int dup = _rows.Count(r => r.IsDuplicateConfigKey);
            if (_duplicateBanner != null)
            {
                if (dup > 0)
                {
                    int keys = _rows.GroupBy(r => r.Builder.ConfigKey).Count(g => g.Count() > 1);
                    _duplicateBanner.text =
                        $"{keys} duplicate ConfigKey group(s) ({dup} assets) — fix before Deploy.";
                    _duplicateBanner.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _duplicateBanner.style.display = DisplayStyle.None;
                }
            }

            string env = Deployments.Instance?.EnvironmentProvider?.Current;
            if (_envLabel != null)
            {
                _envLabel.text = string.IsNullOrEmpty(env) ? "Env: (none)" : $"Env: {env}";
            }

            if (_listView != null)
            {
                _listView.itemsSource = _rows;
                _listView.RefreshItems();
                if (_rows.Count > 0 && GetSelectedRowIndex() < 0)
                {
                    _listView.SetSelectionWithoutNotify(new[] { 0 });
                }

                RebindDetail();
            }

            UpdateBatchToolbarState();
        }

        private void UpdateBatchToolbarState()
        {
            bool hasDup = _rows.Any(r => r.IsDuplicateConfigKey);
            if (_toolbar == null)
            {
                return;
            }

            foreach (VisualElement ve in _toolbar.Children())
            {
                if (ve is ToolbarButton tb
                    && (tb.text == "Pull All" || tb.text == "Deploy All"))
                {
                    tb.SetEnabled(!hasDup);
                }
            }
        }

        private int GetSelectedRowIndex()
        {
            if (_listView == null)
            {
                return -1;
            }

            return _listView.selectedIndex;
        }

        private void RebindDetail()
        {
            if (_detailView == null)
            {
                return;
            }

            int i = GetSelectedRowIndex();
            LiveOpsConfigDiscovery.Row row = i >= 0 && i < _rows.Count ? _rows[i] : null;
            bool actionsOk = row != null && !row.IsDuplicateConfigKey;
            _deployer ??= new RemoteDeployer();
            _detailView.Rebind(row, actionsOk, _deployer, RefreshAll, this);
        }

        private void PullAll()
        {
            if (_rows.Any(r => r.IsDuplicateConfigKey))
            {
                Debug.LogError("[LiveOps Config] Resolve duplicate ConfigKey assets before Pull All.");
                return;
            }

            try
            {
                foreach (LiveOpsConfigDiscovery.Row row in _rows)
                {
                    RcSyncService.Pull(row.Builder);
                }

                RefreshAll();
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Pull All failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task DeploySelectedAsync()
        {
            int i = GetSelectedRowIndex();
            if (i < 0 || i >= _rows.Count)
            {
                Debug.LogWarning("[LiveOps Config] Select a row to deploy.");
                return;
            }

            LiveOpsConfigDiscovery.Row row = _rows[i];
            if (row.IsDuplicateConfigKey)
            {
                Debug.LogError("[LiveOps Config] Cannot deploy a duplicate ConfigKey row.");
                return;
            }

            try
            {
                AssetDatabase.SaveAssets();
                RcSyncService.Sync(row.Builder);
                RefreshAll();
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Sync before deploy failed: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            await DeployPathsAsync(new[] { RcSyncService.GetRcPath(row.Builder) });
        }

        private async Task DeployAllAsync()
        {
            if (_rows.Any(r => r.IsDuplicateConfigKey))
            {
                Debug.LogError("[LiveOps Config] Resolve duplicate ConfigKey assets before Deploy All.");
                return;
            }

            try
            {
                AssetDatabase.SaveAssets();
                foreach (LiveOpsConfigDiscovery.Row r in _rows)
                {
                    RcSyncService.Sync(r.Builder);
                }

                RefreshAll();
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Sync before deploy failed: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            List<string> paths = _rows.Select(r => RcSyncService.GetRcPath(r.Builder)).ToList();
            await DeployPathsAsync(paths);
        }

        private async Task DeployPathsAsync(IReadOnlyList<string> paths)
        {
            try
            {
                DeployOutcome outcome = await _deployer.DeployAsync(paths, CancellationToken.None);
                if (outcome.AllSucceeded)
                {
                    Debug.Log($"[LiveOps Config] Deploy finished ({outcome.Transport}): {outcome.Message}");
                }
                else
                {
                    Debug.LogError($"[LiveOps Config] Deploy reported failure ({outcome.Transport}): {outcome.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Deploy exception: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

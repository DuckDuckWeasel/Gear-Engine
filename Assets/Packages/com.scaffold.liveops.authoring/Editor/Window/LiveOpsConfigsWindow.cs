using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using Unity.Services.Core.Editor.Environments;
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

        private PropertyChangedEventHandler _environmentPropertyHandler;

        private List<LiveOpsConfigDiscovery.Row> _rows = new List<LiveOpsConfigDiscovery.Row>();

        private ListView _listView;

        private Toolbar _toolbar;

        private VisualElement _busyOverlay;

        private Label _busyStatusLabel;

        private bool _isDeployBusy;

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

        public void CreateGUI()
        {
            _deployer ??= new RemoteDeployer();

            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.position = Position.Relative;

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

            var envBlock = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexShrink = 0,
                    paddingLeft = 6,
                    paddingRight = 4,
                },
            };

            _envLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 2,
                },
            };
            envBlock.Add(_envLabel);
            _toolbar.Add(envBlock);

            _toolbar.Add(
                new ToolbarSpacer
                {
                    style = { flexGrow = 1, flexShrink = 1 },
                });

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
                    tooltip = "Save, regenerate .rc from the selected builder, then run ugs deploy. CLI auth is separate from the Editor: run ugs login once in a terminal (service account), or set UGS_CLI_SERVICE_KEY_ID / UGS_CLI_SERVICE_SECRET_KEY. Linked project + environment: Project Settings → Services.",
                });

            _toolbar.Add(
                new ToolbarButton(() => _ = DeployAllAsync())
                {
                    text = "Deploy All",
                    tooltip = "Save, regenerate every builder’s .rc, then run ugs deploy for each (same CLI auth as Deploy Selected). Skipped when duplicate ConfigKeys exist.",
                });

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

            _busyOverlay = new VisualElement
            {
                pickingMode = PickingMode.Position,
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    display = DisplayStyle.None,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    backgroundColor = new Color(0.05f, 0.05f, 0.06f, 0.55f),
                },
            };

            var busyCard = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 18,
                    paddingRight = 18,
                    paddingTop = 14,
                    paddingBottom = 14,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(0.35f, 0.35f, 0.38f),
                    borderRightColor = new Color(0.35f, 0.35f, 0.38f),
                    borderTopColor = new Color(0.35f, 0.35f, 0.38f),
                    borderBottomColor = new Color(0.35f, 0.35f, 0.38f),
                    backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(0.16f, 0.16f, 0.17f, 0.98f)
                        : new Color(0.96f, 0.96f, 0.97f, 0.98f),
                    maxWidth = 560,
                },
            };

            var spinner = new Label("\u21bb")
            {
                style =
                {
                    fontSize = 22,
                    marginRight = 12,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    rotate = new Rotate(0),
                },
            };
            spinner.AddToClassList("liveops-deploy-spinner");
            busyCard.Add(spinner);

            _busyStatusLabel = new Label("Working…")
            {
                style =
                {
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    color = EditorGUIUtility.isProSkin ? Color.white : new Color(0.12f, 0.12f, 0.14f),
                },
            };
            busyCard.Add(_busyStatusLabel);

            _busyOverlay.Add(busyCard);
            rootVisualElement.Add(_busyOverlay);

            _busyOverlay.schedule.Execute(RotateBusySpinner).Every(50L);

            RefreshAll();
        }

        private float _spinnerAngle;

        private void RotateBusySpinner()
        {
            if (_busyOverlay == null
                || _busyOverlay.style.display == DisplayStyle.None
                || _busyOverlay.childCount == 0)
            {
                return;
            }

            VisualElement card = _busyOverlay[0];
            if (card == null || card.childCount == 0)
            {
                return;
            }

            if (card[0] is not Label spin)
            {
                return;
            }

            _spinnerAngle += 18f;
            if (_spinnerAngle >= 360f)
            {
                _spinnerAngle = 0f;
            }

            spin.style.rotate = new Rotate(_spinnerAngle);
        }

        private void OnEnable()
        {
            SubscribeEnvironmentApi();
            if (_listView == null)
            {
                return;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            UnsubscribeEnvironmentApi();
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

            ApplyEnvironmentLabel();

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

            ApplyToolbarEnabledState();
        }

        private void ApplyToolbarEnabledState()
        {
            bool hasDup = _rows.Any(r => r.IsDuplicateConfigKey);
            if (_toolbar == null)
            {
                return;
            }

            foreach (VisualElement ve in _toolbar.Children())
            {
                if (ve is ToolbarButton tb)
                {
                    bool batchRestricted = tb.text == "Pull All" || tb.text == "Deploy All";
                    tb.SetEnabled(!_isDeployBusy && (!batchRestricted || !hasDup));
                }
            }
        }

        private void SetDeployBusy(bool busy, string statusText = null)
        {
            _isDeployBusy = busy;
            if (_busyOverlay != null)
            {
                _busyOverlay.style.display = busy ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_busyStatusLabel != null && !string.IsNullOrEmpty(statusText))
            {
                _busyStatusLabel.text = statusText;
            }

            ApplyToolbarEnabledState();
            if (_listView != null)
            {
                _listView.SetEnabled(!busy);
            }

            _detailView?.SetDeployChromeLocked(busy);
        }

        private void ScheduleBusyStatusText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (_busyStatusLabel != null && _isDeployBusy)
                {
                    _busyStatusLabel.text = text;
                }
            };
        }

        internal async Task RunDeployWorkflowAsync(Action preparationStep, IReadOnlyList<string> paths)
        {
            _deployer ??= new RemoteDeployer();
            SetDeployBusy(true, "Preparing deploy…");
            try
            {
                try
                {
                    preparationStep();
                }
                catch (Exception ex)
                {
                    if (_busyStatusLabel != null)
                    {
                        _busyStatusLabel.text = "Save/sync failed — see Console.";
                    }

                    Debug.LogError($"[LiveOps Config] Sync before deploy failed: {ex.Message}\n{ex.StackTrace}");
                    return;
                }

                RefreshAll();
                Repaint();

                var progress = new System.Progress<string>(ScheduleBusyStatusText);
                DeployOutcome outcome = await _deployer.DeployAsync(paths, CancellationToken.None, progress);
                if (_busyStatusLabel != null)
                {
                    _busyStatusLabel.text = outcome.AllSucceeded
                        ? "Deploy finished."
                        : "Deploy finished with errors — see Console.";
                }

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
                if (_busyStatusLabel != null)
                {
                    _busyStatusLabel.text = "Deploy failed — see Console.";
                }

                Debug.LogError($"[LiveOps Config] Deploy exception: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                EditorApplication.delayCall += () => SetDeployBusy(false);
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
            if (_isDeployBusy)
            {
                _detailView.SetDeployChromeLocked(true);
            }

            SynchronizeListViewStatusLights();
        }

        /// <summary>
        /// <see cref="ListView"/> can leave recycled rows showing stale data after
        /// <see cref="ListView.RefreshItems()"/>; per-index <see cref="ListView.RefreshItem(int)"/>
        /// forces <c>bindItem</c> to run with the current <see cref="RcSyncService.GetStatus"/>,
        /// so list lights match the detail title.
        /// </summary>
        private void SynchronizeListViewStatusLights()
        {
            RefreshListViewItemBindings();

            if (rootVisualElement != null && rootVisualElement.panel != null)
            {
                rootVisualElement.schedule.Execute(RefreshListViewItemBindings).ExecuteLater(0L);
            }
        }

        private void RefreshListViewItemBindings()
        {
            if (_listView == null || _rows == null)
            {
                return;
            }

            for (int j = 0; j < _rows.Count; j++)
            {
                _listView.RefreshItem(j);
            }
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

            string path = RcSyncService.GetRcPath(row.Builder);
            await RunDeployWorkflowAsync(
                () =>
                {
                    AssetDatabase.SaveAssets();
                    RcSyncService.Sync(row.Builder);
                },
                new[] { path });
        }

        private async Task DeployAllAsync()
        {
            if (_rows.Any(r => r.IsDuplicateConfigKey))
            {
                Debug.LogError("[LiveOps Config] Resolve duplicate ConfigKey assets before Deploy All.");
                return;
            }

            List<string> paths = _rows.Select(r => RcSyncService.GetRcPath(r.Builder)).ToList();
            await RunDeployWorkflowAsync(
                () =>
                {
                    AssetDatabase.SaveAssets();
                    foreach (LiveOpsConfigDiscovery.Row r in _rows)
                    {
                        RcSyncService.Sync(r.Builder);
                    }
                },
                paths);
        }

        private void SubscribeEnvironmentApi()
        {
            UnsubscribeEnvironmentApi();
            try
            {
                IEnvironmentsApi api = EnvironmentsApi.Instance;
                if (api is INotifyPropertyChanged npc)
                {
                    _environmentPropertyHandler = OnEnvironmentApiPropertyChanged;
                    npc.PropertyChanged += _environmentPropertyHandler;
                }
            }
            catch
            {
                // Services may not be initialized yet (e.g. first frame).
            }
        }

        private void UnsubscribeEnvironmentApi()
        {
            if (_environmentPropertyHandler == null)
            {
                return;
            }

            try
            {
                IEnvironmentsApi api = EnvironmentsApi.Instance;
                if (api is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= _environmentPropertyHandler;
                }
            }
            catch
            {
            }

            _environmentPropertyHandler = null;
        }

        private void OnEnvironmentApiPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null
                || string.IsNullOrEmpty(e.PropertyName)
                || e.PropertyName == "ActiveEnvironmentName"
                || e.PropertyName == "ActiveEnvironmentId"
                || e.PropertyName == "Environments")
            {
                ApplyEnvironmentLabel();
            }
        }

        private void ApplyEnvironmentLabel()
        {
            if (_envLabel == null)
            {
                return;
            }

            (string line, string tip) = BuildEnvironmentDisplay();
            _envLabel.text = line;
            _envLabel.tooltip = tip;
        }

        private static (string line, string tooltip) BuildEnvironmentDisplay()
        {
            const string defaultTip =
                "UGS environment for deploy and Remote Config. Change it in Edit → Project Settings → Services, or use the environment selector in the Services window when signed in.";

            string deployEnvId = null;
            try
            {
                deployEnvId = Deployments.Instance?.EnvironmentProvider?.Current;
            }
            catch
            {
            }

            string displayName = null;
            IEnvironmentsApi envApi = null;
            try
            {
                envApi = EnvironmentsApi.Instance;
                displayName = envApi?.ActiveEnvironmentName;
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(deployEnvId) && envApi?.Environments != null)
            {
                try
                {
                    if (Guid.TryParse(deployEnvId, out Guid gid))
                    {
                        foreach (EnvironmentInfo info in envApi.Environments)
                        {
                            if (info.Id == gid)
                            {
                                displayName = info.Name;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(deployEnvId))
            {
                return ($"Environment: {deployEnvId}", defaultTip);
            }

            if (string.IsNullOrEmpty(displayName))
            {
                return ("Environment: (not linked)", defaultTip);
            }

            if (!string.IsNullOrEmpty(deployEnvId) && !string.Equals(deployEnvId, displayName, StringComparison.Ordinal)
                && !string.Equals(deployEnvId, displayName, StringComparison.OrdinalIgnoreCase))
            {
                return ($"Environment: {displayName}", $"{defaultTip}\nDeployment id: {deployEnvId}");
            }

            return ($"Environment: {displayName}", defaultTip);
        }
    }
}

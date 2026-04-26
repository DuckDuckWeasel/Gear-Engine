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
    /// Unified LiveOps config editor: list builders, edit inline, sync <c>.rc</c> / <c>.gor</c>, deploy to UGS.
    /// </summary>
    public sealed class LiveOpsConfigsWindow : EditorWindow
    {
        private const int MainTabConfigs = 0;

        private const int MainTabProfiles = 1;

        private ConfigDetailView _detailView;

        private IRemoteDeployer _deployer;

        private Label _duplicateBanner;

        private Label _envLabel;

        private PropertyChangedEventHandler _environmentPropertyHandler;

        private List<LiveOpsConfigDiscovery.Row> _rows = new List<LiveOpsConfigDiscovery.Row>();

        private List<LiveOpsConfigDiscovery.VariantListItem> _variantListItems = new List<LiveOpsConfigDiscovery.VariantListItem>();

        private List<ConfigProfileDiscovery.Row> _profileRows = new List<ConfigProfileDiscovery.Row>();

        private int _mainTabIndex;

        private Button _mainTabConfigsButton;

        private Button _mainTabProfilesButton;

        private VisualElement _configTabRoot;

        private VisualElement _profileTabRoot;

        private ListView _configListView;

        private ListView _profileListView;

        private TwoPaneSplitView _configSplit;

        private TwoPaneSplitView _profileSplit;

        private VisualElement _profileDetailHost;

        private TextField _profileJexlPreview;

        private readonly List<UnityEditor.Editor> _profileEditors = new List<UnityEditor.Editor>();

        private Toolbar _toolbar;

        private VisualElement _busyOverlay;

        private Label _busyStatusLabel;

        private bool _isDeployBusy;

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
            _mainTabIndex = MainTabConfigs;

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

            _mainTabConfigsButton = new ToolbarButton(() => SelectMainTab(MainTabConfigs)) { text = "Configs" };
            _mainTabProfilesButton = new ToolbarButton(() => SelectMainTab(MainTabProfiles)) { text = "Profiles" };
            StyleMainTabButton(_mainTabConfigsButton, true);
            StyleMainTabButton(_mainTabProfilesButton, false);
            _mainTabConfigsButton.style.marginRight = 2;
            _mainTabProfilesButton.style.marginLeft = 2;
            _toolbar.Add(_mainTabConfigsButton);
            _toolbar.Add(_mainTabProfilesButton);

            _toolbar.Add(
                new ToolbarSpacer
                {
                    style = { width = 8, flexShrink = 0 },
                });

            _toolbar.Add(
                new ToolbarButton(RefreshAll)
                {
                    text = "Refresh",
                    tooltip = "Rescan the project for config builder and profile assets and refresh status.",
                });

            _toolbar.Add(
                new ToolbarButton(PullAll)
                {
                    text = "Pull All",
                    tooltip = "Run Pull on every builder variant (skipped when duplicate (ConfigKey, Profile) exist).",
                });

            _toolbar.Add(
                new ToolbarButton(() => _ = DeploySelectedAsync())
                {
                    text = "Deploy Selected",
                    tooltip = "Sync then ugs deploy for the selected variant or profile. CLI auth: ugs login or UGS_CLI_SERVICE_KEY_* .",
                });

            _toolbar.Add(
                new ToolbarButton(() => _ = DeployAllAsync())
                {
                    text = "Deploy All",
                    tooltip = "Sync all local files, then ugs deploy each .rc and .gor (skipped on duplicate variants).",
                });

            rootVisualElement.Add(_toolbar);

            _configTabRoot = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
            _profileTabRoot = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column,
                    display = DisplayStyle.None,
                },
            };

            _configSplit = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1 },
            };

            _configListView = new ListView
            {
                selectionType = SelectionType.Single,
                fixedItemHeight = 24,
                style = { flexGrow = 1 },
            };

            _configListView.makeItem = MakeConfigListItem;
            _configListView.bindItem = BindConfigListItem;
            _configListView.itemsSource = _variantListItems;
            _configListView.selectionChanged += _ => RebindConfigDetailIfNeeded();

            _detailView = new ConfigDetailView();
            _configSplit.Add(_configListView);
            _configSplit.Add(_detailView);
            _configTabRoot.Add(_configSplit);

            _profileSplit = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1, display = DisplayStyle.None },
            };

            _profileListView = new ListView
            {
                selectionType = SelectionType.Single,
                fixedItemHeight = 24,
                style = { flexGrow = 1 },
            };

            _profileListView.makeItem = () =>
            {
                var el = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        paddingLeft = 4,
                        paddingRight = 4,
                    },
                };
                el.Add(
                    new Label
                    {
                        name = "profile-id-label",
                        style = { flexGrow = 1 },
                    });
                return el;
            };
            _profileListView.bindItem = (el, i) =>
            {
                var lab = el.Q<Label>("profile-id-label");
                if (lab == null || i < 0 || i >= _profileRows.Count)
                {
                    if (lab != null)
                    {
                        lab.text = string.Empty;
                    }

                    return;
                }

                ConfigProfileDiscovery.Row r = _profileRows[i];
                if (r.Profile == null)
                {
                    lab.text = "(null)";
                    return;
                }

                string suffix = r.Profile.IsDefault ? "  (default)" : string.Empty;
                lab.text = r.Profile.ProfileId + suffix;
                lab.tooltip = r.AssetPath;
            };
            _profileListView.itemsSource = _profileRows;
            _profileListView.selectionChanged += _ => RebindProfileDetailIfNeeded();

            _profileDetailHost = new ScrollView { style = { flexGrow = 1 } };
            _profileJexlPreview = new TextField("JEXL (preview)")
            {
                multiline = true,
                isReadOnly = true,
            };
            _profileJexlPreview.style.minHeight = 120;
            _profileSplit.Add(_profileListView);
            _profileSplit.Add(_profileDetailHost);
            _profileTabRoot.Add(_profileSplit);

            rootVisualElement.Add(_configTabRoot);
            rootVisualElement.Add(_profileTabRoot);

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

        private static void StyleMainTabButton(Button b, bool active)
        {
            b.style.height = 22;
            b.style.paddingLeft = 10;
            b.style.paddingRight = 10;
            b.style.borderTopLeftRadius = 3;
            b.style.borderTopRightRadius = 3;
            b.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
            bool pro = EditorGUIUtility.isProSkin;
            b.style.backgroundColor = active
                ? (pro ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.9f, 0.9f, 0.9f))
                : (pro ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.82f, 0.82f, 0.82f));
        }

        private void SelectMainTab(int tab)
        {
            _mainTabIndex = tab;
            bool isConfigs = tab == MainTabConfigs;
            if (_configTabRoot != null)
            {
                _configTabRoot.style.display = isConfigs ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_profileTabRoot != null)
            {
                _profileTabRoot.style.display = isConfigs ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (_mainTabConfigsButton != null)
            {
                StyleMainTabButton(_mainTabConfigsButton, isConfigs);
            }

            if (_mainTabProfilesButton != null)
            {
                StyleMainTabButton(_mainTabProfilesButton, !isConfigs);
            }

            if (isConfigs)
            {
                if (_profileSplit != null)
                {
                    _profileSplit.style.display = DisplayStyle.None;
                }

                RebindConfigDetailIfNeeded();
            }
            else
            {
                if (_profileSplit != null)
                {
                    _profileSplit.style.display = DisplayStyle.Flex;
                }

                _detailView?.Rebind(null, false, _deployer ?? new RemoteDeployer(), RefreshAll, this);
                RebindProfileDetailIfNeeded();
            }
        }

        private static VisualElement MakeConfigListItem()
        {
            var rowEl = new VisualElement
            {
                name = "variant-row",
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
        }

        private void BindConfigListItem(VisualElement element, int index)
        {
            var rowEl = (VisualElement)element;
            Image light = rowEl.Q<Image>("list-status-light");
            Label nameLabel = rowEl.Q<Label>("list-config-name");
            if (index < 0 || index >= _variantListItems.Count)
            {
                if (light != null)
                {
                    light.image = null;
                    light.tooltip = string.Empty;
                    light.style.display = DisplayStyle.Flex;
                }

                if (nameLabel != null)
                {
                    nameLabel.text = string.Empty;
                    nameLabel.tooltip = string.Empty;
                }

                return;
            }

            LiveOpsConfigDiscovery.VariantListItem item = _variantListItems[index];
            if (item.IsGroup)
            {
                if (light != null)
                {
                    light.image = null;
                    light.style.display = DisplayStyle.None;
                }

                if (nameLabel != null)
                {
                    nameLabel.text = item.GroupConfigKey;
                    nameLabel.unityFontStyleAndWeight = FontStyle.Bold;
                    nameLabel.style.paddingLeft = 0;
                    nameLabel.style.color = Color.white;
                    if (!EditorGUIUtility.isProSkin)
                    {
                        nameLabel.style.color = new Color(0.12f, 0.12f, 0.14f);
                    }

                    nameLabel.tooltip = "Config key group";
                }

                return;
            }

            if (light != null)
            {
                light.style.display = DisplayStyle.Flex;
            }

            LiveOpsConfigDiscovery.Row vRow = item.VariantRow;
            if (vRow == null || vRow.Builder == null)
            {
                if (light != null)
                {
                    light.image = null;
                }

                if (nameLabel != null)
                {
                    nameLabel.text = string.Empty;
                }

                return;
            }

            RowStatus st = RcSyncService.GetStatus(vRow.Builder);
            bool dup = vRow.IsDuplicateVariant;
            if (light != null)
            {
                LiveOpsConfigStatusLights.ApplyToImage(light, st, dup);
            }

            if (nameLabel != null)
            {
                nameLabel.unityFontStyleAndWeight = FontStyle.Normal;
                nameLabel.style.paddingLeft = 12;
                nameLabel.text = vRow.Builder.ProfileId
                    + (vRow.Builder.IsDefaultVariant ? "  (default)" : string.Empty);
                nameLabel.tooltip = LiveOpsConfigStatusLights.StatusTooltip(st, dup) + "\n" + vRow.AssetPath;
            }
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
            if (_configListView == null)
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
            _variantListItems = LiveOpsConfigDiscovery.BuildVariantListItems(_rows);
            _profileRows = ConfigProfileDiscovery.DiscoverAll();
            int dup = _rows.Count(r => r.IsDuplicateVariant);
            if (_duplicateBanner != null)
            {
                if (dup > 0)
                {
                    int keyGroups = _rows
                        .GroupBy(r => r.Builder.ConfigKey + "\u0001" + r.Builder.ProfileId)
                        .Count(g => g.Count() > 1);
                    _duplicateBanner.text =
                        $"{keyGroups} duplicate (ConfigKey, Profile) group(s) ({dup} assets) — fix before Deploy.";
                    _duplicateBanner.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _duplicateBanner.style.display = DisplayStyle.None;
                }
            }

            ApplyEnvironmentLabel();

            if (_configListView != null)
            {
                _configListView.itemsSource = _variantListItems;
                _configListView.RefreshItems();
                if (GetSelectedVariantRow() == null && _variantListItems.Count > 0)
                {
                    int firstVariant = 0;
                    for (int i = 0; i < _variantListItems.Count; i++)
                    {
                        if (!_variantListItems[i].IsGroup)
                        {
                            firstVariant = i;
                            break;
                        }
                    }

                    _configListView.SetSelectionWithoutNotify(new[] { firstVariant });
                }

                RebindConfigDetailIfNeeded();
            }

            if (_profileListView != null)
            {
                _profileListView.itemsSource = _profileRows;
                _profileListView.RefreshItems();
                if (_profileRows.Count > 0 && _profileListView.selectedIndex < 0)
                {
                    _profileListView.SetSelectionWithoutNotify(new[] { 0 });
                }

                if (_mainTabIndex == MainTabProfiles)
                {
                    RebindProfileDetailIfNeeded();
                }
            }

            ApplyToolbarEnabledState();
        }

        private void ApplyToolbarEnabledState()
        {
            bool hasDup = _rows.Any(r => r.IsDuplicateVariant);
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
            if (_configListView != null)
            {
                _configListView.SetEnabled(!busy);
            }

            if (_profileListView != null)
            {
                _profileListView.SetEnabled(!busy);
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

        private int GetConfigListSelectedIndex()
        {
            return _configListView != null ? _configListView.selectedIndex : -1;
        }

        private int GetProfileListSelectedIndex()
        {
            return _profileListView != null ? _profileListView.selectedIndex : -1;
        }

        private LiveOpsConfigDiscovery.Row GetSelectedVariantRow()
        {
            int i = GetConfigListSelectedIndex();
            if (i < 0 || i >= _variantListItems.Count)
            {
                return null;
            }

            LiveOpsConfigDiscovery.VariantListItem it = _variantListItems[i];
            return it.IsGroup ? null : it.VariantRow;
        }

        private void RebindConfigDetailIfNeeded()
        {
            if (_mainTabIndex != MainTabConfigs || _detailView == null)
            {
                return;
            }

            LiveOpsConfigDiscovery.Row row = GetSelectedVariantRow();
            bool actionsOk = row != null && !row.IsDuplicateVariant;
            _deployer ??= new RemoteDeployer();
            _detailView.Rebind(row, actionsOk, _deployer, RefreshAll, this);
            if (_isDeployBusy)
            {
                _detailView.SetDeployChromeLocked(true);
            }

            SynchronizeConfigListViewStatusLights();
        }

        private void RebindProfileDetailIfNeeded()
        {
            if (_mainTabIndex != MainTabProfiles || _profileDetailHost == null)
            {
                return;
            }

            ClearProfileEditors();
            _profileDetailHost.Clear();
            int i = GetProfileListSelectedIndex();
            if (i < 0 || i >= _profileRows.Count)
            {
                if (_profileJexlPreview != null)
                {
                    _profileJexlPreview.value = string.Empty;
                }

                return;
            }

            ConfigProfileSO prof = _profileRows[i].Profile;
            if (prof == null)
            {
                return;
            }

            if (_profileJexlPreview != null)
            {
                _profileJexlPreview.value = prof.IsDefault
                    ? "Default profile has no JEXL (Settings only)."
                    : TargetingJexlEmitter.Emit(prof);
                _profileDetailHost.Add(_profileJexlPreview);
            }

            UnityEditor.Editor e = UnityEditor.Editor.CreateEditor(prof);
            _profileEditors.Add(e);
            var imgui = new IMGUIContainer(
                () =>
                {
                    if (e != null && e.target != null)
                    {
                        e.OnInspectorGUI();
                    }
                })
            {
                style = { minHeight = 200f, marginTop = 6 },
            };
            _profileDetailHost.Add(imgui);
        }

        private void ClearProfileEditors()
        {
            foreach (UnityEditor.Editor ed in _profileEditors)
            {
                if (ed != null)
                {
                    UnityEngine.Object.DestroyImmediate(ed);
                }
            }

            _profileEditors.Clear();
        }

        private void SynchronizeConfigListViewStatusLights()
        {
            RefreshConfigListViewItemBindings();

            if (rootVisualElement != null && rootVisualElement.panel != null)
            {
                rootVisualElement.schedule.Execute(RefreshConfigListViewItemBindings).ExecuteLater(0L);
            }
        }

        private void RefreshConfigListViewItemBindings()
        {
            if (_configListView == null || _variantListItems == null)
            {
                return;
            }

            for (int j = 0; j < _variantListItems.Count; j++)
            {
                _configListView.RefreshItem(j);
            }
        }

        private void PullAll()
        {
            if (_rows.Any(r => r.IsDuplicateVariant))
            {
                Debug.LogError("[LiveOps Config] Resolve duplicate (ConfigKey, Profile) before Pull All.");
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
            if (_mainTabIndex == MainTabConfigs)
            {
                LiveOpsConfigDiscovery.Row row = GetSelectedVariantRow();
                if (row == null)
                {
                    Debug.LogWarning("[LiveOps Config] Select a variant row (not a group header) to deploy.");
                    return;
                }

                if (row.IsDuplicateVariant)
                {
                    Debug.LogError("[LiveOps Config] Cannot deploy a duplicate variant row.");
                    return;
                }

                IReadOnlyList<string> paths = RcSyncService.CollectDeployPathsForVariant(row.Builder);
                await RunDeployWorkflowAsync(
                    () =>
                    {
                        AssetDatabase.SaveAssets();
                        RcSyncService.SyncForBuilder(row.Builder);
                    },
                    paths);
            }
            else
            {
                int pi = GetProfileListSelectedIndex();
                if (pi < 0 || pi >= _profileRows.Count)
                {
                    Debug.LogWarning("[LiveOps Config] Select a profile to deploy.");
                    return;
                }

                ConfigProfileSO p = _profileRows[pi].Profile;
                if (p == null)
                {
                    return;
                }

                IReadOnlyList<string> paths = RcSyncService.CollectDeployPathsForProfile(
                    p,
                    out bool skipProfileOnly);
                if (skipProfileOnly)
                {
                    Debug.Log(
                        "[LiveOps Config] This profile is default Settings only — use Variant deploy from the Configs tab, or Deploy All.");
                    return;
                }

                if (paths.Count == 0)
                {
                    Debug.LogWarning("[LiveOps Config] No .gor file for this profile (no non-default variants reference it).");
                    return;
                }

                await RunDeployWorkflowAsync(
                    () =>
                    {
                        AssetDatabase.SaveAssets();
                        RcSyncService.SyncProfileOverride(p, LiveOpsConfigDiscovery.DiscoverAllRows());
                    },
                    paths);
            }
        }

        private async Task DeployAllAsync()
        {
            if (_rows.Any(r => r.IsDuplicateVariant))
            {
                Debug.LogError("[LiveOps Config] Resolve duplicate (ConfigKey, Profile) before Deploy All.");
                return;
            }

            IReadOnlyList<string> paths = RcSyncService.CollectAllDeployPaths(LiveOpsConfigDiscovery.DiscoverAllRows());
            await RunDeployWorkflowAsync(
                () =>
                {
                    AssetDatabase.SaveAssets();
                    RcSyncService.SyncAll(_rows);
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

        private void OnDestroy()
        {
            ClearProfileEditors();
        }
    }
}

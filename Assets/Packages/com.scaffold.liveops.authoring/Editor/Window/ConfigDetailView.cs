using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>Right pane: title bar, Config / Diff tabs, embedded inspectors.</summary>
    internal sealed class ConfigDetailView : VisualElement
    {
        private readonly List<UnityEditor.Editor> _editors = new List<UnityEditor.Editor>();

        private IRemoteDeployer _deployer;

        private Action _refreshChrome;

        private EditorWindow _host;

        private TextField _localDiffField;

        private TextField _remoteDiffField;

        private LiveOpsConfigDiscovery.Row _row;

        private Label _titleStatusLabel;

        private Image _titleStatusLight;

        private Button _findButton;

        private Button _pullButton;

        private Button _deployButton;

        private Button _compareCloudButton;

        private Button _tabConfigButton;

        private Button _tabDiffButton;

        private bool _rowActionsEnabled;

        private VisualElement _tabConfigPage;

        private VisualElement _tabDiffPage;

        public ConfigDetailView()
        {
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Column;
        }

        public void Rebind(
            LiveOpsConfigDiscovery.Row row,
            bool actionsEnabled,
            IRemoteDeployer deployer,
            Action refreshChrome,
            EditorWindow host)
        {
            ClearBoundEditors();
            Clear();
            _localDiffField = null;
            _remoteDiffField = null;
            _findButton = null;
            _pullButton = null;
            _deployButton = null;
            _compareCloudButton = null;
            _tabConfigButton = null;
            _tabDiffButton = null;
            _tabConfigPage = null;
            _tabDiffPage = null;
            _row = row;
            _deployer = deployer;
            _refreshChrome = refreshChrome;
            _host = host;
            _titleStatusLight = null;
            _titleStatusLabel = null;

            if (row == null || row.Builder == null)
            {
                Add(new HelpBox("Select a config.", HelpBoxMessageType.Info));
                return;
            }

            RowStatus disk = RcSyncService.GetStatus(row.Builder);
            bool dup = row.IsDuplicateConfigKey;
            _rowActionsEnabled = actionsEnabled && !dup;

            Add(BuildTitleBar(row, disk, dup, actionsEnabled));
            if (dup)
            {
                Add(
                    new HelpBox(
                        $"Duplicate variant '{row.Builder.ConfigKey}' / profile '{row.Builder.ProfileId}'. "
                        + $"Another builder uses the same (ConfigKey, Profile) pair. Fix before Deploy.\nAsset: {row.AssetPath}",
                        HelpBoxMessageType.Error)
                    {
                        style = { marginTop = 6, marginBottom = 4 },
                    });
            }

            BuildTabbedBody(row, actionsEnabled, dup);
        }

        private VisualElement BuildTitleBar(
            LiveOpsConfigDiscovery.Row row,
            RowStatus disk,
            bool isDuplicate,
            bool actionsEnabled)
        {
            bool pro = EditorGUIUtility.isProSkin;
            Color barBg = pro ? new Color(0.17f, 0.17f, 0.17f) : new Color(0.78f, 0.78f, 0.78f);
            Color borderCol = pro ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.55f, 0.55f, 0.55f);

            var bar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 8,
                    paddingBottom = 8,
                    backgroundColor = barBg,
                    borderBottomWidth = 1,
                    borderBottomColor = borderCol,
                    flexShrink = 0,
                },
            };

            _titleStatusLight = CreateStatusLightImage(disk, isDuplicate);
            bar.Add(_titleStatusLight);

            var titleBlock = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexGrow = 1,
                    marginLeft = 6,
                    flexWrap = Wrap.Wrap,
                },
            };

            var nameLabel = new Label($"{row.Builder.ConfigKey}  ·  {row.Builder.ProfileId}")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginRight = 4,
                },
            };
            titleBlock.Add(nameLabel);

            string statusText = isDuplicate ? "Duplicate variant" : LiveOpsConfigStatusLights.ShortStatusLabel(disk);
            Color statusColor = isDuplicate ? new Color(1f, 0.45f, 0.45f) : StatusToTextColor(disk, pro);
            _titleStatusLabel = new Label($"·  {statusText}")
            {
                style =
                {
                    fontSize = 12,
                    color = statusColor,
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            };
            titleBlock.Add(_titleStatusLabel);

            bar.Add(titleBlock);

            var actions = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                    alignItems = Align.Center,
                    marginLeft = 8,
                },
            };

            bool canAct = actionsEnabled && !isDuplicate;

            Button makeAction(string text, Action onClick, string tooltip)
            {
                var b = new Button(onClick) { text = text, tooltip = tooltip };
                b.SetEnabled(canAct);
                b.style.height = 22;
                b.style.marginLeft = 4;
                return b;
            }

            _findButton = new Button(() => PingConfigAsset(row))
            {
                text = "Find",
                tooltip = "Select and highlight this config asset in the Project window",
            };
            _findButton.style.height = 22;
            _findButton.style.marginLeft = 0;
            actions.Add(_findButton);

            _pullButton = makeAction(
                    "Pull",
                    () =>
                    {
                        try
                        {
                            RcSyncService.Pull(row.Builder);
                            RefreshLocalDiffPayload();
                            _refreshChrome?.Invoke();
                            _host?.Repaint();
                            RebindTitleStatusOnly(row);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[LiveOps Config] Pull failed: {ex.Message}\n{ex.StackTrace}");
                        }
                    },
                    "Read this variant from the local .rc or _overrides .gor and apply it back onto the builder (Apply() fields only).");
            actions.Add(_pullButton);

            _deployButton = makeAction(
                    "Deploy",
                    () =>
                    {
                        _ = DeployOneAsync(row);
                    },
                    "Sync, then ugs deploy the .rc (default profile) or the profile’s .gor (non-default).");
            actions.Add(_deployButton);

            bar.Add(actions);
            return bar;
        }

        private static void PingConfigAsset(LiveOpsConfigDiscovery.Row row)
        {
            if (row == null)
            {
                return;
            }

            UnityEngine.Object obj = row.Builder;
            if (obj == null && !string.IsNullOrEmpty(row.AssetPath))
            {
                obj = AssetDatabase.LoadMainAssetAtPath(row.AssetPath);
            }

            if (obj == null)
            {
                return;
            }

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        /// <summary>Updates status light + label in the title bar without rebuilding tabs (after Pull or deploy’s sync step).</summary>
        private void RebindTitleStatusOnly(LiveOpsConfigDiscovery.Row row)
        {
            if (row?.Builder == null || _titleStatusLight == null || _titleStatusLabel == null)
            {
                return;
            }

            RowStatus disk = RcSyncService.GetStatus(row.Builder);
            bool dup = row.IsDuplicateConfigKey;
            LiveOpsConfigStatusLights.ApplyToImage(_titleStatusLight, disk, dup);
            _titleStatusLabel.text = dup ? "·  Duplicate variant" : $"·  {LiveOpsConfigStatusLights.ShortStatusLabel(disk)}";
            bool pro = EditorGUIUtility.isProSkin;
            _titleStatusLabel.style.color = dup ? new Color(1f, 0.45f, 0.45f) : StatusToTextColor(disk, pro);
        }

        private static Image CreateStatusLightImage(RowStatus disk, bool isDuplicate)
        {
            var img = new Image();
            LiveOpsConfigStatusLights.ApplyToImage(img, disk, isDuplicate);
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = 18;
            img.style.height = 18;
            img.style.flexShrink = 0;
            return img;
        }

        private static Color StatusToTextColor(RowStatus disk, bool proSkin)
        {
            return disk switch
            {
                RowStatus.InSync => proSkin ? new Color(0.55f, 0.95f, 0.55f) : new Color(0.1f, 0.45f, 0.12f),
                RowStatus.Drift => proSkin ? new Color(1f, 0.75f, 0.35f) : new Color(0.65f, 0.35f, 0.05f),
                RowStatus.Missing => proSkin ? new Color(1f, 0.5f, 0.5f) : new Color(0.65f, 0.15f, 0.15f),
                _ => proSkin ? Color.gray : new Color(0.3f, 0.3f, 0.3f),
            };
        }

        private void BuildTabbedBody(LiveOpsConfigDiscovery.Row row, bool actionsEnabled, bool isDuplicate)
        {
            var tabStrip = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                    marginTop = 6,
                    marginLeft = 8,
                    marginRight = 8,
                    borderBottomWidth = 1,
                    borderBottomColor = EditorGUIUtility.isProSkin
                        ? new Color(0.12f, 0.12f, 0.12f)
                        : new Color(0.65f, 0.65f, 0.65f),
                },
            };

            _tabConfigPage = new ScrollView { style = { flexGrow = 1 } };
            _tabDiffPage = new ScrollView { style = { flexGrow = 1, display = DisplayStyle.None } };

            _tabConfigButton = new Button(() => SelectTab(0))
            {
                text = "Config",
                tooltip = "Edit the builder and any referenced ScriptableObjects, and view generated local JSON.",
            };
            _tabDiffButton = new Button(() => SelectTab(1))
            {
                text = "Diff",
                tooltip = "Compare locally generated JSON with the live Remote Config value in the cloud for this key.",
            };
            StyleTabButton(_tabConfigButton, true);
            StyleTabButton(_tabDiffButton, false);
            _tabConfigButton.style.marginRight = 2;
            _tabDiffButton.style.marginLeft = 2;

            tabStrip.Add(_tabConfigButton);
            tabStrip.Add(_tabDiffButton);
            Add(tabStrip);

            var body = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
            body.Add(_tabConfigPage);
            body.Add(_tabDiffPage);
            Add(body);

            BuildConfigTabContent(row);
            BuildDiffTabContent(row, actionsEnabled, isDuplicate);

            SelectTab(0);
        }

        private static void StyleTabButton(Button b, bool active)
        {
            b.style.height = 24;
            b.style.paddingLeft = 14;
            b.style.paddingRight = 14;
            b.style.marginBottom = -1;
            b.style.borderTopLeftRadius = 4;
            b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 0;
            b.style.borderBottomRightRadius = 0;
            b.style.borderTopWidth = 1;
            b.style.borderLeftWidth = 1;
            b.style.borderRightWidth = 1;
            b.style.borderBottomWidth = active ? 0 : 1;

            bool pro = EditorGUIUtility.isProSkin;
            Color bg = active
                ? (pro ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.88f, 0.88f, 0.88f))
                : (pro ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.75f, 0.75f, 0.75f));
            Color border = pro ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.6f, 0.6f, 0.6f);

            b.style.backgroundColor = bg;
            b.style.borderTopColor = border;
            b.style.borderLeftColor = border;
            b.style.borderRightColor = border;
            b.style.borderBottomColor = border;
            b.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
        }

        private void SelectTab(int index)
        {
            bool configSelected = index == 0;
            if (_tabConfigPage != null)
            {
                _tabConfigPage.style.display = configSelected ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_tabDiffPage != null)
            {
                _tabDiffPage.style.display = configSelected ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (_tabConfigButton != null)
            {
                StyleTabButton(_tabConfigButton, configSelected);
            }

            if (_tabDiffButton != null)
            {
                StyleTabButton(_tabDiffButton, !configSelected);
            }
        }

        private void BuildConfigTabContent(LiveOpsConfigDiscovery.Row row)
        {
            var wrap = new VisualElement { style = { paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 12 } };
            bool pro = EditorGUIUtility.isProSkin;
            Color configBlockBg = pro ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.95f, 0.95f, 0.95f);
            Color configBorder = pro ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.78f, 0.78f, 0.78f);
            Color refsBlockBg = pro ? new Color(0.15f, 0.17f, 0.19f) : new Color(0.91f, 0.93f, 0.96f);
            Color refsBorder = pro ? new Color(0.12f, 0.14f, 0.16f) : new Color(0.72f, 0.76f, 0.82f);

            var configBlock = new VisualElement
            {
                style =
                {
                    backgroundColor = configBlockBg,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = configBorder,
                    borderRightColor = configBorder,
                    borderTopColor = configBorder,
                    borderBottomColor = configBorder,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 10,
                },
            };

            var builderHeader = new Label("Config builder")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 6,
                },
            };
            configBlock.Add(builderHeader);

            UnityEditor.Editor builderEditor = UnityEditor.Editor.CreateEditor(row.Builder);
            _editors.Add(builderEditor);
            var builderImgui = new IMGUIContainer(
                () =>
                {
                    if (builderEditor != null && builderEditor.target != null)
                    {
                        EditorGUILayout.Space(4f);
                        builderEditor.OnInspectorGUI();
                    }
                })
            {
                style = { minHeight = 120f },
            };
            configBlock.Add(builderImgui);
            wrap.Add(configBlock);

            IReadOnlyList<ScriptableObject> refs = ConfigReferencedScriptableObjects.Enumerate(row.Builder);
            if (refs.Count > 0)
            {
                var refsIntro = new VisualElement
                {
                    style =
                    {
                        marginTop = 18,
                        paddingTop = 14,
                        borderTopWidth = 1,
                        borderTopColor = configBorder,
                    },
                };

                refsIntro.Add(
                    new Label("Referenced assets")
                    {
                        style =
                        {
                            unityFontStyleAndWeight = FontStyle.Bold,
                            fontSize = 12,
                            marginBottom = 2,
                        },
                    });

                refsIntro.Add(
                    new Label(
                        "Each reference is its own card: the top row is the read-only object field (click to select in the Project window) and expand/collapse; the full inspector is below when expanded.")
                    {
                        style =
                        {
                            fontSize = 11,
                            whiteSpace = WhiteSpace.Normal,
                            color = pro ? new Color(0.65f, 0.65f, 0.65f) : new Color(0.38f, 0.38f, 0.38f),
                            marginBottom = 8,
                        },
                    });

                foreach (ScriptableObject sob in refs)
                {
                    Type refType = sob.GetType();

                    var card = new VisualElement
                    {
                        style =
                        {
                            backgroundColor = refsBlockBg,
                            borderLeftWidth = 1,
                            borderRightWidth = 1,
                            borderTopWidth = 1,
                            borderBottomWidth = 1,
                            borderLeftColor = refsBorder,
                            borderRightColor = refsBorder,
                            borderTopColor = refsBorder,
                            borderBottomColor = refsBorder,
                            borderTopLeftRadius = 6,
                            borderTopRightRadius = 6,
                            borderBottomLeftRadius = 6,
                            borderBottomRightRadius = 6,
                            marginBottom = 10,
                            overflow = Overflow.Hidden,
                        },
                    };

                    var headerRow = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            paddingLeft = 4,
                            paddingRight = 6,
                            paddingTop = 4,
                            paddingBottom = 4,
                            backgroundColor = pro
                                ? new Color(0.12f, 0.14f, 0.16f)
                                : new Color(0.86f, 0.88f, 0.9f),
                            borderBottomWidth = 0,
                        },
                    };

                    var body = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = 6,
                            paddingRight = 6,
                            paddingTop = 4,
                            paddingBottom = 8,
                            borderTopWidth = 1,
                            borderTopColor = pro ? new Color(0.2f, 0.22f, 0.25f) : new Color(0.82f, 0.85f, 0.88f),
                        },
                    };

                    bool expanded = true;
                    var chevron = new Button
                    {
                        text = "\u25bc",
                        tooltip = "Show or hide the full inspector for this reference.",
                    };
                    chevron.style.width = 28;
                    chevron.style.minWidth = 28;
                    chevron.style.height = 20;
                    chevron.style.flexShrink = 0;
                    chevron.clicked += () =>
                    {
                        expanded = !expanded;
                        body.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                        chevron.text = expanded ? "\u25bc" : "\u25b6";
                    };

                    Type objectFieldType = refType.IsAbstract ? typeof(ScriptableObject) : refType;
                    var objField = new ObjectField
                    {
                        label = string.Empty,
                        objectType = objectFieldType,
                        value = sob,
                        allowSceneObjects = false,
                    };
                    objField.tooltip =
                        $"{refType.Name} — {AssetDatabase.GetAssetPath(sob)} — click the field to select this asset in the Project window.";
                    objField.style.flexGrow = 1;
                    objField.SetEnabled(false);

                    headerRow.Add(chevron);
                    headerRow.Add(objField);
                    card.Add(headerRow);

                    UnityEditor.Editor catEditor = UnityEditor.Editor.CreateEditor(sob);
                    _editors.Add(catEditor);
                    var catImgui = new IMGUIContainer(
                        () =>
                        {
                            if (catEditor != null && catEditor.target != null)
                            {
                                catEditor.OnInspectorGUI();
                            }
                        })
                    {
                        style = { minHeight = 60f },
                    };
                    body.Add(catImgui);
                    card.Add(body);
                    refsIntro.Add(card);
                }
                wrap.Add(refsIntro);
            }

            _tabConfigPage.Add(wrap);
        }

        private void BuildDiffTabContent(LiveOpsConfigDiscovery.Row row, bool actionsEnabled, bool isDuplicate)
        {
            var wrap = new VisualElement { style = { paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 12 } };

            var hint = new Label(
                "Local: generated JSON for this ConfigKey (default = Settings; non-default = value inside the profile’s Game Override). Use Compare with cloud to fetch the live value for this key.")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 8,
                    fontSize = 11,
                    color = EditorGUIUtility.isProSkin ? new Color(0.65f, 0.65f, 0.65f) : new Color(0.35f, 0.35f, 0.35f),
                },
            };
            wrap.Add(hint);

            _compareCloudButton = new Button(
                () =>
                {
                    _ = CompareCloudAsync(row.Builder);
                })
            {
                text = "Compare with cloud",
                tooltip = "Fetch the current Remote Config value for this key from UGS and show it next to the local generated JSON.",
            };
            _compareCloudButton.SetEnabled(actionsEnabled && !isDuplicate);
            _compareCloudButton.style.height = 24;
            _compareCloudButton.style.marginBottom = 8;
            wrap.Add(_compareCloudButton);

            _localDiffField = new TextField("Local (generated)") { isReadOnly = true, multiline = true };
            _remoteDiffField = new TextField("Remote (dashboard)") { isReadOnly = true, multiline = true };
            RefreshLocalDiffPayload();

            _localDiffField.style.minHeight = 200f;
            _remoteDiffField.style.minHeight = 200f;
            wrap.Add(_localDiffField);
            wrap.Add(_remoteDiffField);

            _tabDiffPage.Add(wrap);
        }

        private void RefreshLocalDiffPayload()
        {
            if (_localDiffField == null || _row?.Builder == null)
            {
                return;
            }

            try
            {
                if (_row.Builder.IsDefaultVariant)
                {
                    string envText = RcSyncService.RenderEnvelopeJson(_row.Builder);
                    var root = JObject.Parse(envText);
                    JToken inner = root["entries"]?[_row.Builder.ConfigKey];
                    _localDiffField.value = inner?.ToString(Formatting.Indented) ?? "(empty)";
                }
                else
                {
                    _localDiffField.value = RcSyncService.RenderLocalConfigPayloadJson(_row.Builder);
                }
            }
            catch (Exception ex)
            {
                _localDiffField.value = $"Could not build local JSON: {ex.Message}";
            }
        }

        private async Task CompareCloudAsync(ConfigBuilderSOBase builder)
        {
            if (_remoteDiffField == null || _localDiffField == null)
            {
                return;
            }

            try
            {
                _remoteDiffField.value = "Fetching…";
                CloudFetchResult res = await CloudRemoteConfigSnapshot.TryFetchValueJsonForKeyAsync(
                    builder.ConfigKey,
                    CancellationToken.None);
                if (!res.Ok)
                {
                    _remoteDiffField.value = res.Error;
                    Debug.LogWarning($"[LiveOps Config] Cloud compare: {res.Error}");
                    return;
                }

                _remoteDiffField.value = res.Json ?? string.Empty;
                RefreshLocalDiffPayload();
            }
            catch (Exception ex)
            {
                _remoteDiffField.value = ex.Message;
                Debug.LogError($"[LiveOps Config] Cloud compare failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        internal void SetDeployChromeLocked(bool locked)
        {
            _findButton?.SetEnabled(!locked);
            _pullButton?.SetEnabled(_rowActionsEnabled && !locked);
            _deployButton?.SetEnabled(_rowActionsEnabled && !locked);
            _compareCloudButton?.SetEnabled(_rowActionsEnabled && !locked);
            _tabConfigButton?.SetEnabled(!locked);
            _tabDiffButton?.SetEnabled(!locked);
        }

        private async Task DeployOneAsync(LiveOpsConfigDiscovery.Row row)
        {
            if (_deployer == null || row?.Builder == null)
            {
                return;
            }

            IReadOnlyList<string> paths = RcSyncService.CollectDeployPathsForVariant(row.Builder);
            if (_host is LiveOpsConfigsWindow win)
            {
                await win.RunDeployWorkflowAsync(
                    () =>
                    {
                        AssetDatabase.SaveAssets();
                        RcSyncService.SyncForBuilder(row.Builder);
                        RefreshLocalDiffPayload();
                        RebindTitleStatusOnly(row);
                        _host?.Repaint();
                    },
                    paths);
                return;
            }

            try
            {
                AssetDatabase.SaveAssets();
                RcSyncService.SyncForBuilder(row.Builder);
                RefreshLocalDiffPayload();
                RebindTitleStatusOnly(row);
                _refreshChrome?.Invoke();
                _host?.Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Sync before deploy failed: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            try
            {
                DeployOutcome outcome = await _deployer.DeployAsync(
                    paths,
                    CancellationToken.None);
                if (outcome.AllSucceeded)
                {
                    Debug.Log($"[LiveOps Config] Deploy ok ({outcome.Transport}): {outcome.Message}");
                }
                else
                {
                    Debug.LogError($"[LiveOps Config] Deploy failed ({outcome.Transport}): {outcome.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Deploy exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ClearBoundEditors()
        {
            foreach (UnityEditor.Editor e in _editors)
            {
                if (e != null)
                {
                    UnityEngine.Object.DestroyImmediate(e);
                }
            }

            _editors.Clear();
        }
    }
}

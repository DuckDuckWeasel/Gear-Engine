using System.Collections.Generic;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Editor
{
    public static class AssetPublisherRebakeMenu
    {
        [MenuItem("Tools/Scaffold/AppFlow/Rebake All Layer Asset Publishers")]
        public static void RebakeAll()
        {
            int count = 0;
            foreach (MonoBehaviour mb in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (mb is IAssetPublisherDefinitionHost host)
                {
                    IReadOnlyList<AssetPublisherDefinition> defs = host.AssetPublisherDefinitions;
                    for (int i = 0; i < defs.Count; i++)
                    {
                        if (defs[i] == null)
                        {
                            continue;
                        }

                        defs[i].Rebake();
                        count++;
                    }

                    EditorUtility.SetDirty(mb);
                }
            }

            if (count > 0)
            {
                Debug.Log($"[AppFlow] Rebake All Layer Asset Publishers: rebaked {count} definition(s).");
            }
            else
            {
                Debug.Log(
                    "[AppFlow] Rebake All Layer Asset Publishers: no definitions rebaked. Open scenes/prefabs that implement IAssetPublisherDefinitionHost, or add inline publishers to GearAppFlowRoot.");
            }

            AssetDatabase.SaveAssets();
        }
    }
}

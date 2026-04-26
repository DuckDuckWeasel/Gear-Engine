using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using Scaffold.AppFlow.Publishers.Addressables;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;

namespace GearEngine.App.Bootstrap.Editor
{
    /// <summary>Default <see cref="AssetPublisherDefinition"/> rows: label-driven <see cref="TrackDefinition"/> (liveops.tracks) + single-address <see cref="GearEngine.GearEngine.Config.GearCatalogSO"/>.</summary>
    public static class LayerPublishersBuildUtility
    {
        public const string CampaignGearAddressableGuid = "f2f2f2f2f2f2f2f2f2f2f2f2f2f2f2f2";
        public const string TracksAddressableLabel = "liveops.tracks";
        public const string PlayerPrefInlineSeeded = "GearEngine.LayerInlinePublishersSeededV2";

        public static List<AssetPublisherDefinition> CreateDefaultCampaignDefinitions()
        {
            var list = new List<AssetPublisherDefinition>(2);
            var label = new AddressableLabelSource();
            label.SetLabelAndTypeForEditor(TracksAddressableLabel, typeof(TrackDefinition).AssemblyQualifiedName);
            var defTracks = new AssetPublisherDefinition();
            defTracks.SetSourceAndRebake(label);
            list.Add(defTracks);
            var single = new AddressableSingleSource();
            single.SetAssetByGuidForEditor(CampaignGearAddressableGuid);
            var defGear = new AssetPublisherDefinition();
            defGear.SetSourceAndRebake(single);
            list.Add(defGear);
            return list;
        }
    }
}

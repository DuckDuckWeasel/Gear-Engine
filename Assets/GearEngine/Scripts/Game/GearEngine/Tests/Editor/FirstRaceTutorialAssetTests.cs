using System.IO;
using GearEngine.GearEngine.Presentation.UI.Input;
using GearEngine.GearEngine.Presentation.UI.Tags;
using NUnit.Framework;
using Scaffold;
using Scaffold.Tutorial.Data;
using UnityEditor;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public sealed class FirstRaceTutorialAssetTests
    {
        private const string k_tutorialPath =
            "Assets/GearEngine/Data/Tutorial/FirstRaceTutorial.asset";
        private const string k_wrapperPath =
            "Assets/GearEngine/Data/Tutorial/TutorialWrapper.asset";
        private const string k_raceButtonPrefabPath =
            "Assets/GearEngine/Prefabs/Campaign/Race_Button.prefab";
        private const string k_raceButtonTagPath =
            "Assets/GearEngine/Data/Gear/Tag/Tutorial/RaceButton_Tag.asset";

        [Test]
        public void TutorialAsset_ContainsMinimalBlackboardSequence()
        {
            TutorialSO tutorial =
                AssetDatabase.LoadAssetAtPath<TutorialSO>(k_tutorialPath);
            TutorialWrapper wrapper =
                AssetDatabase.LoadAssetAtPath<TutorialWrapper>(k_wrapperPath);

            Assert.That(tutorial, Is.Not.Null);
            Assert.That(tutorial.Id, Is.EqualTo("gear_engine_first_race"));
            Assert.That(tutorial.TutorialProgressController, Is.Not.Null);
            Assert.That(wrapper, Is.Not.Null);
            Assert.That(wrapper.Tutorials, Does.Contain(tutorial));
            Assert.That(wrapper.StartTutorials, Does.Contain(tutorial));

            GameObject prefab =
                tutorial.TutorialProgressController.gameObject;
            Assert.That(
                prefab.GetComponent<
                    Scaffold.Tutorial.Controllers.TutorialProgressController>(),
                Is.Not.Null);

            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            string prefabYaml = File.ReadAllText(prefabPath);
            Assert.That(prefabYaml, Does.Contain("blockName: FTUE_01_PLACE_GEAR"));
            Assert.That(prefabYaml, Does.Contain("blockName: FTUE_02_START_RACE"));
            Assert.That(prefabYaml, Does.Contain("blockName: FTUE_03_COMPLETE"));
            Assert.That(prefabYaml, Does.Contain("class: CompleteTutorial"));
            Assert.That(
                prefabYaml,
                Does.Contain(
                    "progressController: {fileID: 7477327881227392170}"));
        }

        [Test]
        public void RaceButtonPrefab_HasTutorialTargetTag()
        {
            GameObject raceButton =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    k_raceButtonPrefabPath);
            TagSO raceButtonTag =
                AssetDatabase.LoadAssetAtPath<TagSO>(k_raceButtonTagPath);

            Assert.That(raceButton, Is.Not.Null);
            Assert.That(raceButtonTag, Is.Not.Null);
            TagComponent tagComponent =
                raceButton.GetComponent<TagComponent>();
            Assert.That(tagComponent, Is.Not.Null);
            Assert.That(tagComponent.HasTag(raceButtonTag), Is.True);
        }

    }
}

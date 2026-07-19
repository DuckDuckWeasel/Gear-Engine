using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Scaffold.Tests.Editor
{
    public class ScaffoldEditorResourcesTests
    {
        [Test]
        public void FlowGraph_ProvidesFreeAndProTextures()
        {
            EditorUtils.ScaffoldEditorResources resource = Scaffold.EditorUtils.ScaffoldEditorResources.Instance;
            SerializedObject serializedResource = new SerializedObject(resource);
            SerializedProperty flowGraph = serializedResource.FindProperty("flow_graph");

            Assert.That(flowGraph, Is.Not.Null);

            Texture2D freeTexture = flowGraph.FindPropertyRelative("free").objectReferenceValue as Texture2D;
            Texture2D proTexture = flowGraph.FindPropertyRelative("pro").objectReferenceValue as Texture2D;

            Assert.That(freeTexture, Is.Not.Null);
            Assert.That(proTexture, Is.Not.Null);
            Assert.That(freeTexture.width, Is.EqualTo(32));
            Assert.That(freeTexture.height, Is.EqualTo(32));
            Assert.That(proTexture.width, Is.EqualTo(32));
            Assert.That(proTexture.height, Is.EqualTo(32));
        }

        [Test]
        public void BlockInspectorScriptIcon_ReferencesTheFreeFlowGraphTexture()
        {
            string metaPath = Path.Combine(Application.dataPath, "3rdParty/ScaffoldVisualScripting/Scripts/Editor/BlockInspector.cs.meta");
            string metaContents = File.ReadAllText(metaPath);

            Assert.That(metaContents, Does.Contain("guid: 6a8f86d6e2294031b2ab144f9b86ea57"));
        }

        [TestCase(10f, 20f, 20f, false)]
        [TestCase(80f, 20f, 80f, false)]
        [TestCase(120f, 20f, 80f, true)]
        public void DescriptionLayout_ClampsHeightAndReportsScrollRequirement(float contentHeight, float lineHeight, float expectedHeight, bool expectedScroll)
        {
            Type styleSheetType = typeof(Scaffold.EditorUtils.BlockEditor).Assembly.GetType("Scaffold.EditorUtils.BlockInspectorStyleSheet");
            MethodInfo calculateMethod = styleSheetType.GetMethod("CalculateDescriptionLayout", BindingFlags.Static | BindingFlags.NonPublic);
            object layout = calculateMethod.Invoke(null, new object[] { contentHeight, lineHeight });
            Type layoutType = layout.GetType();

            float height = (float)layoutType.GetProperty("Height", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(layout);
            bool requiresScroll = (bool)layoutType.GetProperty("RequiresScroll", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(layout);

            Assert.That(height, Is.EqualTo(expectedHeight));
            Assert.That(requiresScroll, Is.EqualTo(expectedScroll));
        }

        [TestCase(479f, true)]
        [TestCase(480f, false)]
        [TestCase(900f, false)]
        public void SummaryLayout_UsesVerticalLayoutOnlyBelowTheCompactBreakpoint(float availableWidth, bool expectedCompactLayout)
        {
            Type styleSheetType = typeof(Scaffold.EditorUtils.BlockEditor).Assembly.GetType("Scaffold.EditorUtils.BlockInspectorStyleSheet");
            MethodInfo compactLayoutMethod = styleSheetType.GetMethod("UsesCompactSummaryLayout", BindingFlags.Static | BindingFlags.NonPublic);
            bool usesCompactLayout = (bool)compactLayoutMethod.Invoke(null, new object[] { availableWidth });

            Assert.That(usesCompactLayout, Is.EqualTo(expectedCompactLayout));
        }

        [Test]
        public void RefreshForBlackboard_RebuildsTheOpenWindowBlockCollection()
        {
            UnityEngine.Object previousSelection = Selection.activeObject;
            GameObject blackboardObject = new GameObject("Blackboard");
            Blackboard blackboard = blackboardObject.AddComponent<Blackboard>();
            blackboardObject.AddComponent<Block>();
            TestBlackboardWindow window = ScriptableObject.CreateInstance<TestBlackboardWindow>();

            try
            {
                Selection.activeGameObject = blackboardObject;
                window.SetBlackboard(blackboard);
                Scaffold.EditorUtils.BlackboardWindow.RefreshForBlackboard(blackboard);

                Assert.That(window.BlockCount, Is.EqualTo(1));

                blackboardObject.AddComponent<Block>();
                Scaffold.EditorUtils.BlackboardWindow.RefreshForBlackboard(blackboard);

                Assert.That(window.BlockCount, Is.EqualTo(2));
            }
            finally
            {
                Selection.activeObject = previousSelection;
                UnityEngine.Object.DestroyImmediate(window);
                UnityEngine.Object.DestroyImmediate(blackboardObject);
            }
        }

        private class TestBlackboardWindow : Scaffold.EditorUtils.BlackboardWindow
        {
            public int BlockCount => blocks.Length;

            public void SetBlackboard(Blackboard targetBlackboard)
            {
                blackboard = targetBlackboard;
            }
        }
    }
}

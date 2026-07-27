using System.Collections.Generic;
using OM.Animora.Runtime;
using OM.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace OM.Animora.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="AnimoraAction"/> that dynamically renders all child properties
    /// using UI Toolkit, with optional filtering and change handling.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimoraActionWithTargets<>), true)]
    public class AnimoraActionWithTargetsDrawer : PropertyDrawer
    {
        /// <summary>
        /// Cached dictionary of property fields by name for easy access and update handling.
        /// </summary>
        protected readonly Dictionary<string, PropertyField> propertyFields = new Dictionary<string, PropertyField>();
        protected SerializedProperty mainProperty;

        /// <summary>
        /// Creates the UI layout for the property using VisualElements.
        /// </summary>
        /// <param name="property">The serialized property being drawn.</param>
        /// <returns>A <see cref="VisualElement"/> containing the entire property UI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            mainProperty = property.Copy();
            propertyFields.Clear();

            VisualElement root = new VisualElement();

            foreach (SerializedProperty childProperty in property.GetAllProperties())
            {
                DrawProperty(childProperty.Copy(), root);
            }

            PropertyField customTargetsField = GetPropertyField("customTargets");
            SerializedProperty useCustomTargetsProp = mainProperty.FindPropertyRelative("useCustomTargets");
            if (customTargetsField != null && useCustomTargetsProp != null)
            {
                customTargetsField.SetDisplay(useCustomTargetsProp.boolValue);
            }

            return root;
        }

        /// <summary>
        /// Draws an individual child property if allowed by <see cref="CanDrawProperty"/>.
        /// </summary>
        /// <param name="property">The property to draw.</param>
        /// <param name="parent">The parent element to add the field to.</param>
        protected virtual void DrawProperty(SerializedProperty property, VisualElement parent)
        {
            if (!CanDrawProperty(property))
            {
                return;
            }

            PropertyField propertyField = new PropertyField(property);
            propertyField.Bind(property.serializedObject);
            parent.Add(propertyField);

            propertyFields.Add(property.name, propertyField);

            propertyField.RegisterValueChangeCallback(e =>
            {
                OnAnyPropertyChanged(property.name, propertyField);
            });
        }

        /// <summary>
        /// Determines whether a property should be drawn.
        /// Skips properties marked with <see cref="OM_HideInInspector"/>.
        /// </summary>
        /// <param name="property">The property to evaluate.</param>
        /// <returns>True if the property should be drawn; otherwise, false.</returns>
        protected virtual bool CanDrawProperty(SerializedProperty property)
        {
            OM_HideInInspector hideInInspector = property.GetAttribute<OM_HideInInspector>(true);
            return hideInInspector == null;
        }

        /// <summary>
        /// Called whenever any property field changes.
        /// Can be overridden to implement custom update behavior.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        /// <param name="propertyField">The field instance that changed.</param>
        protected virtual void OnAnyPropertyChanged(string propertyName, PropertyField propertyField)
        {
            // Override in subclass to react to property changes
            if (propertyName == "useCustomTargets")
            {
                PropertyField field = GetPropertyField("customTargets");
                field?.SetDisplay(mainProperty.FindPropertyRelative("useCustomTargets").boolValue);
            }
        }

        /// <summary>
        /// Gets a drawn <see cref="PropertyField"/> by its property name.
        /// </summary>
        /// <param name="propertyName">The name of the property field.</param>
        /// <returns>The corresponding <see cref="PropertyField"/>, or null if not found.</returns>
        protected PropertyField GetPropertyField(string propertyName)
        {
            return propertyFields.GetValueOrDefault(propertyName);
        }
    }
}

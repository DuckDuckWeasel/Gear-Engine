using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardTypeDropdown : AdvancedDropdown
    {
        public BlackboardTypeDropdown(AdvancedDropdownState state, string title, IReadOnlyList<Type> types, Action<Type> selected, bool includeNone = false) : base(state)
        {
            this.title = title ?? "Select Type";
            this.types = types ?? throw new ArgumentNullException(nameof(types));
            this.selected = selected ?? throw new ArgumentNullException(nameof(selected));
            this.includeNone = includeNone;
            minimumSize = new UnityEngine.Vector2(320f, 360f);
        }

        private readonly string title;
        private readonly IReadOnlyList<Type> types;
        private readonly Action<Type> selected;
        private readonly bool includeNone;

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem(title);
            if (includeNone)
            {
                root.AddChild(new TypeItem("None", null));
            }

            for (int index = 0; index < types.Count; index++)
            {
                AddType(root, types[index]);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem typeItem)
            {
                selected.Invoke(typeItem.Type);
            }
        }

        private void AddType(AdvancedDropdownItem root, Type type)
        {
            string category = BlackboardEditorDisplay.GetCategory(type);
            AdvancedDropdownItem parent = GetOrCreateCategory(root, category);
            TypeItem item = new TypeItem(BlackboardEditorDisplay.GetName(type), type);
            parent.AddChild(item);
        }

        private AdvancedDropdownItem GetOrCreateCategory(AdvancedDropdownItem root, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return root;
            }

            AdvancedDropdownItem parent = root;
            string[] segments = category.Split('/');
            for (int index = 0; index < segments.Length; index++)
            {
                parent = GetOrCreateChild(parent, segments[index]);
            }

            return parent;
        }

        private AdvancedDropdownItem GetOrCreateChild(AdvancedDropdownItem parent, string name)
        {
            for (int index = 0; index < parent.childList.Count; index++)
            {
                AdvancedDropdownItem child = parent.childList[index];
                if (!(child is TypeItem) && string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            AdvancedDropdownItem category = new AdvancedDropdownItem(name);
            parent.AddChild(category);
            return category;
        }

        private sealed class TypeItem : AdvancedDropdownItem
        {
            public TypeItem(string name, Type type) : base(name)
            {
                Type = type;
            }

            public Type Type { get; }
        }
    }
}

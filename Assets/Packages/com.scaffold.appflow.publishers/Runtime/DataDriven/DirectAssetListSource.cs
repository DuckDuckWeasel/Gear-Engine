using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [Serializable]
    public sealed class DirectAssetListSource : IAssetPublisherSource
    {
        [SerializeField]
        private List<UnityEngine.Object> assets = new();

        public IReadOnlyList<UnityEngine.Object> Assets => assets;

#if UNITY_EDITOR
        public bool IsConfigured => assets != null && assets.Count > 0 && CommonType() != null;

        public IPublisherRegistrar Bake()
        {
            Type t = CommonType();
            if (t == null)
            {
                return null;
            }

            Type closed = typeof(DirectAssetListPublisherRegistrar<>).MakeGenericType(t);
            int len = assets.Count;
            Array typed = Array.CreateInstance(t, len);
            for (int i = 0; i < len; i++)
            {
                object entry = assets[i];
                if (entry == null)
                {
                    return null;
                }

                typed.SetValue(entry, i);
            }

            object listInstance = ToGenericList(typed, t);
            if (listInstance == null)
            {
                return null;
            }

            return (IPublisherRegistrar)Activator.CreateInstance(closed, new object[] { listInstance });
        }

        private Type CommonType()
        {
            if (assets == null || assets.Count == 0)
            {
                return null;
            }

            Type t = null;
            foreach (UnityEngine.Object a in assets)
            {
                if (a == null)
                {
                    return null;
                }

                Type aType = a.GetType();
                t = t == null ? aType : DirectTypeCompatibility.LeastCommonAncestor(t, aType);
            }

            if (t == null || t == typeof(UnityEngine.Object))
            {
                return null;
            }

            return t;
        }

        private static object ToGenericList(Array typed, Type t)
        {
            Type listType = typeof(List<>).MakeGenericType(t);
            object list = Activator.CreateInstance(listType);
            MethodInfo add = listType.GetMethod("Add", new[] { t });
            if (add == null)
            {
                return null;
            }

            for (int i = 0; i < typed.Length; i++)
            {
                add.Invoke(list, new[] { typed.GetValue(i) });
            }

            return list;
        }
#endif
    }
}

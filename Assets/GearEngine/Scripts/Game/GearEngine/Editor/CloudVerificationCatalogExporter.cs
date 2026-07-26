using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    public static class CloudVerificationCatalogExporter
    {
        private const string CatalogPathEnvironmentVariable = "CLOUD_VERIFICATION_CATALOG_PATH";
        private const string CloudVerificationCategory = "CloudVerification";

        public static void Export()
        {
            try
            {
                string catalogPath = Environment.GetEnvironmentVariable(CatalogPathEnvironmentVariable);
                if (string.IsNullOrWhiteSpace(catalogPath))
                {
                    throw new InvalidOperationException($"{CatalogPathEnvironmentVariable} is required.");
                }

                CloudVerificationCatalog catalog = new CloudVerificationCatalog
                {
                    tests = DiscoverTests().OrderBy(test => test.fullName, StringComparer.Ordinal).ToArray(),
                };

                string directory = Path.GetDirectoryName(catalogPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(catalogPath, JsonUtility.ToJson(catalog, true));
                Debug.Log($"[CloudVerification] Exported {catalog.tests.Length} tests to {catalogPath}.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CloudVerification] Failed to export the test catalog. {exception}");
                throw;
            }
        }

        private static IEnumerable<CloudVerificationCatalogTest> DiscoverTests()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GetName().Name.StartsWith("Game.GearEngine.Tests", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (Type type in GetLoadableTypes(assembly))
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(method);
                        if (!IsTest(attributes) || !HasCategory(attributes, CloudVerificationCategory))
                        {
                            continue;
                        }

                        string[] targets = GetTargets(attributes);
                        if (targets.Length == 0)
                        {
                            throw new InvalidOperationException($"{type.FullName}.{method.Name} is missing CloudVerificationTargets.");
                        }

                        string[] categories = GetCategories(attributes)
                            .Where(category => !string.Equals(category, CloudVerificationCategory, StringComparison.Ordinal))
                            .ToArray();
                        yield return new CloudVerificationCatalogTest
                        {
                            fullName = $"{type.FullName}.{method.Name}",
                            name = method.Name,
                            category = categories.FirstOrDefault() ?? "Uncategorized",
                            categories = categories,
                            targets = targets,
                        };
                    }
                }
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }

        private static bool IsTest(IList<CustomAttributeData> attributes)
        {
            return attributes.Any(attribute =>
                string.Equals(attribute.AttributeType.FullName, "NUnit.Framework.TestAttribute", StringComparison.Ordinal) ||
                string.Equals(attribute.AttributeType.FullName, "UnityEngine.TestTools.UnityTestAttribute", StringComparison.Ordinal));
        }

        private static bool HasCategory(IList<CustomAttributeData> attributes, string expectedCategory)
        {
            return GetCategories(attributes).Any(category => string.Equals(category, expectedCategory, StringComparison.Ordinal));
        }

        private static string[] GetCategories(IList<CustomAttributeData> attributes)
        {
            return attributes
                .Where(attribute => string.Equals(attribute.AttributeType.FullName, "NUnit.Framework.CategoryAttribute", StringComparison.Ordinal))
                .Select(attribute => attribute.ConstructorArguments[0].Value?.ToString())
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .ToArray();
        }

        private static string[] GetTargets(IList<CustomAttributeData> attributes)
        {
            CustomAttributeData targetAttribute = attributes.FirstOrDefault(attribute =>
                string.Equals(attribute.AttributeType.FullName, "GearEngine.GearEngine.Tests.Editor.CloudVerificationTargetsAttribute", StringComparison.Ordinal));
            if (targetAttribute == null || targetAttribute.ConstructorArguments.Count == 0)
            {
                return Array.Empty<string>();
            }

            if (targetAttribute.ConstructorArguments[0].Value is not IReadOnlyCollection<CustomAttributeTypedArgument> targetArguments)
            {
                return Array.Empty<string>();
            }

            return targetArguments
                .Select(target => Enum.GetName(target.ArgumentType, target.Value))
                .Where(target => !string.IsNullOrWhiteSpace(target))
                .ToArray();
        }

        [Serializable]
        private sealed class CloudVerificationCatalog
        {
            public CloudVerificationCatalogTest[] tests;
        }

        [Serializable]
        private sealed class CloudVerificationCatalogTest
        {
            public string fullName;
            public string name;
            public string category;
            public string[] categories;
            public string[] targets;
        }
    }
}

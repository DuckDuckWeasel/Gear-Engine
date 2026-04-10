using System;
using System.Reflection;
using UnityEngine;
using VContainer;
using Scaffold.Navigation;

namespace Game.Race.Navigation
{
    /// <summary>
    /// Registers Scaffold navigation for <see cref="RaceScope"/> using reflection, so the project still parses
    /// if UPM has not yet restored <c>com.scaffold.navigation</c> under <c>Library/PackageCache</c>.
    /// </summary>
    internal static class RaceNavigationInstaller
    {
        public static void Install(IContainerBuilder builder, NavigationSettings navigationSettings, Transform viewHolder)
        {
            if (navigationSettings == null)
            {
                throw new ArgumentNullException(nameof(navigationSettings));
            }

            if (viewHolder == null)
            {
                throw new ArgumentNullException(nameof(viewHolder));
            }

            builder.RegisterInstance(viewHolder);

            var navigationContainerAsm = TryLoadAssembly("Scaffold.Navigation.Container");
            if (navigationContainerAsm == null)
            {
                Debug.LogError(
                    "[RaceNavigationInstaller] Assembly Scaffold.Navigation.Container not found. " +
                    "Open the project in Unity once so UPM restores com.scaffold.navigation.");
                return;
            }

            if (TryInvokeAddNavigationExtension(builder, navigationSettings, viewHolder, navigationContainerAsm))
            {
                return;
            }

            if (TryInvokeNavigationInstaller(builder, navigationSettings, viewHolder, navigationContainerAsm))
            {
                return;
            }

            Debug.LogError(
                "[RaceNavigationInstaller] Could not register navigation (no AddNavigation extension or NavigationInstaller.Install match). " +
                "Inspect com.scaffold.navigation in Library/PackageCache and align this helper.");
        }

        private static bool TryInvokeAddNavigationExtension(
            IContainerBuilder builder,
            NavigationSettings settings,
            Transform viewHolder,
            Assembly navigationContainerAsm)
        {
            foreach (var type in navigationContainerAsm.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!IsExtensionMethod(method))
                    {
                        continue;
                    }

                    if (!string.Equals(method.Name, "AddNavigation", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    try
                    {
                        if (parameters.Length == 2
                            && parameters[0].ParameterType == typeof(IContainerBuilder)
                            && parameters[1].ParameterType == typeof(NavigationSettings))
                        {
                            method.Invoke(null, new object[] { builder, settings });
                            return true;
                        }

                        if (parameters.Length == 3
                            && parameters[0].ParameterType == typeof(IContainerBuilder)
                            && parameters[1].ParameterType == typeof(NavigationSettings)
                            && typeof(Transform).IsAssignableFrom(parameters[2].ParameterType))
                        {
                            method.Invoke(null, new object[] { builder, settings, viewHolder });
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[RaceNavigationInstaller] AddNavigation failed: {ex.Message}");
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool TryInvokeNavigationInstaller(
            IContainerBuilder builder,
            NavigationSettings settings,
            Transform viewHolder,
            Assembly navigationContainerAsm)
        {
            var installerType = navigationContainerAsm.GetType("Scaffold.Navigation.Container.NavigationInstaller");
            if (installerType == null)
            {
                return false;
            }

            object installer;
            try
            {
                installer = Activator.CreateInstance(installerType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceNavigationInstaller] Could not create NavigationInstaller: {ex.Message}");
                return false;
            }

            foreach (var method in installerType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!string.Equals(method.Name, "Install", StringComparison.Ordinal))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                try
                {
                    if (parameters.Length == 2
                        && parameters[0].ParameterType == typeof(IContainerBuilder)
                        && parameters[1].ParameterType == typeof(NavigationSettings))
                    {
                        method.Invoke(installer, new object[] { builder, settings });
                        return true;
                    }

                    if (parameters.Length == 3
                        && parameters[0].ParameterType == typeof(IContainerBuilder)
                        && parameters[1].ParameterType == typeof(NavigationSettings)
                        && typeof(Transform).IsAssignableFrom(parameters[2].ParameterType))
                    {
                        method.Invoke(installer, new object[] { builder, settings, viewHolder });
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RaceNavigationInstaller] NavigationInstaller.Install failed: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        private static bool IsExtensionMethod(MethodInfo method)
        {
            return method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }

        private static Assembly TryLoadAssembly(string name)
        {
            try
            {
                return Assembly.Load(name);
            }
            catch
            {
                return null;
            }
        }
    }
}

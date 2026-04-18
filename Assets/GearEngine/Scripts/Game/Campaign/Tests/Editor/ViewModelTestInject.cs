using System.Reflection;
using NUnit.Framework;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;

namespace GearEngine.Campaign.Tests.Editor
{
    internal static class ViewModelTestInject
    {
        public static void InvokeInitialize(ViewModel vm)
        {
            MethodInfo init = vm.GetType().GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(init, Is.Not.Null);
            init.Invoke(vm, null);
        }

        public static void InjectPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        public static void InjectNavigation(ViewModel vm, INavigation navigation)
        {
            FieldInfo field = typeof(ViewModel).GetField("navigation", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(vm, navigation);
        }
    }
}

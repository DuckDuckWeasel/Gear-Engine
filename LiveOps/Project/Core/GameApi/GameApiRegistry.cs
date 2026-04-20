using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameModule.GameApi
{
    /// <summary>
    /// Maps <c>RequestKey</c> strings to request/response CLR types for all registered handlers.
    /// </summary>
    public sealed class GameApiRegistry
    {
        private readonly Dictionary<string, (Type Req, Type Res)> _map = new Dictionary<string, (Type Req, Type Res)>();

        public GameApiRegistry(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentException("At least one assembly is required.", nameof(assemblies));
            }

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (Type iface in type.GetInterfaces())
                    {
                        if (!iface.IsGenericType)
                        {
                            continue;
                        }

                        if (iface.GetGenericTypeDefinition() != typeof(IGameApiHandler<,>))
                        {
                            continue;
                        }

                        Type[] args = iface.GetGenericArguments();
                        string key = args[0].Name;
                        if (_map.ContainsKey(key))
                        {
                            throw new InvalidOperationException($"Duplicate GameApi handler for request key '{key}'.");
                        }

                        _map[key] = (args[0], args[1]);
                    }
                }
            }
        }

        public bool Contains(string requestKey)
        {
            return !string.IsNullOrEmpty(requestKey) && _map.ContainsKey(requestKey);
        }

        public bool TryResolve(string requestKey, out Type requestType, out Type responseType)
        {
            if (string.IsNullOrEmpty(requestKey) || !_map.TryGetValue(requestKey, out (Type Req, Type Res) pair))
            {
                requestType = null;
                responseType = null;
                return false;
            }

            requestType = pair.Req;
            responseType = pair.Res;
            return true;
        }
    }
}

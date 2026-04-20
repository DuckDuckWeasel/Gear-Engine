using System;
using System.Collections.Generic;

namespace Scaffold.CloudCode
{
    internal sealed class CloudCodeOptimisticHandlerRegistry
    {
        private readonly Dictionary<(Type Request, Type Response), IRequestHandler> handlers =
            new Dictionary<(Type Request, Type Response), IRequestHandler>();

        internal void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler) where TRequest : class
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type requestType = typeof(TRequest);
            Type responseType = typeof(TResponse);
            (Type, Type) key = (requestType, responseType);
            if (handlers.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Duplicate optimistic handler for ({requestType.Name}, {responseType.Name}).");
            }

            handlers.Add(key, handler);
        }

        internal bool TryGetHandler(Type requestType, Type responseType, out IRequestHandler handler)
        {
            return handlers.TryGetValue((requestType, responseType), out handler);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Scaffold.CloudCode
{
    public sealed class CloudCodeOptimisticHandlerRegistry
    {
        private readonly Dictionary<(Type Request, Type Response), IRequestHandler> handlers =
            new Dictionary<(Type Request, Type Response), IRequestHandler>();

        public void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler) where TRequest : class
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

        public bool TryResolve<TResponse>(string module, string endpoint, object request, out IRequestHandler<TResponse> handler, out TResponse optimisticResponse)
        {
            handler = null;
            optimisticResponse = default;
            if (request == null)
            {
                return false;
            }

            if (!handlers.TryGetValue((request.GetType(), typeof(TResponse)), out IRequestHandler found) || found == null)
            {
                return false;
            }

            if (found is not IRequestHandler<TResponse> typedHandler)
            {
                return false;
            }

            if (!found.TryMatch(module, endpoint, request))
            {
                return false;
            }

            handler = typedHandler;
            optimisticResponse = typedHandler.GetOptimisticResponse(request);
            return true;
        }
    }
}

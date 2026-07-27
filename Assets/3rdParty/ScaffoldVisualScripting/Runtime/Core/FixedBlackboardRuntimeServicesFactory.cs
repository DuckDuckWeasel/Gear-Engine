using System;

namespace Scaffold.VisualScripting
{
    public sealed class FixedBlackboardRuntimeServicesFactory : IBlackboardRuntimeServicesFactory
    {
        public FixedBlackboardRuntimeServicesFactory(BlackboardRuntimeServices services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private readonly BlackboardRuntimeServices services;

        public BlackboardRuntimeServices Create()
        {
            return services;
        }
    }
}

using System;

namespace Scaffold.VisualScripting.Authoring
{
    public sealed class BlackboardDefinitionResolutionException : InvalidOperationException
    {
        public BlackboardDefinitionResolutionException(string message) : base(message)
        {
        }
    }
}

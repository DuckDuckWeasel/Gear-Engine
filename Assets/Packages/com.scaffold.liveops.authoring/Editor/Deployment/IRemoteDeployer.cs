using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>Deploys <c>.rc</c> assets to UGS without opening the Deployment window.</summary>
    public interface IRemoteDeployer
    {
        Task<DeployOutcome> DeployAsync(IReadOnlyList<string> rcPaths, CancellationToken cancellationToken = default);
    }

    public enum DeployTransport
    {
        Api,
        Cli,
    }

    public sealed class DeployOutcome
    {
        public DeployOutcome(bool allSucceeded, string message, DeployTransport transport)
        {
            AllSucceeded = allSucceeded;
            Message = message ?? string.Empty;
            Transport = transport;
        }

        public bool AllSucceeded { get; }

        public string Message { get; }

        public DeployTransport Transport { get; }
    }
}

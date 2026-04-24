using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>Deploys <c>.rc</c> assets to UGS with the <c>ugs deploy</c> CLI (see <c>Scaffold.LiveOps.Authoring.Editor.Deployment.RemoteDeployer</c>).</summary>
    public interface IRemoteDeployer
    {
        Task<DeployOutcome> DeployAsync(
            IReadOnlyList<string> rcPaths,
            CancellationToken cancellationToken = default,
            IProgress<string> statusProgress = null);
    }

    public enum DeployTransport
    {
        /// <summary>No external deploy (e.g. no paths).</summary>
        Api,

        /// <summary><c>ugs deploy</c> CLI.</summary>
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

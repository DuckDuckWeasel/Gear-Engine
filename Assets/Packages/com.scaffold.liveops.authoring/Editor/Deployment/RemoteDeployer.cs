using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.DeploymentApi.Editor;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Tries <see cref="IDeploymentWindow.Deploy(System.Collections.Generic.IReadOnlyList{string},System.Threading.CancellationToken)"/>;
    /// falls back to <c>ugs deploy</c> on the PATH.
    /// </summary>
    public sealed class RemoteDeployer : IRemoteDeployer
    {
        public async Task<DeployOutcome> DeployAsync(IReadOnlyList<string> rcPaths, CancellationToken cancellationToken = default)
        {
            if (rcPaths == null || rcPaths.Count == 0)
            {
                return new DeployOutcome(true, "No paths to deploy.", DeployTransport.Api);
            }

            try
            {
                IDeploymentWindow window = TryGetDeploymentWindow();
                if (window != null)
                {
                    await window.Deploy(rcPaths, cancellationToken);
                    return new DeployOutcome(true, "Deployment API finished.", DeployTransport.Api);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[LiveOps Config] Deployment API failed, trying ugs CLI: {ex.Message}");
            }

            return await DeployViaUgsCliAsync(rcPaths, cancellationToken);
        }

        private static IDeploymentWindow TryGetDeploymentWindow()
        {
            try
            {
                IDeploymentWindow w = Deployments.Instance?.DeploymentWindow;
                if (w == null)
                {
                    return null;
                }

                _ = w.GetDeploymentDefinitions();
                return w;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[LiveOps Config] Could not initialize deployment window API: {ex.Message}");
                return null;
            }
        }

        private static async Task<DeployOutcome> DeployViaUgsCliAsync(
            IReadOnlyList<string> rcPaths,
            CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
            bool allOk = true;

            foreach (string path in rcPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    log.AppendLine($"Missing file: {path}");
                    allOk = false;
                    continue;
                }

                try
                {
                    string full = Path.GetFullPath(path);
                    var psi = new ProcessStartInfo
                    {
                        FileName = "ugs",
                        Arguments = $"deploy \"{full}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null)
                    {
                        log.AppendLine("Could not start ugs process.");
                        allOk = false;
                        continue;
                    }

                    string stdout = string.Empty;
                    string stderr = string.Empty;
                    await Task.Run(
                        () =>
                        {
                            stdout = proc.StandardOutput.ReadToEnd();
                            stderr = proc.StandardError.ReadToEnd();
                            proc.WaitForExit();
                        },
                        cancellationToken);

                    log.AppendLine(stdout);
                    if (!string.IsNullOrEmpty(stderr))
                    {
                        log.AppendLine(stderr);
                    }

                    if (proc.ExitCode != 0)
                    {
                        allOk = false;
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LiveOps Config] ugs deploy failed for '{path}': {ex.Message}\n{ex.StackTrace}");
                    log.AppendLine(ex.Message);
                    allOk = false;
                }
            }

            return new DeployOutcome(allOk, log.ToString().Trim(), DeployTransport.Cli);
        }
    }
}

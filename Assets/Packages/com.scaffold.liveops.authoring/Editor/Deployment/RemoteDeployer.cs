using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Deploys <c>.rc</c> files with the Unity Gaming Services CLI: <c>ugs deploy</c> with
    /// <c>--project-id</c> and <c>--environment-name</c> from the linked editor project (same source
    /// as the LiveOps window). On Windows, runs through <c>cmd /c</c> so <c>ugs.cmd</c> from npm is found.
    /// Authentication is separate from the Editor (service account; see <c>UgsCliAuthenticationHint</c> in this type).
    /// </summary>
    public sealed class RemoteDeployer : IRemoteDeployer
    {
        /// <summary>Appended to deploy log when the CLI reports that no service account session exists.</summary>
        internal const string UgsCliAuthenticationHint =
            "The Unity Gaming Services CLI does not use the Editor’s sign-in. One-time: open a terminal (same user as this PC) and run: ugs login\n"
            + "and paste a Service Account Key ID and Secret from the Unity dashboard (UGS project → a team / service-accounts with deploy rights).\n"
            + "Or set UGS_CLI_SERVICE_KEY_ID and UGS_CLI_SERVICE_SECRET_KEY in your user or system environment, then deploy again. "
            + "Reference: services.docs.unity.com → UGS CLI → Login.";

        /// <summary>Appended when the CLI returns 403 / No Permissions (e.g. GetEnvironments).</summary>
        internal const string UgsCliProjectRolesHint =
            "The service account you used for ugs login is signed in, but it does not have project roles for this UGS project (or the wrong project id). "
            + "The CLI must use the same Project ID as Edit → Project Settings → Services, and that service account needs roles assigned for that project in the Unity Dashboard (Service accounts).\n"
            + "For Remote Config deploy (.rc), Unity documents: assign at least Unity Environments Admin and Remote Config Admin for the Deploy command. "
            + "Details: services.docs.unity.com → UGS CLI → Troubleshooting → Project roles (Deploy command), and unauthorized-error-403.";

        public async Task<DeployOutcome> DeployAsync(
            IReadOnlyList<string> rcPaths,
            CancellationToken cancellationToken = default,
            IProgress<string> statusProgress = null)
        {
            if (rcPaths == null || rcPaths.Count == 0)
            {
                return new DeployOutcome(true, "No paths to deploy.", DeployTransport.Api);
            }

            if (!UgsCliDeployContext.TryGetForDeploy(out string projectId, out string environmentName, out string contextError))
            {
                return new DeployOutcome(false, contextError, DeployTransport.Cli);
            }

            return await DeployViaUgsCliAsync(
                rcPaths,
                projectId,
                environmentName,
                cancellationToken,
                statusProgress);
        }

        private static async Task<DeployOutcome> DeployViaUgsCliAsync(
            IReadOnlyList<string> rcPaths,
            string projectId,
            string environmentName,
            CancellationToken cancellationToken,
            IProgress<string> statusProgress)
        {
            var log = new StringBuilder();
            bool allOk = true;
            int total = rcPaths.Count;
            int index = 0;

            foreach (string path in rcPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                index++;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    log.AppendLine($"Missing file: {path}");
                    allOk = false;
                    statusProgress?.Report($"Upload failed ({index}/{total}): missing file — {path}");
                    continue;
                }

                try
                {
                    string fileName = Path.GetFileName(path);
                    statusProgress?.Report($"Uploading {index}/{total}: {fileName}…");
                    string full = Path.GetFullPath(path);
                    using Process proc = CreateUgsProcess(full, projectId, environmentName);
                    if (proc == null)
                    {
                        log.AppendLine(
                            "Could not start ugs. Install: npm i -g ugs — then open a new terminal and confirm `ugs --version`. The Editor must inherit PATH (restart Unity after installing if needed).");
                        allOk = false;
                        statusProgress?.Report($"Upload failed ({index}/{total}): could not start ugs CLI");
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

                    log.AppendLine(FilterUgsCliNoise(stdout));
                    string errFiltered = FilterUgsCliNoise(stderr);
                    if (!string.IsNullOrEmpty(errFiltered))
                    {
                        log.AppendLine(errFiltered);
                    }

                    if (proc.ExitCode != 0)
                    {
                        allOk = false;
                        statusProgress?.Report($"Upload failed ({index}/{total}): {fileName} (exit {proc.ExitCode})");
                    }
                    else
                    {
                        statusProgress?.Report($"Uploaded {index}/{total}: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(
                        $"[LiveOps Config] ugs deploy failed for '{path}': {ex.Message}\n{ex.StackTrace}");
                    log.AppendLine(ex.Message);
                    allOk = false;
                    statusProgress?.Report($"Upload failed ({index}/{total}): {Path.GetFileName(path)} — {ex.Message}");
                }
            }

            statusProgress?.Report(allOk ? "Deploy finished." : "Deploy finished with errors — see Console for details.");

            string text = log.ToString().Trim();
            if (!allOk)
            {
                if (UgsOutputIndicatesNotLoggedIn(text))
                {
                    text = text + Environment.NewLine + Environment.NewLine + UgsCliAuthenticationHint;
                }
                else if (UgsOutputIndicatesForbiddenOrMissingProjectRoles(text))
                {
                    text = text + Environment.NewLine + Environment.NewLine + UgsCliProjectRolesHint;
                }
            }

            return new DeployOutcome(allOk, text, DeployTransport.Cli);
        }

        private static bool UgsOutputIndicatesNotLoggedIn(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                return false;
            }

            return log.IndexOf("not logged into any service account", StringComparison.OrdinalIgnoreCase) >= 0
                || log.IndexOf("Please login using the", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool UgsOutputIndicatesForbiddenOrMissingProjectRoles(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                return false;
            }

            bool forbidden =
                log.IndexOf("Status: 403", StringComparison.OrdinalIgnoreCase) >= 0
                || log.IndexOf("Code: 56", StringComparison.Ordinal) >= 0;
            bool noPerms = log.IndexOf("No Permissions", StringComparison.Ordinal) >= 0
                || log.IndexOf("Forbidden", StringComparison.Ordinal) >= 0;
            bool envCall = log.IndexOf("GetEnvironments", StringComparison.Ordinal) >= 0;
            return (forbidden && noPerms) || (noPerms && envCall);
        }

        /// <summary>Drops Node deprecation noise from the <c>ugs</c> npm wrapper (not actionable).</summary>
        private static string FilterUgsCliNoise(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var sb = new StringBuilder();
            foreach (string line in s.Split(new[] { '\r', '\n' }, StringSplitOptions.None))
            {
                if (line.IndexOf("DeprecationWarning", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (line.IndexOf("[DEP0", StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                if (line.IndexOf("trace-deprecation", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (line.IndexOf("(Use `node", StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                sb.AppendLine(line);
            }

            return sb.ToString().Trim();
        }

        private static Process CreateUgsProcess(string fullRcPath, string projectId, string environmentName)
        {
            string pathQ = EscapeForCmdQuotedArg(fullRcPath);
            string pidQ = EscapeForCmdQuotedArg(projectId);
            string envQ = EscapeForCmdQuotedArg(environmentName);
            try
            {
                if (Path.DirectorySeparatorChar == '\\')
                {
                    string cmdLine =
                        $"/d /c ugs deploy \"{pathQ}\" --project-id \"{pidQ}\" --environment-name \"{envQ}\"";
                    return Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                            Arguments = cmdLine,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                        });
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "ugs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                psi.ArgumentList.Add("deploy");
                psi.ArgumentList.Add(fullRcPath);
                psi.ArgumentList.Add("--project-id");
                psi.ArgumentList.Add(projectId);
                psi.ArgumentList.Add("--environment-name");
                psi.ArgumentList.Add(environmentName);
                
                return Process.Start(psi);
            }
            catch
            {
                return null;
            }
        }

        private static string EscapeForCmdQuotedArg(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\"", "\\\"", StringComparison.Ordinal);
        }
    }
}


using System;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.DeploymentApi.Editor;
using UnityEditor;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>Resolves UGS project id and environment <b>name</b> for <c>ugs deploy --project-id --environment-name</c>.</summary>
    internal static class UgsCliDeployContext
    {
        public static bool TryGetForDeploy(out string projectId, out string environmentName, out string userMessage)
        {
            projectId = CloudProjectSettings.projectId;
            if (string.IsNullOrEmpty(projectId))
            {
                environmentName = null;
                userMessage =
                    "This Unity project is not linked to Unity Gaming Services. Use Edit → Project Settings → Services to sign in and link a project.";
                return false;
            }

            IEnvironmentsApi envApi = null;
            string displayName = null;
            try
            {
                envApi = EnvironmentsApi.Instance;
                displayName = envApi?.ActiveEnvironmentName;
            }
            catch
            {
            }

            string deployEnvId = null;
            try
            {
                deployEnvId = Deployments.Instance?.EnvironmentProvider?.Current;
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(deployEnvId) && envApi?.Environments != null)
            {
                try
                {
                    if (Guid.TryParse(deployEnvId, out Guid gid))
                    {
                        foreach (EnvironmentInfo info in envApi.Environments)
                        {
                            if (info.Id == gid)
                            {
                                displayName = info.Name;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(displayName))
            {
                environmentName = null;
                userMessage =
                    "No active UGS environment name. Set the Editor environment in Edit → Project Settings → Services, or the environment selector in the Services window (signed in). The ugs CLI needs an environment name (e.g. production, development).";
                return false;
            }

            environmentName = displayName;
            userMessage = null;
            return true;
        }
    }
}

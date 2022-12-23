using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Data
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetProjectsAsync();

        Task<Project> GetProjectAsync(Guid id);

        Task<IEnumerable<ProjectVersion>> GetVersionsAsync(Guid projectId);

        Task<ProjectVersion> UpdateVersionAsync(Guid projectId, Guid versionId, ProjectVersionUpdateRequest updateRequest);

        Task<IEnumerable<VersionDeployment>> GetVersionDeploymentsAsync(Guid projectId, Guid versionId);

        Task<VersionDeployment> AddVersionDeploymentAsync(Guid projectId, Guid versionId, AddVersionDeploymentRequest deploymentRequest);

        Task RemoveVersionDeploymentAsync(Guid projectId, Guid versionId, Guid deploymentId);

    }
}

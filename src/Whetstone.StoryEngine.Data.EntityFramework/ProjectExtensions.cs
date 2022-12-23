using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Amazon.RDS.Model.Internal.MarshallTransformations;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    internal static class ProjectExtensions
    {

        internal static ProjectVersion ToProjectVersion(this DataTitleVersion dataVersion, Guid projectId)
        {

            ProjectVersion retVersion = new ProjectVersion
            {
                Id = dataVersion.Id,
                Version = dataVersion.Version,
                Description = dataVersion.Description,
                LogFullClientMessages = dataVersion.LogFullClientMessages,
                ProjectId = projectId
            };

            return retVersion;
        }


        internal static VersionDeployment ToVersionDeployment(this DataTitleVersionDeployment dataDeployment,
            Guid projectId)
        {


            if (!dataDeployment.Id.HasValue)
                throw new ArgumentException($"{nameof(dataDeployment)} must have an Id");

            if(!dataDeployment.PublishDate.HasValue)
                throw new ArgumentException($"{nameof(dataDeployment)} must have a PublishDate");

            VersionDeployment retVersion = new VersionDeployment
            {
                Id = dataDeployment.Id.Value,
                Alias = dataDeployment.Alias,
                VersionId = dataDeployment.VersionId,
                ProjectId = projectId,
                PublishDate = dataDeployment.PublishDate.Value,
                ClientId = dataDeployment.ClientIdentifier,
                ClientType = dataDeployment.Client
            };

            return retVersion;
        }

    }
}

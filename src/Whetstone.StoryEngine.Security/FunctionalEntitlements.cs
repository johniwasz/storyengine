using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Security
{
    public static class FunctionalEntitlements
    {
        #region project entitlements

        public const string PermissionCreateProject = "project-create";

        public const string PermissionViewProject = "project-view";

        public const string PermissionViewVersion = "version-view";

        public const string IsRegisteredUser = "is-registered-user";

        public static readonly OperationAuthorizationRequirement CreateProject =
            new OperationAuthorizationRequirement { Name = nameof(CreateProject) };

        public static readonly OperationAuthorizationRequirement UpdateProject =
            new OperationAuthorizationRequirement { Name = nameof(UpdateProject) };

        public static readonly OperationAuthorizationRequirement DeleteProject =
            new OperationAuthorizationRequirement { Name = nameof(DeleteProject) };

        public static readonly OperationAuthorizationRequirement ViewProject =
            new OperationAuthorizationRequirement { Name = nameof(ViewProject) };

        public static readonly OperationAuthorizationRequirement ViewReports =
            new OperationAuthorizationRequirement { Name = nameof(ViewReports) };

        public static readonly OperationAuthorizationRequirement ExportReports =
            new OperationAuthorizationRequirement { Name = nameof(ExportReports) };

        #endregion


        #region version entitlements

        public static OperationAuthorizationRequirement CreateVersion =
            new OperationAuthorizationRequirement { Name = nameof(CreateVersion) };

        public static OperationAuthorizationRequirement UpdateVersion =
            new OperationAuthorizationRequirement { Name = nameof(UpdateVersion) };

        public static OperationAuthorizationRequirement ViewVersion =
            new OperationAuthorizationRequirement { Name = nameof(ViewVersion) };

        public static OperationAuthorizationRequirement AddAudioFile =
            new OperationAuthorizationRequirement { Name = nameof(AddAudioFile) };

        public static OperationAuthorizationRequirement UpdateAudioFile =
            new OperationAuthorizationRequirement { Name = nameof(UpdateAudioFile) };

        public static OperationAuthorizationRequirement DeleteAudioFile =
            new OperationAuthorizationRequirement { Name = nameof(DeleteAudioFile) };

        public static OperationAuthorizationRequirement PlayAudioFile =
            new OperationAuthorizationRequirement { Name = nameof(PlayAudioFile) };

        public static OperationAuthorizationRequirement AddImageFile =
            new OperationAuthorizationRequirement { Name = nameof(AddImageFile) };

        public static OperationAuthorizationRequirement ViewImageFile =
            new OperationAuthorizationRequirement { Name = nameof(ViewImageFile) };

        public static OperationAuthorizationRequirement DeleteImageFile =
            new OperationAuthorizationRequirement { Name = nameof(DeleteImageFile) };

        public static OperationAuthorizationRequirement UpdateImageFile =
            new OperationAuthorizationRequirement { Name = nameof(UpdateImageFile) };

        #endregion


        #region deployment entitlements

        public static OperationAuthorizationRequirement DeployVersion =
            new OperationAuthorizationRequirement { Name = nameof(DeployVersion) };

        public static OperationAuthorizationRequirement ViewVersionDeployment =
            new OperationAuthorizationRequirement { Name = nameof(ViewVersionDeployment) };

        public static OperationAuthorizationRequirement RemoveVersionDeployment =
            new OperationAuthorizationRequirement { Name = nameof(RemoveVersionDeployment) };

        public static OperationAuthorizationRequirement EditChannelDeploymentMetadata =
            new OperationAuthorizationRequirement { Name = nameof(EditChannelDeploymentMetadata) };

        public static OperationAuthorizationRequirement ViewChannelDeploymentMetadata =
            new OperationAuthorizationRequirement { Name = nameof(ViewChannelDeploymentMetadata) };

        #endregion


        #region organization entitlements

        public static OperationAuthorizationRequirement CreateGroup =
            new OperationAuthorizationRequirement { Name = nameof(CreateGroup) };

        public static OperationAuthorizationRequirement EditGroup =
            new OperationAuthorizationRequirement { Name = nameof(EditGroup) };

        public static OperationAuthorizationRequirement DeleteGroup =
            new OperationAuthorizationRequirement { Name = nameof(DeleteGroup) };

        public static OperationAuthorizationRequirement ViewGroup =
            new OperationAuthorizationRequirement { Name = nameof(ViewGroup) };

        public static OperationAuthorizationRequirement AddUserToGroup =
            new OperationAuthorizationRequirement { Name = nameof(AddUserToGroup) };

        public static OperationAuthorizationRequirement RemoveUserFromGroup =
            new OperationAuthorizationRequirement { Name = nameof(RemoveUserFromGroup) };

        public static OperationAuthorizationRequirement InviteUserToOrganization =
            new OperationAuthorizationRequirement { Name = nameof(InviteUserToOrganization) };

        #endregion
    }
}

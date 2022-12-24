using Microsoft.EntityFrameworkCore.Migrations;


namespace Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations
{
    public static class GrantExtension
    {
        public static MigrationBuilder GrantPermissions(
            this MigrationBuilder migrationBuilder,
            string tableName,
            Permission userPermissions,
            string userName,
            string schema)
        {
            migrationBuilder.Operations.Add(
                new GrantPermissionOperation(
                 tableName, userName, userPermissions, schema));

            return migrationBuilder;
        }
    }
}

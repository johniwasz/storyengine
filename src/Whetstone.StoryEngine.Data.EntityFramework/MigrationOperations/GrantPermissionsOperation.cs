using System;


namespace Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations
{

    [Flags]
    public enum Permission
    {
        Select = 1,
        Update = 2,
        Insert = 4,
        Delete = 8
    }

    public class GrantPermissionOperation : Microsoft.EntityFrameworkCore.Migrations.Operations.MigrationOperation
    {
        public GrantPermissionOperation(string table, string user, Permission permission, string schema)
            : base()
        {
            Table = table;
            User = user;
            Permission = permission;
            Schema = schema;
        }

        public string Schema { get; private set; }
        public string Table { get; private set; }
        public string User { get; private set; }
        public Permission Permission { get; private set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations;

namespace Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations
{
    public class PostGresEngineMigrationGenerator : Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.NpgsqlMigrationsSqlGenerator
    {

        public PostGresEngineMigrationGenerator(MigrationsSqlGeneratorDependencies dependencies,
            INpgsqlSingletonOptions npgsqlOptions)
            : base(dependencies, npgsqlOptions)
        {
           
        }


        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            if (operation is GrantPermissionOperation grantPermissionOperation)
            {
                Generate(grantPermissionOperation, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }


        private void Generate(
            GrantPermissionOperation operation,
            MigrationCommandListBuilder builder)
        {
            
            var sqlHelper = Dependencies.SqlGenerationHelper;
            var stringMapping = Dependencies.TypeMappingSource.FindMapping(typeof(string));
            //GRANT SELECT, UPDATE, INSERT, DELETE ON whetstone.group_role_xrefs TO storyengineuser;

            List<string> perms = new List<string>();

            if(operation.Permission.HasFlag(Permission.Update))
                perms.Add(Permission.Update.ToString().ToUpper());

            if (operation.Permission.HasFlag(Permission.Insert))
                perms.Add(Permission.Insert.ToString().ToUpper());

            if (operation.Permission.HasFlag(Permission.Delete))
                perms.Add(Permission.Delete.ToString().ToUpper());

            if (operation.Permission.HasFlag(Permission.Select))
                perms.Add(Permission.Select.ToString().ToUpper());

            builder
                .Append("GRANT ")
                .Append(string.Join(',', perms))
                .Append(" ON ")
                .Append(operation.Schema)
                .Append(".")
                .Append(operation.Table)
                .Append(" TO ")
                .Append(operation.User)
                .AppendLine(sqlHelper.StatementTerminator)
                .EndCommand();
        }


    }

}

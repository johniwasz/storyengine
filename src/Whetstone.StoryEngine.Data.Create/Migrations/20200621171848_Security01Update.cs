using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class Security01Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funcentitlements",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false),
                    claim = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcentitlements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptionlevels",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionlevels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    cognito_sub = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funcent_role_xrefs",
                schema: "whetstone",
                columns: table => new
                {
                    func_entitlement_id = table.Column<Guid>(nullable: false),
                    role_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_funcentitlement", x => new { x.role_id, x.func_entitlement_id });
                    table.ForeignKey(
                        name: "FK_funcent_role_xrefs_funcentitlements_func_entitlement_id",
                        column: x => x.func_entitlement_id,
                        principalSchema: "whetstone",
                        principalTable: "funcentitlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_funcent_role_xrefs_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "whetstone",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false),
                    subscriptionlevel_id = table.Column<Guid>(nullable: false),
                    isenabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_subscriptionlevels_subscriptionlevel_id",
                        column: x => x.subscriptionlevel_id,
                        principalSchema: "whetstone",
                        principalTable: "subscriptionlevels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false),
                    organization_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "whetstone",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_role_xrefs",
                schema: "whetstone",
                columns: table => new
                {
                    group_id = table.Column<Guid>(nullable: false),
                    role_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_role", x => new { x.group_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_group_role_xrefs_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "whetstone",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_group_role_xrefs_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "whetstone",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "title_group_xrefs",
                schema: "whetstone",
                columns: table => new
                {
                    title_id = table.Column<Guid>(nullable: false),
                    group_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_title", x => new { x.group_id, x.title_id });
                    table.ForeignKey(
                        name: "FK_title_group_xrefs_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "whetstone",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_title_group_xrefs_titles_title_id",
                        column: x => x.title_id,
                        principalSchema: "whetstone",
                        principalTable: "titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_group_xrefs",
                schema: "whetstone",
                columns: table => new
                {
                    user_id = table.Column<Guid>(nullable: false),
                    group_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_group", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_user_group_xrefs_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "whetstone",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_group_xrefs_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "whetstone",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_funcent_role_xrefs_func_entitlement_id",
                schema: "whetstone",
                table: "funcent_role_xrefs",
                column: "func_entitlement_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_role_xrefs_role_id",
                schema: "whetstone",
                table: "group_role_xrefs",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_organization_id",
                schema: "whetstone",
                table: "groups",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_subscriptionlevel_id",
                schema: "whetstone",
                table: "organizations",
                column: "subscriptionlevel_id");

            migrationBuilder.CreateIndex(
                name: "IX_title_group_xrefs_title_id",
                schema: "whetstone",
                table: "title_group_xrefs",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_group_xrefs_user_id",
                schema: "whetstone",
                table: "user_group_xrefs",
                column: "user_id");

            string scriptText = MigrationFunctionExtensions.GetScriptContent("addservicelevels");
            migrationBuilder.Sql(scriptText);

            string userName = "storyengineuser";
            string schema = "whetstone";
            migrationBuilder.GrantPermissions("organizations",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("subscriptionlevels",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("roles",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("groups",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("users",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("group_role_xrefs",
                Permission.Update | Permission.Insert | Permission.Select | Permission.Delete, userName, schema);

            migrationBuilder.GrantPermissions("title_group_xrefs",
                Permission.Update | Permission.Insert | Permission.Select | Permission.Delete, userName, schema);

            migrationBuilder.GrantPermissions("user_group_xrefs",
                Permission.Update | Permission.Insert | Permission.Select | Permission.Delete, userName, schema);

            migrationBuilder.GrantPermissions("funcent_role_xrefs",
                Permission.Update | Permission.Insert | Permission.Select | Permission.Delete, userName, schema);

            migrationBuilder.GrantPermissions("funcentitlements",
                Permission.Update | Permission.Insert | Permission.Select | Permission.Delete, userName, schema);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "funcent_role_xrefs",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "group_role_xrefs",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "title_group_xrefs",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "user_group_xrefs",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "funcentitlements",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "users",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "subscriptionlevels",
                schema: "whetstone");
        }
    }
}

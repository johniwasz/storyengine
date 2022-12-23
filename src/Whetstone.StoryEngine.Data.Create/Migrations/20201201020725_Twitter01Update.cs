using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class Twitter01Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "org_twitterapplications",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    organization_id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    twitter_app_id = table.Column<long>(nullable: false),
                    isenabled = table.Column<bool>(nullable: false),
                    isdeleted = table.Column<bool>(nullable: false),
                    title_version_id = table.Column<Guid>(nullable: true),
                    isverified = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_twitterapplications", x => x.id);
                    table.ForeignKey(
                        name: "FK_org_twitterapplications_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "whetstone",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_org_twitterapplications_titleversions_title_version_id",
                        column: x => x.title_version_id,
                        principalSchema: "whetstone",
                        principalTable: "titleversions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "org_twittercredentials",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    twitterapplication_id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: false),
                    consumer_key = table.Column<string>(nullable: false),
                    consumer_secret = table.Column<string>(nullable: false),
                    access_token = table.Column<string>(nullable: false),
                    access_token_secret = table.Column<string>(nullable: false),
                    bearer_token = table.Column<string>(nullable: false),
                    isenabled = table.Column<bool>(nullable: false),
                    isdeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_twittercredentials", x => x.id);
                    table.ForeignKey(
                        name: "FK_org_twittercredentials_org_twitterapplications_twitterappli~",
                        column: x => x.twitterapplication_id,
                        principalSchema: "whetstone",
                        principalTable: "org_twitterapplications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "twittersubscriptions",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    application_id = table.Column<Guid>(nullable: false),
                    twitter_user_id = table.Column<long>(nullable: false),
                    enable_autofollowback = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twittersubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_twittersubscriptions_org_twitterapplications_application_id",
                        column: x => x.application_id,
                        principalSchema: "whetstone",
                        principalTable: "org_twitterapplications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_org_twitterapplications_organization_id",
                schema: "whetstone",
                table: "org_twitterapplications",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_twitterapplications_title_version_id",
                schema: "whetstone",
                table: "org_twitterapplications",
                column: "title_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_twittercredentials_twitterapplication_id",
                schema: "whetstone",
                table: "org_twittercredentials",
                column: "twitterapplication_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_twittersubscriptions_application_id",
                schema: "whetstone",
                table: "twittersubscriptions",
                column: "application_id");

            string scriptText = MigrationFunctionExtensions.GetScriptContent("addtwitterentitlements");
            migrationBuilder.Sql(scriptText);

            string userName = "storyengineuser";
            string schema = "whetstone";
            migrationBuilder.GrantPermissions("org_twittercredentials",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("twittersubscriptions",
                Permission.Update | Permission.Insert | Permission.Select, userName, schema);

            migrationBuilder.GrantPermissions("org_twitterapplications",
             Permission.Update | Permission.Insert | Permission.Select, userName, schema);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string scriptText = MigrationFunctionExtensions.GetScriptContent("deletetwitterentitlements");
            migrationBuilder.Sql(scriptText);

            migrationBuilder.DropTable(
                name: "org_twittercredentials",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "twittersubscriptions",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "org_twitterapplications",
                schema: "whetstone");
        }
    }
}

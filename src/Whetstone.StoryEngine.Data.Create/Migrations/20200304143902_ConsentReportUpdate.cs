using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class ConsentReportUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            string scriptText = MigrationFunctionExtensions.GetScriptContent("add_messageconsentview_2");
            migrationBuilder.Sql(scriptText);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Roll back to the prior function.
            string scriptText = MigrationFunctionExtensions.GetScriptContent("add_messageconsentview");
            migrationBuilder.Sql(scriptText);
        }
    }
}

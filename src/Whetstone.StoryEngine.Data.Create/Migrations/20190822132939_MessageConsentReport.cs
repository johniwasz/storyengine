using Microsoft.EntityFrameworkCore.Migrations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class MessageConsentReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isfirstsession",
                schema: "whetstone",
                table: "engine_session",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outboundmessage_logs_logtime",
                schema: "whetstone",
                table: "outboundmessage_logs",
                column: "logtime");

            string scriptText = MigrationFunctionExtensions.GetScriptContent("add_messageconsentview");
            migrationBuilder.Sql(scriptText);

            string functionText = MigrationFunctionExtensions.GetFunctionContent("add_getconsentreport");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("add_intentaction_isnewuser");
            migrationBuilder.Sql(functionText);



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string functionText = MigrationFunctionExtensions.GetFunctionContent("add_intentaction_isnewuser");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("drop_getconsentreport");
            migrationBuilder.Sql(functionText);

            string scriptText = MigrationFunctionExtensions.GetScriptContent("drop_messageconsentview");
            migrationBuilder.Sql(scriptText);

            migrationBuilder.DropIndex(
                name: "IX_outboundmessage_logs_logtime",
                schema: "whetstone",
                table: "outboundmessage_logs");

            migrationBuilder.DropColumn(
                name: "isfirstsession",
                schema: "whetstone",
                table: "engine_session");
        }
    }
}

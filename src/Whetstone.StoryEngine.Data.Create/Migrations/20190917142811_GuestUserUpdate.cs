using Microsoft.EntityFrameworkCore.Migrations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class GuestUserUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isguest",
                schema: "whetstone",
                table: "title_clientusers",
                nullable: true);

            string scriptText = MigrationFunctionExtensions.GetFunctionContent("add_intentaction_isguest");
            migrationBuilder.Sql(scriptText);

            string functionText = MigrationFunctionExtensions.GetFunctionContent("addupdatetitleuser_isguest");
            migrationBuilder.Sql(functionText);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            string functionText = MigrationFunctionExtensions.GetFunctionContent("addupdatetitleuser_isguest");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("add_intentaction_isguest");
            migrationBuilder.Sql(functionText);

            migrationBuilder.DropColumn(
                name: "isguest",
                schema: "whetstone",
                table: "title_clientusers");
        }
    }
}

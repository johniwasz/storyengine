using Microsoft.EntityFrameworkCore.Migrations;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class MsgBackKeyUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_outbound_messages_outbound_batches_id",
                schema: "whetstone",
                table: "outbound_messages");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_messages_outboundbatchrecordid",
                schema: "whetstone",
                table: "outbound_messages",
                column: "outboundbatchrecordid");

            migrationBuilder.AddForeignKey(
                name: "FK_outbound_messages_outbound_batches_outboundbatchrecordid",
                schema: "whetstone",
                table: "outbound_messages",
                column: "outboundbatchrecordid",
                principalSchema: "whetstone",
                principalTable: "outbound_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_outbound_messages_outbound_batches_outboundbatchrecordid",
                schema: "whetstone",
                table: "outbound_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbound_messages_outboundbatchrecordid",
                schema: "whetstone",
                table: "outbound_messages");

            migrationBuilder.AddForeignKey(
                name: "FK_outbound_messages_outbound_batches_id",
                schema: "whetstone",
                table: "outbound_messages",
                column: "id",
                principalSchema: "whetstone",
                principalTable: "outbound_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

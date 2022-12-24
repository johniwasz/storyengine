using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "whetstone");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "phonenumbers",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    phonenumber = table.Column<string>(nullable: false),
                    phonetype = table.Column<int>(nullable: false),
                    isverified = table.Column<bool>(nullable: false),
                    nationalformat = table.Column<string>(nullable: true),
                    cangetsmsmessage = table.Column<bool>(nullable: false),
                    countrycode = table.Column<string>(nullable: true),
                    carriercountrycode = table.Column<string>(nullable: true),
                    carriernetworkcode = table.Column<string>(nullable: true),
                    carriername = table.Column<string>(nullable: true),
                    carriererrorcode = table.Column<string>(nullable: true),
                    url = table.Column<string>(nullable: true),
                    registeredname = table.Column<string>(nullable: true),
                    registeredtype = table.Column<string>(nullable: true),
                    registerederrorcode = table.Column<string>(nullable: true),
                    phoneservice = table.Column<string>(nullable: true),
                    systemstatus = table.Column<int>(nullable: false),
                    createdate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phonenumbers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "titles",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    shortname = table.Column<string>(nullable: false),
                    title = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_titles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "title_clientusers",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    hashkey = table.Column<string>(nullable: true),
                    titleid = table.Column<Guid>(nullable: false),
                    clientuserid = table.Column<string>(nullable: false),
                    client = table.Column<int>(nullable: false),
                    userlocale = table.Column<string>(nullable: true),
                    storynodename = table.Column<string>(nullable: true),
                    nodename = table.Column<string>(nullable: true),
                    createdtime = table.Column<DateTime>(nullable: false),
                    lastaccesseddate = table.Column<DateTime>(nullable: false),
                    titlecrumbs = table.Column<string>(type: "jsonb", nullable: true),
                    permanenttitlecrumbs = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_title_clientusers", x => x.id);
                    table.ForeignKey(
                        name: "FK_title_clientusers_titles_titleid",
                        column: x => x.titleid,
                        principalSchema: "whetstone",
                        principalTable: "titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "titleversions",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    titleid = table.Column<Guid>(nullable: false),
                    version = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    isdeleted = table.Column<bool>(nullable: false),
                    deletedate = table.Column<DateTime>(nullable: true),
                    logfullclientmessages = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_titleversions", x => x.id);
                    table.ForeignKey(
                        name: "FK_titleversions_titles_titleid",
                        column: x => x.titleid,
                        principalSchema: "whetstone",
                        principalTable: "titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_session",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    titleuserid = table.Column<Guid>(nullable: false),
                    sessionid = table.Column<string>(nullable: true),
                    userid = table.Column<string>(nullable: true),
                    userlocale = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    deploymentid = table.Column<Guid>(nullable: false),
                    startdate = table.Column<DateTime>(nullable: true),
                    lastaccesseddate = table.Column<DateTime>(nullable: true),
                    sessionattributes = table.Column<Dictionary<string, string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_engine_session_title_clientusers_titleuserid",
                        column: x => x.titleuserid,
                        principalSchema: "whetstone",
                        principalTable: "title_clientusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "titleversiondeployments",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    versionid = table.Column<Guid>(nullable: false),
                    client = table.Column<int>(nullable: false),
                    clientidentifier = table.Column<string>(nullable: true),
                    alias = table.Column<string>(nullable: true),
                    publishdate = table.Column<DateTime>(nullable: true),
                    isdeleted = table.Column<bool>(nullable: false),
                    deletedate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_titleversiondeployments", x => x.id);
                    table.ForeignKey(
                        name: "FK_titleversiondeployments_titleversions_versionid",
                        column: x => x.versionid,
                        principalSchema: "whetstone",
                        principalTable: "titleversions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userphoneconsents",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    titleclientuserid = table.Column<Guid>(nullable: false),
                    phoneid = table.Column<Guid>(nullable: false),
                    titleversionid = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    isconsentgiven = table.Column<bool>(nullable: false),
                    smsconsentdate = table.Column<DateTime>(nullable: true),
                    enginerequestid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userphoneconsents", x => x.id);
                    table.ForeignKey(
                        name: "FK_userphoneconsents_phonenumbers_phoneid",
                        column: x => x.phoneid,
                        principalSchema: "whetstone",
                        principalTable: "phonenumbers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userphoneconsents_title_clientusers_titleclientuserid",
                        column: x => x.titleclientuserid,
                        principalSchema: "whetstone",
                        principalTable: "title_clientusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userphoneconsents_titleversions_titleversionid",
                        column: x => x.titleversionid,
                        principalSchema: "whetstone",
                        principalTable: "titleversions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_requestaudit",
                schema: "whetstone",
                columns: table => new
                {
                    requestid = table.Column<string>(nullable: false),
                    sessionid = table.Column<Guid>(nullable: false),
                    id = table.Column<Guid>(nullable: false),
                    intentname = table.Column<string>(nullable: true),
                    slots = table.Column<Dictionary<string, string>>(nullable: true),
                    selectiontime = table.Column<DateTime>(nullable: false),
                    processduration = table.Column<long>(nullable: false),
                    prenodeactionlog = table.Column<string>(nullable: true),
                    postnodeactionlog = table.Column<string>(nullable: true),
                    mappednode = table.Column<string>(nullable: true),
                    requesttype = table.Column<int>(nullable: false),
                    canfulfill = table.Column<int>(nullable: true),
                    slotfulfillment = table.Column<string>(type: "jsonb", nullable: true),
                    requestattributes = table.Column<Dictionary<string, string>>(nullable: true),
                    rawtext = table.Column<string>(nullable: true),
                    intentconfidence = table.Column<float>(nullable: true),
                    responsebody = table.Column<string>(nullable: true),
                    requestbody = table.Column<string>(nullable: true),
                    engineerror = table.Column<string>(nullable: true),
                    responseconversionerror = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_requestaudit", x => new { x.sessionid, x.requestid });
                    table.UniqueConstraint("AK_engine_requestaudit_id", x => x.id);
                    table.ForeignKey(
                        name: "FK_engine_requestaudit_engine_session_sessionid",
                        column: x => x.sessionid,
                        principalSchema: "whetstone",
                        principalTable: "engine_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outbound_batches",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    titleclientid = table.Column<Guid>(nullable: false),
                    enginerequestid = table.Column<Guid>(nullable: true),
                    smstonumberid = table.Column<Guid>(nullable: true),
                    smsfromnumberid = table.Column<Guid>(nullable: true),
                    consentid = table.Column<Guid>(nullable: true),
                    smsprovider = table.Column<int>(nullable: true),
                    allsent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_batches_userphoneconsents_consentid",
                        column: x => x.consentid,
                        principalSchema: "whetstone",
                        principalTable: "userphoneconsents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_batches_phonenumbers_smsfromnumberid",
                        column: x => x.smsfromnumberid,
                        principalSchema: "whetstone",
                        principalTable: "phonenumbers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_batches_phonenumbers_smstonumberid",
                        column: x => x.smstonumberid,
                        principalSchema: "whetstone",
                        principalTable: "phonenumbers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbound_messages",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    outboundbatchrecordid = table.Column<Guid>(nullable: false),
                    message = table.Column<string>(nullable: true),
                    tags = table.Column<Dictionary<string, string>>(nullable: true),
                    status = table.Column<int>(nullable: true),
                    providermessageid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_messages_outbound_batches_id",
                        column: x => x.id,
                        principalSchema: "whetstone",
                        principalTable: "outbound_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outboundmessage_logs",
                schema: "whetstone",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    outboundmessageid = table.Column<Guid>(nullable: false),
                    isexception = table.Column<bool>(nullable: true),
                    httpstatus = table.Column<int>(nullable: true),
                    extendedstatus = table.Column<string>(nullable: true),
                    status = table.Column<int>(nullable: false),
                    logtime = table.Column<DateTime>(nullable: false),
                    providersendduration = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outboundmessage_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_outboundmessage_logs_outbound_messages_outboundmessageid",
                        column: x => x.outboundmessageid,
                        principalSchema: "whetstone",
                        principalTable: "outbound_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_requestaudit_sessionid",
                schema: "whetstone",
                table: "engine_requestaudit",
                column: "sessionid");

            migrationBuilder.CreateIndex(
                name: "IX_engine_session_titleuserid",
                schema: "whetstone",
                table: "engine_session",
                column: "titleuserid");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_batches_consentid",
                schema: "whetstone",
                table: "outbound_batches",
                column: "consentid");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_batches_smsfromnumberid",
                schema: "whetstone",
                table: "outbound_batches",
                column: "smsfromnumberid");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_batches_smstonumberid",
                schema: "whetstone",
                table: "outbound_batches",
                column: "smstonumberid");

            migrationBuilder.CreateIndex(
                name: "IX_outboundmessage_logs_outboundmessageid",
                schema: "whetstone",
                table: "outboundmessage_logs",
                column: "outboundmessageid");

            migrationBuilder.CreateIndex(
                name: "IX_phonenumbers_phonenumber",
                schema: "whetstone",
                table: "phonenumbers",
                column: "phonenumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_title_clientusers_titleid_client_clientuserid",
                schema: "whetstone",
                table: "title_clientusers",
                columns: new[] { "titleid", "client", "clientuserid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_titles_shortname",
                schema: "whetstone",
                table: "titles",
                column: "shortname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_titleversiondeployments_versionid",
                schema: "whetstone",
                table: "titleversiondeployments",
                column: "versionid");

            migrationBuilder.CreateIndex(
                name: "IX_titleversions_titleid_version",
                schema: "whetstone",
                table: "titleversions",
                columns: new[] { "titleid", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userphoneconsents_phoneid",
                schema: "whetstone",
                table: "userphoneconsents",
                column: "phoneid");

            migrationBuilder.CreateIndex(
                name: "IX_userphoneconsents_titleclientuserid",
                schema: "whetstone",
                table: "userphoneconsents",
                column: "titleclientuserid");

            migrationBuilder.CreateIndex(
                name: "IX_userphoneconsents_titleversionid",
                schema: "whetstone",
                table: "userphoneconsents",
                column: "titleversionid");


            string functionText = MigrationFunctionExtensions.GetScriptContent("lambdausercreate");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("addintentaction_consolidate");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("addupdatephonenumber");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("addupdatesmsconsent");
            migrationBuilder.Sql(functionText);


            functionText = MigrationFunctionExtensions.GetFunctionContent("addupdatetitleuser");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("addoutboundmessagelog");
            migrationBuilder.Sql(functionText);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            string functionText = MigrationFunctionExtensions.GetFunctionContent("drop_getconsentreport");
            migrationBuilder.Sql(functionText);

            string scriptText = MigrationFunctionExtensions.GetScriptContent("drop_messageconsentview");
            migrationBuilder.Sql(scriptText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("drop_outboundmessagelog");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("drop_updatetitleuser");
            migrationBuilder.Sql(functionText);

            functionText = MigrationFunctionExtensions.GetFunctionContent("drop_updatesmsconsent");
            migrationBuilder.Sql(functionText);


            migrationBuilder.DropTable(
                name: "engine_requestaudit",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "outboundmessage_logs",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "titleversiondeployments",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "engine_session",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "outbound_messages",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "outbound_batches",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "userphoneconsents",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "phonenumbers",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "title_clientusers",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "titleversions",
                schema: "whetstone");

            migrationBuilder.DropTable(
                name: "titles",
                schema: "whetstone");
        }
    }
}

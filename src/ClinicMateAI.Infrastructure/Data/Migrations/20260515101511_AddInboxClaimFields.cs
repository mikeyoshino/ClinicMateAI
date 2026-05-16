using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMateAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxClaimFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalMessageId",
                table: "Messages",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Conversations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Open",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "AiStatus",
                table: "Conversations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "AssignedStaff",
                table: "Conversations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimedAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnreadCount",
                table: "Conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ClinicId_ExternalMessageId",
                table: "Messages",
                columns: new[] { "ClinicId", "ExternalMessageId" },
                unique: true,
                filter: "\"ExternalMessageId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ClinicId_ExternalMessageId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ExternalMessageId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AiStatus",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AssignedStaff",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ClaimedAt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UnreadCount",
                table: "Conversations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Conversations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Open");
        }
    }
}

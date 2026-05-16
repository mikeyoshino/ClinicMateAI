using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMateAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelConnectionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionStatus",
                table: "ClinicChannelConfigs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NotConnected");

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "ClinicChannelConfigs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedAtUtc",
                table: "ClinicChannelConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenOrLongLivedToken",
                table: "ClinicChannelConfigs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiresAtUtc",
                table: "ClinicChannelConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ClinicChannelConfigs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionStatus",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "LastVerifiedAtUtc",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "RefreshTokenOrLongLivedToken",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "TokenExpiresAtUtc",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ClinicChannelConfigs");
        }
    }
}

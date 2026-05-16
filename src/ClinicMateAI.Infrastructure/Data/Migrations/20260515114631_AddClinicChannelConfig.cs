using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMateAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicChannelConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicChannelConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalPageId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicChannelConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicChannelConfigs_ClinicId_Channel",
                table: "ClinicChannelConfigs",
                columns: new[] { "ClinicId", "Channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicChannelConfigs");
        }
    }
}

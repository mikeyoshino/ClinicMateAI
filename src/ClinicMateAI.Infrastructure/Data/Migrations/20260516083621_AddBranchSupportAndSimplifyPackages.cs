using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMateAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchSupportAndSimplifyPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_ClinicId_Name",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_ClinicId_Status",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClinicId_Channel_ExternalConversationId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClinicId_LastMessageAtUtc",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_ClinicChannelConfigs_ClinicId_Channel",
                table: "ClinicChannelConfigs");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Promotions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalBranchMonthlyPrice",
                table: "Clinics",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageTier",
                table: "Clinics",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Starter");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "ClinicChannelConfigs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MapUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BusinessHours = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Active"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicUserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DefaultBranchId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicUserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBranchAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBranchAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ClinicId_BranchId_Name",
                table: "Services",
                columns: new[] { "ClinicId", "BranchId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ClinicId_BranchId_Status",
                table: "Promotions",
                columns: new[] { "ClinicId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClinicId_BranchId_Channel_ExternalConversatio~",
                table: "Conversations",
                columns: new[] { "ClinicId", "BranchId", "Channel", "ExternalConversationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClinicId_BranchId_LastMessageAtUtc",
                table: "Conversations",
                columns: new[] { "ClinicId", "BranchId", "LastMessageAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicChannelConfigs_ClinicId_BranchId_Channel",
                table: "ClinicChannelConfigs",
                columns: new[] { "ClinicId", "BranchId", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ClinicId_Name",
                table: "Branches",
                columns: new[] { "ClinicId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUserProfiles_UserId_ClinicId",
                table: "ClinicUserProfiles",
                columns: new[] { "UserId", "ClinicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBranchAssignments_UserId_BranchId",
                table: "UserBranchAssignments",
                columns: new[] { "UserId", "BranchId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "ClinicUserProfiles");

            migrationBuilder.DropTable(
                name: "UserBranchAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Services_ClinicId_BranchId_Name",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_ClinicId_BranchId_Status",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClinicId_BranchId_Channel_ExternalConversatio~",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClinicId_BranchId_LastMessageAtUtc",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_ClinicChannelConfigs_ClinicId_BranchId_Channel",
                table: "ClinicChannelConfigs");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AdditionalBranchMonthlyPrice",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "PackageTier",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "ClinicChannelConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ClinicId_Name",
                table: "Services",
                columns: new[] { "ClinicId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ClinicId_Status",
                table: "Promotions",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClinicId_Channel_ExternalConversationId",
                table: "Conversations",
                columns: new[] { "ClinicId", "Channel", "ExternalConversationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClinicId_LastMessageAtUtc",
                table: "Conversations",
                columns: new[] { "ClinicId", "LastMessageAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicChannelConfigs_ClinicId_Channel",
                table: "ClinicChannelConfigs",
                columns: new[] { "ClinicId", "Channel" },
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NewsFlow.Infrastructure.Data;

#nullable disable

namespace NewsFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(NewsFlowDbContext))]
    [Migration("20260607230000_AddUserSettings")]
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AiModel = table.Column<string>(type: "text", nullable: false),
                    OutputLanguage = table.Column<string>(type: "text", nullable: false),
                    IngestFrequency = table.Column<string>(type: "text", nullable: false),
                    Voice = table.Column<string>(type: "text", nullable: false),
                    StockFootage = table.Column<string>(type: "text", nullable: false),
                    AspectRatio = table.Column<string>(type: "text", nullable: false),
                    EmailAlerts = table.Column<string>(type: "text", nullable: false),
                    ReviewAlerts = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}

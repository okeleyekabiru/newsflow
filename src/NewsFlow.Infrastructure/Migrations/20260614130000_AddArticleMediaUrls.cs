using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsFlow.Infrastructure.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(NewsFlow.Infrastructure.Data.NewsFlowDbContext))]
    [Migration("20260614130000_AddArticleMediaUrls")]
    public partial class AddArticleMediaUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Articles",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ThumbnailUrl", table: "Articles");
            migrationBuilder.DropColumn(name: "VideoUrl",     table: "Articles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_AspNetUsers_AuthorId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_AuthorId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Songs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ApplicationUserId",
                table: "Songs",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_AspNetUsers_ApplicationUserId",
                table: "Songs",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_AspNetUsers_ApplicationUserId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_ApplicationUserId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Songs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AuthorId",
                table: "Songs",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_AspNetUsers_AuthorId",
                table: "Songs",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

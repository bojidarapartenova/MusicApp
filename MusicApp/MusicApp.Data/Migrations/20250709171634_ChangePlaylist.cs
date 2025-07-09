using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_AspNetUsers_ApplicationUserId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_ApplicationUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "IsFavorites",
                table: "Playlists");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Playlists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_AspNetUsers_UserId",
                table: "Playlists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_AspNetUsers_UserId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Playlists");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Playlists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorites",
                table: "Playlists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ApplicationUserId",
                table: "Playlists",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_AspNetUsers_ApplicationUserId",
                table: "Playlists",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MusicApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSongPublisherGenreArtist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_AspNetUsers_ApplicationUserId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_ApplicationUserId",
                table: "Songs");

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: new Guid("a7b1e9c3-3fc2-4d6b-b14a-91d208bc72e1"));

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: new Guid("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"));

            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: new Guid("f4e2a8c9-7f38-4cbe-bad4-3c4e5f27893d"));

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "Songs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Songs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublisherId",
                table: "Songs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "1", 0, "3acd0da2-f7c4-4406-9a00-cc6f47bdfdb5", "admin@gmail.com", false, false, null, "Admin", "ADMIN@GMAIL.COM", null, null, null, false, "a366c121-b804-42ae-9ac7-7d3025ec1377", false, "admin@gmail.com" });

            migrationBuilder.CreateIndex(
                name: "IX_Songs_PublisherId",
                table: "Songs",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_AspNetUsers_PublisherId",
                table: "Songs",
                column: "PublisherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_AspNetUsers_PublisherId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_PublisherId",
                table: "Songs");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DropColumn(
                name: "Artist",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Songs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "ApplicationUserId", "AudioUrl", "Duration", "ImageUrl", "IsDeleted", "Likes", "ReleaseDate", "Title" },
                values: new object[,]
                {
                    { new Guid("a7b1e9c3-3fc2-4d6b-b14a-91d208bc72e1"), null, "/audio/espresso.mp3", 162, "https://upload.wikimedia.org/wikipedia/en/7/71/Espresso_-_Sabrina_Carpenter.png", false, 0, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Espresso - Sabrina Carpenter (Sped up)" },
                    { new Guid("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"), null, "/audio/beautiful_things.mp3", 154, "https://upload.wikimedia.org/wikipedia/en/4/4b/Benson_Boone_-_Beautiful_Things.png", false, 0, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Benson Boone - Beautiful Things (Sped Up)" },
                    { new Guid("f4e2a8c9-7f38-4cbe-bad4-3c4e5f27893d"), null, "/audio/popular.mp3", 187, "https://upload.wikimedia.org/wikipedia/en/2/24/The_Weeknd_-_Popular.png", false, 0, new DateTime(2023, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Weeknd - Popular (Sped up)" }
                });

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
    }
}

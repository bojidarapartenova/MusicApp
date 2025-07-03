using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MusicApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: new Guid("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d8c9f6a0-6c0a-4f5b-bca9-11e3b47f6c42");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Songs");

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pop" },
                    { 2, "R&B" },
                    { 3, "Hip Hop" },
                    { 4, "Rock" },
                    { 5, "Electronic" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Songs_GenreId",
                table: "Songs",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Genres_GenreId",
                table: "Songs",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Genres_GenreId",
                table: "Songs");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Songs_GenreId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Songs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d8c9f6a0-6c0a-4f5b-bca9-11e3b47f6c42", 0, "247482b0-0f37-4108-9f8b-ceedb362c4ae", "admin@gmail.com", true, false, null, "Admin", "ADMIN@GMAIL.COM", "ADMIN@GMAIL.COM", "AQAAAAEAACcQAAAAEI8zx8QfPVhWt05TqYOTWIl8YYpH83SKWurBnm/OoNYVfGcZ8yR6oV47YOcPMTWTkA==", null, false, "04cd4957-72b9-438b-8721-4c688190a653", false, "admin@gmail.com" });

            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Artist", "AudioUrl", "Duration", "Genre", "ImageUrl", "IsDeleted", "Likes", "PublisherId", "ReleaseDate", "Title" },
                values: new object[] { new Guid("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"), "Benson Boone", "/audio/beautiful_things.mp3", 154, "Pop", "https://upload.wikimedia.org/wikipedia/en/4/4b/Benson_Boone_-_Beautiful_Things.png", false, 0, "d8c9f6a0-6c0a-4f5b-bca9-11e3b47f6c42", new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Benson Boone - Beautiful Things (Sped Up)" });
        }
    }
}

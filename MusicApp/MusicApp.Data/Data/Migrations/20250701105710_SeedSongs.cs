using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MusicApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSongs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "ApplicationUserId", "AudioUrl", "Duration", "ImageUrl", "IsDeleted", "Likes", "ReleaseDate", "Title" },
                values: new object[,]
                {
                    { new Guid("a7b1e9c3-3fc2-4d6b-b14a-91d208bc72e1"), null, "/audio/espresso.mp3", 162, "https://upload.wikimedia.org/wikipedia/en/7/71/Espresso_-_Sabrina_Carpenter.png", false, 0, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Espresso - Sabrina Carpenter (Sped up)" },
                    { new Guid("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"), null, "/audio/beautiful_things.mp3", 154, "https://upload.wikimedia.org/wikipedia/en/4/4b/Benson_Boone_-_Beautiful_Things.png", false, 0, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Benson Boone - Beautiful Things (Sped Up)" },
                    { new Guid("f4e2a8c9-7f38-4cbe-bad4-3c4e5f27893d"), null, "/audio/popular.mp3", 187, "https://upload.wikimedia.org/wikipedia/en/2/24/The_Weeknd_-_Popular.png", false, 0, new DateTime(2023, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Weeknd - Popular (Sped up)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}

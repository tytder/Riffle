using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Riffle.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_PlaylistSongs_SongId",
                table: "SongPlayed");

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed");

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_PlaylistSongs_SongId",
                table: "SongPlayed",
                column: "SongId",
                principalTable: "PlaylistSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

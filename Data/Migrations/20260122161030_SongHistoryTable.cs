using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Riffle.Data.Migrations
{
    /// <inheritdoc />
    public partial class SongHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PlaylistSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SongPlayed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SongId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlayedFromId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlayedFromName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayed_PlaylistSongs_SongId",
                        column: x => x.SongId,
                        principalTable: "PlaylistSongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SongPlayed_Playlists_PlayedFromId",
                        column: x => x.PlayedFromId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSongs_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayed_PlayedFromId",
                table: "SongPlayed",
                column: "PlayedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayed_SongId",
                table: "SongPlayed",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongPlayed");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistSongs_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlaylistSongs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs",
                columns: new[] { "PlaylistId", "SongId" });
        }
    }
}

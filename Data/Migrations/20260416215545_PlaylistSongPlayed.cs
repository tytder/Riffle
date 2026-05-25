using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Riffle.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistSongPlayed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_Playlists_PlayedFromId",
                table: "SongPlayed");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed");

            migrationBuilder.RenameColumn(
                name: "PlayedFromId",
                table: "SongPlayed",
                newName: "PlaylistSongId");

            migrationBuilder.RenameIndex(
                name: "IX_SongPlayed_PlayedFromId",
                table: "SongPlayed",
                newName: "IX_SongPlayed_PlaylistSongId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "SongPlayed",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "ArtistName",
                table: "SongPlayed",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlaylistId",
                table: "SongPlayed",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SongName",
                table: "SongPlayed",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayed_PlaylistId",
                table: "SongPlayed",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_PlaylistSongs_PlaylistSongId",
                table: "SongPlayed",
                column: "PlaylistSongId",
                principalTable: "PlaylistSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_Playlists_PlaylistId",
                table: "SongPlayed",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_PlaylistSongs_PlaylistSongId",
                table: "SongPlayed");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_Playlists_PlaylistId",
                table: "SongPlayed");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed");

            migrationBuilder.DropIndex(
                name: "IX_SongPlayed_PlaylistId",
                table: "SongPlayed");

            migrationBuilder.DropColumn(
                name: "ArtistName",
                table: "SongPlayed");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "SongPlayed");

            migrationBuilder.DropColumn(
                name: "SongName",
                table: "SongPlayed");

            migrationBuilder.RenameColumn(
                name: "PlaylistSongId",
                table: "SongPlayed",
                newName: "PlayedFromId");

            migrationBuilder.RenameIndex(
                name: "IX_SongPlayed_PlaylistSongId",
                table: "SongPlayed",
                newName: "IX_SongPlayed_PlayedFromId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "SongPlayed",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_Playlists_PlayedFromId",
                table: "SongPlayed",
                column: "PlayedFromId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayed_Songs_SongId",
                table: "SongPlayed",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

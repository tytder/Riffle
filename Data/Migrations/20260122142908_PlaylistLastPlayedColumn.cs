using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Riffle.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistLastPlayedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayed",
                table: "Playlists",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "Playlists");
        }
    }
}

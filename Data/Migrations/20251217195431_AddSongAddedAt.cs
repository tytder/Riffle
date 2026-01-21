using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Riffle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSongAddedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "Songs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "Songs");
        }
    }
}

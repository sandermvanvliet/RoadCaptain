// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadCaptain.App.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHashToRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Routes",
                type: "TEXT",
                nullable: false,
                defaultValue: "(not yet calculated)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Routes");
        }
    }
}


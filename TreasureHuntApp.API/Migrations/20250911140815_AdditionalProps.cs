using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreasureHuntApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationEntityId",
                table: "Photos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_LocationEntityId",
                table: "Photos",
                column: "LocationEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Locations_LocationEntityId",
                table: "Photos",
                column: "LocationEntityId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Locations_LocationEntityId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_LocationEntityId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "LocationEntityId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Events");
        }
    }
}

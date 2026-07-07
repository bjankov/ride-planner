using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RydePlannr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRideParticipantsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RideParticipants_RideEventId",
                table: "RideParticipants");

            migrationBuilder.DropColumn(
                name: "CutoffTime",
                table: "RideEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "CutoffMinutes",
                table: "RideEvents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RideParticipants_RideEventId_UserId",
                table: "RideParticipants",
                columns: new[] { "RideEventId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_RideParticipants_RideEventId_UserId",
                table: "RideParticipants");

            migrationBuilder.DropColumn(
                name: "CutoffMinutes",
                table: "RideEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "CutoffTime",
                table: "RideEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RideParticipants_RideEventId",
                table: "RideParticipants",
                column: "RideEventId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RydePlannr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_Users_OwnerId",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clubs_ClubId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClubId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_OwnerId",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Clubs");

            migrationBuilder.RenameColumn(
                name: "ElevationGain",
                table: "Routes",
                newName: "ElevationGainMeters");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "RideEvents",
                newName: "CutoffTime");

            migrationBuilder.AddColumn<double>(
                name: "AverageSpeed",
                table: "RideParticipants",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FinishTime",
                table: "RideParticipants",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "RideParticipants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "EntryFee",
                table: "RideEvents",
                type: "numeric",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clubs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Clubs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Clubs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "ClubMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClubId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMembers_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembers_ClubId",
                table: "ClubMembers",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembers_UserId_ClubId",
                table: "ClubMembers",
                columns: new[] { "UserId", "ClubId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubMembers");

            migrationBuilder.DropColumn(
                name: "AverageSpeed",
                table: "RideParticipants");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "RideParticipants");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "RideParticipants");

            migrationBuilder.DropColumn(
                name: "EntryFee",
                table: "RideEvents");

            migrationBuilder.RenameColumn(
                name: "ElevationGainMeters",
                table: "Routes",
                newName: "ElevationGain");

            migrationBuilder.RenameColumn(
                name: "CutoffTime",
                table: "RideEvents",
                newName: "EndTime");

            migrationBuilder.AddColumn<int>(
                name: "ClubId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clubs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Clubs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Clubs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Clubs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClubId",
                table: "Users",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_OwnerId",
                table: "Clubs",
                column: "OwnerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_Users_OwnerId",
                table: "Clubs",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clubs_ClubId",
                table: "Users",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

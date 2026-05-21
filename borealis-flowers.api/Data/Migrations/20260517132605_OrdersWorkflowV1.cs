using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrdersWorkflowV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminComment",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Budget",
                table: "Requests",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientConfirmedAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventStartsAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "Requests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloristComment",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloristInventory",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloristMaterials",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderKind",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "QuoteTotal",
                table: "Requests",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceTitleSnapshot",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WishNotes",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1219e0a7-4cbb-4395-a7b8-d4ce3979a9ac"),
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2944556b-ca71-449c-8b2a-0b493b2d3a78"),
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2dfda2c0-3dc6-4937-b52a-fca938ac6a63"),
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("5b385e48-5a16-4575-8290-ee173711840c"),
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("73e26494-9a6c-43cd-a5f0-486afced5d61"),
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("ecf67bc9-70f3-4d38-abd6-2e1bbc457756"),
                column: "Description",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ServiceId",
                table: "Requests",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Services_ServiceId",
                table: "Requests",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                UPDATE "Requests" SET "State" = CASE COALESCE("State", 0)
                  WHEN 0 THEN 0
                  WHEN 1 THEN 4
                  WHEN 2 THEN 7
                  WHEN 3 THEN 1
                  ELSE 0 END
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Services" SET "Description" = 'Нежная композиция: ' || "Name" || '. Состав и оттенки согласуются с вами при заказе.'
                WHERE "Description" IS NULL OR "Description" = ''
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Services_ServiceId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ServiceId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AdminComment",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ClientConfirmedAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DepartureAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "EventStartsAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FloristComment",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FloristInventory",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FloristMaterials",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "OrderKind",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "QuoteTotal",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ServiceTitleSnapshot",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WishNotes",
                table: "Requests");
        }
    }
}

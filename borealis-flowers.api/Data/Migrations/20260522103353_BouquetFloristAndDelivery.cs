using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class BouquetFloristAndDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpecialistId",
                table: "Services",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardMessage",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhoneSnapshot",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLatitude",
                table: "Requests",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLongitude",
                table: "Requests",
                type: "REAL",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1219e0a7-4cbb-4395-a7b8-d4ce3979a9ac"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2944556b-ca71-449c-8b2a-0b493b2d3a78"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2dfda2c0-3dc6-4937-b52a-fca938ac6a63"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("5b385e48-5a16-4575-8290-ee173711840c"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("73e26494-9a6c-43cd-a5f0-486afced5d61"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("ecf67bc9-70f3-4d38-abd6-2e1bbc457756"),
                column: "SpecialistId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Services_SpecialistId",
                table: "Services",
                column: "SpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specialists_SpecialistId",
                table: "Services",
                column: "SpecialistId",
                principalTable: "Specialists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Specialists_SpecialistId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_SpecialistId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SpecialistId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CardMessage",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CustomerPhoneSnapshot",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DeliveryLatitude",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DeliveryLongitude",
                table: "Requests");
        }
    }
}

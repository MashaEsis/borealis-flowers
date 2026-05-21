using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAuthAndFloristApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialist",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialistId",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FloristApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Experience = table.Column<string>(type: "TEXT", nullable: false),
                    PortfolioNotes = table.Column<string>(type: "TEXT", nullable: false),
                    Motivation = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AdminComment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloristApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloristApplications_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloristApplications_CustomerId",
                table: "FloristApplications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SpecialistId",
                table: "Customers",
                column: "SpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Specialists_SpecialistId",
                table: "Customers",
                column: "SpecialistId",
                principalTable: "Specialists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Specialists_SpecialistId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "FloristApplications");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SpecialistId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsSpecialist",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SpecialistId",
                table: "Customers");
        }
    }
}

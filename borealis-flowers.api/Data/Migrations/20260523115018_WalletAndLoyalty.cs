using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class WalletAndLoyalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ChargedAmount",
                table: "Requests",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WalletBalance",
                table: "Customers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "PaymentCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    LastFour = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCards_CustomerId",
                table: "PaymentCards",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentCards");

            migrationBuilder.DropColumn(
                name: "ChargedAmount",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PaidAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WalletBalance",
                table: "Customers");
        }
    }
}

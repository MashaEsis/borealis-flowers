using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialistPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StyleDescription",
                table: "Specialists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SpecialistPortfolioWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialistPortfolioWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialistPortfolioWorks_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Specialists",
                keyColumn: "Id",
                keyValue: new Guid("278666b8-3503-47b0-b5f6-7139563dace6"),
                column: "StyleDescription",
                value: null);

            migrationBuilder.UpdateData(
                table: "Specialists",
                keyColumn: "Id",
                keyValue: new Guid("5bd8fd04-9613-4c69-85c9-7347572f6289"),
                column: "StyleDescription",
                value: null);

            migrationBuilder.UpdateData(
                table: "Specialists",
                keyColumn: "Id",
                keyValue: new Guid("88639ec4-d834-4788-bce4-05cfce258cce"),
                column: "StyleDescription",
                value: null);

            migrationBuilder.UpdateData(
                table: "Specialists",
                keyColumn: "Id",
                keyValue: new Guid("b23a6e06-ce61-4445-be74-0cfc5f0a0729"),
                column: "StyleDescription",
                value: null);

            migrationBuilder.UpdateData(
                table: "Specialists",
                keyColumn: "Id",
                keyValue: new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"),
                column: "StyleDescription",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistPortfolioWorks_SpecialistId",
                table: "SpecialistPortfolioWorks",
                column: "SpecialistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialistPortfolioWorks");

            migrationBuilder.DropColumn(
                name: "StyleDescription",
                table: "Specialists");
        }
    }
}

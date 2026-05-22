using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlowerComposition",
                table: "Services",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Services",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowerComposition",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Services");
        }
    }
}

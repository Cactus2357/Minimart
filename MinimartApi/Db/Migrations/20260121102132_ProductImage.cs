using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimartApi.Migrations
{
    /// <inheritdoc />
    public partial class ProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_VariantId",
                table: "PriceHistories");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_VariantId_EffectiveFrom_EffectiveTo",
                table: "PriceHistories",
                columns: new[] { "VariantId", "EffectiveFrom", "EffectiveTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_VariantId_EffectiveFrom_EffectiveTo",
                table: "PriceHistories");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_VariantId",
                table: "PriceHistories",
                column: "VariantId");
        }
    }
}

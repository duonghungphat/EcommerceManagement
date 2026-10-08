using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ProductVariants",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ProductVariants");
        }
    }
}

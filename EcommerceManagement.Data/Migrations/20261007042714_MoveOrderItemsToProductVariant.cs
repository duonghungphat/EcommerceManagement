using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceManagement.Data.Migrations
{
    public partial class MoveOrderItemsToProductVariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO ProductVariants (Name, SKU, Price, StockQuantity, ProductId)
                SELECT 'Mặc định', p.SKU, p.Price, p.StockQuantity, p.Id
                FROM Products p
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ProductVariants pv
                    WHERE pv.ProductId = p.Id
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE OrderItems oi
                INNER JOIN Products p ON p.Id = oi.ProductId
                INNER JOIN ProductVariants pv ON pv.ProductId = p.Id AND pv.SKU = p.SKU
                SET oi.ProductVariantId = pv.Id
                WHERE oi.ProductVariantId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE OrderItems oi
                INNER JOIN (
                    SELECT ProductId, MIN(Id) AS VariantId
                    FROM ProductVariants
                    GROUP BY ProductId
                ) pv ON pv.ProductId = oi.ProductId
                SET oi.ProductVariantId = pv.VariantId
                WHERE oi.ProductVariantId IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE OrderItems oi
                INNER JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId
                SET oi.ProductId = pv.ProductId;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "OrderItems");
        }
    }
}

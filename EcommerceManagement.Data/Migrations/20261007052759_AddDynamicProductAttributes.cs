using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace EcommerceManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicProductAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductVariants",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "ProductAttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CanUseForVariant = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanUseForProduct = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsFilterable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowMultipleProductValues = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeDefinitions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductAttributeValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValues", x => x.Id);
                    table.UniqueConstraint("AK_ProductAttributeValues_Id_AttributeDefinitionId", x => new { x.Id, x.AttributeDefinitionId });
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_ProductAttributeDefinitions_Attribute~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "ProductAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductAttributeSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    AttributeValueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeSelections_ProductAttributeDefinitions_Attri~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "ProductAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeSelections_ProductAttributeValues_AttributeV~",
                        columns: x => new { x.AttributeValueId, x.AttributeDefinitionId },
                        principalTable: "ProductAttributeValues",
                        principalColumns: new[] { "Id", "AttributeDefinitionId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeSelections_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductVariantAttributeSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    AttributeValueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantAttributeSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantAttributeSelections_ProductAttributeDefinition~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "ProductAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVariantAttributeSelections_ProductAttributeValues_Att~",
                        columns: x => new { x.AttributeValueId, x.AttributeDefinitionId },
                        principalTable: "ProductAttributeValues",
                        principalColumns: new[] { "Id", "AttributeDefinitionId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVariantAttributeSelections_ProductVariants_ProductVar~",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeDefinitions_Code",
                table: "ProductAttributeDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSelections_AttributeDefinitionId",
                table: "ProductAttributeSelections",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSelections_AttributeValueId_AttributeDefinit~",
                table: "ProductAttributeSelections",
                columns: new[] { "AttributeValueId", "AttributeDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSelections_ProductId_AttributeValueId",
                table: "ProductAttributeSelections",
                columns: new[] { "ProductId", "AttributeValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_AttributeDefinitionId_Value",
                table: "ProductAttributeValues",
                columns: new[] { "AttributeDefinitionId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantAttributeSelections_AttributeDefinitionId",
                table: "ProductVariantAttributeSelections",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantAttributeSelections_AttributeValueId_Attribute~",
                table: "ProductVariantAttributeSelections",
                columns: new[] { "AttributeValueId", "AttributeDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantAttributeSelections_ProductVariantId_Attribute~",
                table: "ProductVariantAttributeSelections",
                columns: new[] { "ProductVariantId", "AttributeDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributeSelections");

            migrationBuilder.DropTable(
                name: "ProductVariantAttributeSelections");

            migrationBuilder.DropTable(
                name: "ProductAttributeValues");

            migrationBuilder.DropTable(
                name: "ProductAttributeDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductVariants",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);
        }
    }
}

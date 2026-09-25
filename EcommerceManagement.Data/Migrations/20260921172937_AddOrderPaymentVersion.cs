using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentVersion",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentVersion",
                table: "Orders");
        }
    }
}

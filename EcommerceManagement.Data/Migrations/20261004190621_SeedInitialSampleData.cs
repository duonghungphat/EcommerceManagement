using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ seed dữ liệu mẫu khi đây là database mới,
            // tức chưa có bất kỳ tài khoản nào.
            migrationBuilder.Sql("""
        INSERT INTO Categories (Name, Description)
        SELECT seed.Name, seed.Description
        FROM (
            SELECT 'Điện thoại' AS Name, 'Danh mục Điện thoại' AS Description
            UNION ALL
            SELECT 'Laptop', 'Danh mục Laptop'
            UNION ALL
            SELECT 'Phụ kiện', 'Danh mục Phụ kiện'
        ) AS seed
        WHERE NOT EXISTS (
            SELECT 1 FROM ApplicationUsers
        );
    """);

            migrationBuilder.Sql("""
        INSERT INTO Products
            (Name, SKU, Price, StockQuantity, ImagePath, Status, CategoryId)
        SELECT
            seed.Name,
            seed.SKU,
            seed.Price,
            seed.StockQuantity,
            NULL,
            1,
            c.Id
        FROM (
            SELECT
                'Điện thoại mẫu' AS Name,
                'PHONE001' AS SKU,
                15000000 AS Price,
                20 AS StockQuantity,
                'Điện thoại' AS CategoryName

            UNION ALL

            SELECT
                'Laptop mẫu',
                'LAPTOP001',
                25000000,
                8,
                'Laptop'

            UNION ALL

            SELECT
                'Chuột không dây',
                'ACCESSORY001',
                500000,
                30,
                'Phụ kiện'
        ) AS seed
        INNER JOIN Categories c
            ON c.Name = seed.CategoryName
        WHERE NOT EXISTS (
            SELECT 1 FROM ApplicationUsers
        );
    """);

            migrationBuilder.Sql("""
        INSERT INTO Customers
            (FullName, Email, PhoneNumber, Address, CreatedAt)
        SELECT
            seed.FullName,
            seed.Email,
            seed.PhoneNumber,
            seed.Address,
            NOW()
        FROM (
            SELECT
                'Nguyễn Văn A' AS FullName,
                'khachhang1@example.com' AS Email,
                '0901234567' AS PhoneNumber,
                'Thành phố Hồ Chí Minh' AS Address

            UNION ALL

            SELECT
                'Trần Thị B',
                'khachhang2@example.com',
                '0912345678',
                'Hà Nội'
        ) AS seed
        WHERE NOT EXISTS (
            SELECT 1 FROM ApplicationUsers
        );
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

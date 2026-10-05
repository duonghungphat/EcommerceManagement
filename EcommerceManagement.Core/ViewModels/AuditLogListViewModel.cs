namespace EcommerceManagement.Core.ViewModels
{
    public class AuditLogListViewModel
    {
        public int? UserId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<AuditLogItemViewModel> Logs { get; set; } = new();

        public List<AuditLogUserViewModel> Users { get; set; } = new();
    }

    public class AuditLogUserViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
    }

    public class AuditLogItemViewModel
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string? Description { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string ActionText => Action switch
        {
            "LoginSuccess" => "Đăng nhập thành công",
            "LoginFailed" => "Đăng nhập thất bại",

            "CreateUser" => "Tạo tài khoản",
            "RoleChanged" => "Thay đổi vai trò",
            "LockUser" => "Khóa tài khoản",
            "UnlockUser" => "Mở khóa tài khoản",
            "ResetPassword" => "Đặt lại mật khẩu",

            "CreateCategory" => "Thêm danh mục",
            "UpdateCategory" => "Cập nhật danh mục",
            "DeleteCategory" => "Xóa danh mục",

            "CreateProduct" => "Thêm sản phẩm",
            "UpdateProduct" => "Cập nhật sản phẩm",
            "DeleteProduct" => "Xóa sản phẩm",

            "CreateCustomer" => "Thêm khách hàng",
            "UpdateCustomer" => "Cập nhật khách hàng",
            "DeleteCustomer" => "Xóa khách hàng",

            "CreateOrder" => "Tạo đơn hàng",
            "UpdateOrderStatus" => "Cập nhật trạng thái đơn hàng",
            "CancelOrder" => "Hủy đơn hàng",

            "CreatePayment" => "Ghi nhận thanh toán",
            "RefundPayment" => "Hoàn tiền",

            _ => Action
        };

        public string EntityNameText => EntityName switch
        {
            "ApplicationUser" => "Tài khoản",
            "Category" => "Danh mục",
            "Product" => "Sản phẩm",
            "Customer" => "Khách hàng",
            "Order" => "Đơn hàng",
            "Payment" => "Thanh toán",

            _ => EntityName
        };
    }
}
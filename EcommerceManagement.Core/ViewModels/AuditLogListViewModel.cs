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
            "RoleChanged" => "Thay đổi vai trò",
            "CreateOrder" => "Tạo đơn hàng",
            "CancelOrder" => "Hủy đơn hàng",
            "CreatePayment" => "Ghi nhận thanh toán",
            "RefundPayment" => "Hoàn tiền",
            _ => Action
        };

        public string EntityNameText => EntityName switch
        {
            "ApplicationUser" => "Tài khoản",
            "Order" => "Đơn hàng",
            "Payment" => "Thanh toán",
            _ => EntityName
        };
    }
}
namespace EcommerceManagement.Core.ViewModels
{
    public class UserListItemViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public string RoleDisplayName => RoleName switch
        {
            "Admin" => "Quản trị viên",
            "Manager" => "Quản lý",
            "Staff" => "Nhân viên",
            _ => RoleName
        };
    }
}
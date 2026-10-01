namespace EcommerceManagement.Core.ViewModels
{
    public class RoleOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DisplayName => Name switch
        {
            "Admin" => "Quản trị viên",
            "Manager" => "Quản lý",
            "Staff" => "Nhân viên",
            _ => Name
        };
    }
}
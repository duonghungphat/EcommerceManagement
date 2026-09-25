using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [StringLength(10)]
        [RegularExpression(@"^[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải có từ 9 đến 10 chữ số.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
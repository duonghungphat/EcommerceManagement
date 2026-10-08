using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class ProductAttributeDefinitionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thuộc tính không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên thuộc tính tối đa 100 ký tự.")]
        [Display(Name = "Tên thuộc tính")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã thuộc tính không được để trống.")]
        [StringLength(100, ErrorMessage = "Mã thuộc tính tối đa 100 ký tự.")]
        [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Mã thuộc tính chỉ gồm chữ không dấu, số, dấu gạch ngang hoặc gạch dưới.")]
        [Display(Name = "Mã thuộc tính")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Dùng cho biến thể")]
        public bool CanUseForVariant { get; set; }

        [Display(Name = "Dùng cho sản phẩm")]
        public bool CanUseForProduct { get; set; }

        [Display(Name = "Cho phép lọc")]
        public bool IsFilterable { get; set; }

        [Display(Name = "Cho phép chọn nhiều giá trị")]
        public bool AllowMultipleProductValues { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm.")]
        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        public int ValueCount { get; set; }
    }
}

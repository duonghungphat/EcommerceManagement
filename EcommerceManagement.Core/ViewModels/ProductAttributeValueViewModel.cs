using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class ProductAttributeValueViewModel
    {
        public int Id { get; set; }

        public int AttributeDefinitionId { get; set; }

        public string AttributeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá trị không được để trống.")]
        [StringLength(100, ErrorMessage = "Giá trị tối đa 100 ký tự.")]
        [Display(Name = "Giá trị")]
        public string Value { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm.")]
        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}

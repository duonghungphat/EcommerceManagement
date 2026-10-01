using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.Enums
{
    public enum ProductStatus
    {
        [Display(Name = "Đang bán")]
        Selling = 1,

        [Display(Name = "Ngừng bán")]
        Stopped = 2
    }
}
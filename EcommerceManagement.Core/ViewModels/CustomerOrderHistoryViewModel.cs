using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class CustomerOrderHistoryViewModel
    {
        public CustomerViewModel Customer { get; set; } = new();

        public List<CustomerOrderItemViewModel> Orders { get; set; } = new();
    }
}
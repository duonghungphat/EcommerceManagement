using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class CustomerOrderHistoryViewModel
    {
        public CustomerViewModel Customer { get; set; } = new();

        public List<CustomerOrderItemViewModel> Orders { get; set; } = new();
    }

    public class CustomerOrderItemViewModel
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
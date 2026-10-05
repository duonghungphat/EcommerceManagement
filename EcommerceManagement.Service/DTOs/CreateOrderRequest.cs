namespace EcommerceManagement.Service.DTOs
{
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }

        public int CreatedByUserId { get; set; }

        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
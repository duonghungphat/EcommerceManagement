namespace EcommerceManagement.Service.DTOs
{
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
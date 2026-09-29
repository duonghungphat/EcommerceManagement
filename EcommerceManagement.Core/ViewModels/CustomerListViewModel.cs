namespace EcommerceManagement.Core.ViewModels
{
    public class CustomerListViewModel
    {
        public string? SearchTerm { get; set; }

        public List<CustomerViewModel> Customers { get; set; } = new();
    }
}
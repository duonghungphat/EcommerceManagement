namespace EcommerceManagement.Core.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<OrderListItemViewModel> LatestOrders { get; set; } = new();

        public List<LowStockProductViewModel> LowStockProducts { get; set; } = new();

        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();

        public List<TopSellingProductViewModel> TopProducts { get; set; } = new();

        public List<TopCustomerViewModel> TopCustomers { get; set; } = new();
    }

    public class LowStockProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
    }

    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public decimal Revenue { get; set; }

        public string Label => $"{Month:D2}/{Year}";
    }

    public class TopSellingProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public int QuantitySold { get; set; }

        public decimal Revenue { get; set; }
    }

    public class TopCustomerViewModel
    {
        public string CustomerName { get; set; } = string.Empty;

        public int OrderCount { get; set; }

        public decimal TotalSales { get; set; }
    }
}
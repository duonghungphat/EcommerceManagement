using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardViewModel> GetAsync()
        {
            int totalProducts = await _unitOfWork.Products
                    .BuildQuery(p => true)
                    .CountAsync();

            int totalCustomers = await _unitOfWork.Customers
                    .BuildQuery(c => true)
                    .CountAsync();

            int totalOrders = await _unitOfWork.Orders
                    .BuildQuery(o => true)
                    .CountAsync();

            int pendingOrders = await _unitOfWork.Orders
                    .BuildQuery( o => o.Status == OrderStatus.Pending)
                    .CountAsync();

            decimal totalRevenue = await _unitOfWork.Payments
                    .BuildQuery(p => p.Status == PaymentStatus.Successful)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;


            var latestOrders = await _unitOfWork.Orders
                    .BuildQuery(o => true)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new OrderListItemViewModel
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        CustomerName = o.Customer.FullName,
                        CreatedByName = o.CreatedByUser.FullName,
                        CreatedAt = o.CreatedAt,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status
                    })
                    .ToListAsync();


            var lowStockProducts = await _unitOfWork.Products
                    .BuildQuery(p => p.StockQuantity <= 10)
                    .OrderBy(p => p.StockQuantity)
                    .Take(5)
                    .Select(p =>
                        new LowStockProductViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            SKU = p.SKU,
                            StockQuantity = p.StockQuantity
                        })
                    .ToListAsync();


            var monthlyRaw = await _unitOfWork.Payments
                    .BuildQuery(p => p.Status == PaymentStatus.Successful)
                    .GroupBy(p => new
                    {
                        p.TransactionAt.Year,
                        p.TransactionAt.Month
                    })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

            var monthlyRevenue = monthlyRaw.Select(x =>
                    new MonthlyRevenueViewModel
                    {
                        Year = x.Year,
                        Month = x.Month,
                        Revenue = x.Revenue
                    })
                    .ToList();


            var topProductsRaw = await _unitOfWork.OrderItems
                    .BuildQuery(oi => oi.Order.Status != OrderStatus.Cancelled)
                    .GroupBy(oi => new
                    {
                        oi.ProductId,
                        oi.Product.Name
                    })
                    .Select(g => new
                    {
                        ProductName = g.Key.Name,
                        Quantity = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .ToListAsync();

            var topProducts = topProductsRaw.Select(x =>
                    new TopSellingProductViewModel
                    {
                        ProductName = x.ProductName,
                        QuantitySold = x.Quantity,
                        Revenue = x.Revenue
                    })
                    .ToList();


            var topCustomersRaw = await _unitOfWork.Orders
                    .BuildQuery(o => o.Status != OrderStatus.Cancelled)
                    .GroupBy(o => new
                    {
                        o.CustomerId,
                        o.Customer.FullName
                    })
                    .Select(g => new
                    {
                        CustomerName = g.Key.FullName,
                        OrderCount = g.Count(),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(5)
                    .ToListAsync();

            var topCustomers =
                topCustomersRaw.Select(x =>
                    new TopCustomerViewModel
                    {
                        CustomerName = x.CustomerName,
                        OrderCount = x.OrderCount,
                        TotalSales = x.TotalSales
                    })
                    .ToList();


            return new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalRevenue = totalRevenue,
                LatestOrders = latestOrders,
                LowStockProducts = lowStockProducts,
                MonthlyRevenue = monthlyRevenue,
                TopProducts = topProducts,
                TopCustomers = topCustomers
            };
        }
    }
}
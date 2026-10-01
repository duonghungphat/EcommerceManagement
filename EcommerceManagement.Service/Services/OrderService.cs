using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public OrderService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }
        public async Task<OrderListViewModel> SearchAsync(string? searchTerm, OrderStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _unitOfWork.Orders
                .BuildQuery(o => true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(o => o.OrderCode.Contains(searchTerm) || o.Customer.FullName.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);

                query = query.Where(o => o.CreatedAt < end);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
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

            return new OrderListViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Orders = orders
            };
        }

        public async Task<OrderDetailsViewModel?> GetDetailsAsync(int id)
        {
            var order = await _unitOfWork.Orders
                .BuildQuery(o => o.Id == id)
                .Select(o => new OrderDetailsViewModel
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerName = o.Customer.FullName,
                    CustomerEmail = o.Customer.Email,
                    CustomerPhone = o.Customer.PhoneNumber,
                    CreatedByName = o.CreatedByUser.FullName,
                    CreatedAt = o.CreatedAt,
                    Note = o.Note,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,

                    Items = o.OrderItems
                        .Select(i => new OrderItemViewModel
                        {
                            ProductId = i.ProductId,
                            ProductName = i.Product.Name,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        })
                        .ToList(),

                    Payments = o.Payments
                        .OrderByDescending(p => p.TransactionAt)
                        .Select(p => new PaymentViewModel
                        {
                            Id = p.Id,
                            OrderId = p.OrderId,
                            OrderCode = o.OrderCode,
                            Amount = p.Amount,
                            PaymentMethod = p.PaymentMethod,
                            Status = p.Status,
                            TransactionCode = p.TransactionCode,
                            TransactionAt = p.TransactionAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return null;

            order.OriginalPaidAmount = order.Payments
                .Where(p => p.Status == PaymentStatus.Successful || p.Status == PaymentStatus.Refunded)
                .Sum(p => p.Amount);

            order.RefundedAmount = order.Payments
                .Where(p => p.Status == PaymentStatus.Refunded)
                .Sum(p => p.Amount);

            order.PaidAmount = order.Payments
                .Where(p => p.Status == PaymentStatus.Successful)
                .Sum(p => p.Amount);

            return order;
        }

        public async Task<int> CreateAsync(OrderCreateViewModel model, int createdByUserId, string ipAddress)
        {
            if (model.Items == null || model.Items.Count == 0)
            {
                throw new InvalidOperationException("Đơn hàng phải có ít nhất một sản phẩm.");
            }

            if (model.Items.Any(i => i.Quantity < 1))
            {
                throw new InvalidOperationException("Số lượng mỗi sản phẩm phải từ 1 trở lên.");
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(model.CustomerId);

            if (customer == null)
                throw new InvalidOperationException("Khách hàng không tồn tại.");

            var creator = await _unitOfWork.Users.GetByIdAsync(createdByUserId);

            if (creator == null || !creator.IsActive)
                throw new InvalidOperationException("Người tạo đơn không hợp lệ.");

            var order = new Order
            {
                OrderCode = $"ORD-{Guid.NewGuid():N}",
                CustomerId = model.CustomerId,
                CreatedByUserId = createdByUserId,
                Note = model.Note?.Trim(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now
            };

            var groupedItems = model.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                });

            foreach (var item in groupedItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new InvalidOperationException("Sản phẩm không tồn tại.");

                if (product.Status != ProductStatus.Selling)
                {
                    throw new InvalidOperationException($"Sản phẩm {product.Name} đã ngừng bán.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Sản phẩm {product.Name} không đủ hàng. " + $"Tồn kho hiện tại: {product.StockQuantity}.");
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

                order.TotalAmount += product.Price * item.Quantity;

                product.StockQuantity -= item.Quantity;
            }

            await _unitOfWork.Orders.AddAsync(order);

            await _auditLogService.RecordAsync("CreateOrder", "Order", null, $"Tạo đơn hàng {order.OrderCode}, tổng tiền {order.TotalAmount:N0} đ.", ipAddress, createdByUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Tồn kho sản phẩm vừa thay đổi. Vui lòng kiểm tra lại đơn hàng.", ex);
            }

            return order.Id;
        }

        public async Task CancelAsync(int id, int actorUserId, string ipAddress)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Đơn hàng đã bị hủy.");

            if (order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("Đơn hàng đã hoàn thành nên không thể hủy.");

            var items = await _unitOfWork.OrderItems
                .BuildQuery(i => i.OrderId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new InvalidOperationException("Không tìm thấy sản phẩm trong đơn.");

                product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.PaymentVersion++;

            await _auditLogService.RecordAsync("CancelOrder", "Order", order.Id, $"Hủy đơn hàng {order.OrderCode}.", ipAddress, actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Đơn hàng vừa được thay đổi. Vui lòng thử lại.", ex);
            }
        }

        public async Task UpdateStatusAsync(int id, OrderStatus newStatus, int actorUserId, string ipAddress)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
            {
                throw new InvalidOperationException("Đơn hàng không tồn tại.");
            }

            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            {
                throw new InvalidOperationException("Đơn hàng này không thể đổi trạng thái.");
            }

            bool validTransition = (order.Status == OrderStatus.Pending && newStatus == OrderStatus.Confirmed) || (order.Status == OrderStatus.Confirmed && newStatus == OrderStatus.Shipping) || (order.Status == OrderStatus.Shipping && newStatus == OrderStatus.Completed);

            if (!validTransition)
            {
                throw new InvalidOperationException("Trạng thái mới không đúng quy trình.");
            }

            order.Status = newStatus;
            order.PaymentVersion++;

            await _auditLogService.RecordAsync("UpdateOrderStatus", "Order", order.Id, $"Cập nhật đơn {order.OrderCode} sang trạng thái {newStatus}.", ipAddress, actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Đơn hàng vừa được thay đổi bởi một thao tác khác. Vui lòng tải lại trang và thử lại.", ex);
            }
        }
    }
}
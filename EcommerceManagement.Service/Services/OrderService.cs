using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.DTOs;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Order>> GetAllAsync()
        {
            return await _unitOfWork.Orders
                .BuildQuery(o => true)
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Orders
                .BuildQuery(o => o.Id == id)
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                .ThenInclude(item => item.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync();
        }
        public async Task<int> CreateAsync(CreateOrderRequest request)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("Đơn hàng phải có sản phẩm.");

            if (request.Items.Any(i => i.Quantity <= 0))
                throw new InvalidOperationException("Số lượng phải lớn hơn 0.");

            var customer = await _unitOfWork.Customers
                .GetByIdAsync(request.CustomerId);

            if (customer == null)
                throw new InvalidOperationException("Khách hàng không tồn tại.");

            var creator = await _unitOfWork.Users
                .GetByIdAsync(request.CreatedByUserId);

            if (creator == null || !creator.IsActive)
                throw new InvalidOperationException("Nhân viên tạo đơn không hợp lệ.");

            // 2. Tạo đơn hàng trong bộ nhớ
            var order = new Order
            {
                OrderCode = $"ORD-{Guid.NewGuid():N}",
                CustomerId = request.CustomerId,
                CreatedByUserId = request.CreatedByUserId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Gộp các dòng có cùng ProductId
            var items = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(i => i.Quantity)
                });

            // 4. Kiểm tra từng sản phẩm và xây dựng chi tiết đơn hàng
            foreach (var item in items)
            {
                var product = await _unitOfWork.Products
                    .GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new InvalidOperationException("Sản phẩm không tồn tại.");

                if (product.Status != ProductStatus.Selling)
                    throw new InvalidOperationException($"Sản phẩm {product.Name} đã ngừng bán.");

                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Sản phẩm {product.Name} không đủ tồn kho.");

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

                order.TotalAmount += product.Price * item.Quantity;

                product.StockQuantity -= item.Quantity;
            }

            // 5. Đưa Order cùng các OrderItem vào DbContext
            await _unitOfWork.Orders.AddAsync(order);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Tồn kho đã được thay đổi bởi người khác. Vui lòng tải lại và thử tạo đơn hàng.");
            }

            return order.Id;
        }
        public async Task CancelAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            if (order.Status == OrderStatus.Shipping)
                throw new InvalidOperationException("Không thể hủy đơn hàng đang giao.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Đơn hàng đã bị hủy.");

            if (order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("Không thể hủy đơn hàng đã hoàn thành.");

            bool hasSuccessfulPayment = await _unitOfWork.Payments
                .BuildQuery(p => p.OrderId == id && p.Status == PaymentStatus.Successful)
                .AnyAsync();

            if (hasSuccessfulPayment)
                throw new InvalidOperationException("Đơn hàng đã có thanh toán. Cần xử lý hoàn tiền trước khi hủy.");

            var items = await _unitOfWork.OrderItems
                .BuildQuery(item => item.OrderId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                var product = await _unitOfWork.Products
                    .GetByIdAsync(item.ProductId);

                if (product == null)
                    throw new InvalidOperationException("Không tìm thấy sản phẩm trong đơn hàng.");

                product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;

            order.PaymentVersion++;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Đơn hàng vừa được người khác thay đổi. Vui lòng tải lại và thử lại.", ex);
            }
        }
        public async Task UpdateStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            // Chỉ cho phép Confirmed -> Shipping -> Completed
            bool validTransition =
                (order.Status == OrderStatus.Confirmed && newStatus == OrderStatus.Shipping) ||
                (order.Status == OrderStatus.Shipping && newStatus == OrderStatus.Completed);

            if (!validTransition)
                throw new InvalidOperationException("Không thể chuyển sang trạng thái đơn hàng này.");

            // Kiểm tra đơn đã được thanh toán đủ trước khi giao
            if (newStatus == OrderStatus.Shipping)
            {
                decimal paidAmount = await _unitOfWork.Payments
                    .BuildQuery(p => p.OrderId == id && p.Status == PaymentStatus.Successful)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                if (paidAmount < order.TotalAmount)
                    throw new InvalidOperationException("Đơn hàng chưa được thanh toán đủ.");
            }

            order.Status = newStatus;
            order.PaymentVersion++;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Đơn hàng vừa được người khác thay đổi. Vui lòng tải lại.", ex);
            }
        }
    }
}
using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Payment>> GetByOrderIdAsync(int orderId)
        {
            return await _unitOfWork.Payments
                .BuildQuery(p => p.OrderId == orderId)
                .OrderByDescending(p => p.TransactionAt)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(
            int orderId,
            decimal amount,
            PaymentMethod method,
            string? transactionCode)
        {
            // 1. Kiểm tra đơn hàng
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            if (order.Status == OrderStatus.Cancelled ||
                order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("Không thể thêm thanh toán cho đơn hàng này.");

            // 2. Kiểm tra số tiền thanh toán mới
            if (amount <= 0)
                throw new InvalidOperationException("Số tiền thanh toán phải lớn hơn 0.");

            // 3. Tính tổng tiền đã thanh toán thành công
            decimal paidAmount = await _unitOfWork.Payments
                .BuildQuery(p =>
                    p.OrderId == orderId &&
                    p.Status == PaymentStatus.Successful)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            decimal remainingAmount = order.TotalAmount - paidAmount;

            if (amount > remainingAmount)
                throw new InvalidOperationException(
                    "Số tiền thanh toán vượt quá số tiền còn lại.");

            // 4. Tạo giao dịch thanh toán
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = method,
                TransactionCode = transactionCode,
                TransactionAt = DateTime.UtcNow,
                Status = PaymentStatus.Successful
            };

            await _unitOfWork.Payments.AddAsync(payment);

            if (paidAmount + amount == order.TotalAmount && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Confirmed;
            }

            // Đánh dấu rằng thông tin thanh toán của đơn đã thay đổi
            order.PaymentVersion++;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Đơn hàng vừa có thay đổi thanh toán. Vui lòng tải lại và thử lại.",ex);
            }

            return payment.Id;
        }

        public async Task RefundAsync(int paymentId)
        {
            // 1. Tìm giao dịch cần hoàn tiền
            var payment = await _unitOfWork.Payments
                .GetByIdAsync(paymentId);

            if (payment == null)
                throw new InvalidOperationException("Giao dịch thanh toán không tồn tại.");

            if (payment.Status != PaymentStatus.Successful)
                throw new InvalidOperationException("Chỉ có thể hoàn tiền giao dịch đã thanh toán thành công.");

            // 2. Lấy đơn hàng tương ứng
            var order = await _unitOfWork.Orders
                .GetByIdAsync(payment.OrderId);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            if (order.Status == OrderStatus.Shipping || order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Không thể hoàn tiền cho đơn hàng đang giao, đã hoàn thành hoặc đã hủy.");
            }
            // 3. Đánh dấu giao dịch đã hoàn tiền
            payment.Status = PaymentStatus.Refunded;

            // 4. Tính lại số tiền còn được ghi nhận là đã thanh toán
            decimal paidAmount = await _unitOfWork.Payments
                .BuildQuery(p =>
                    p.OrderId == order.Id &&
                    p.Status == PaymentStatus.Successful &&
                    p.Id != payment.Id)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

           
            // Nếu đơn từng được xác nhận nhưng giờ chưa đủ tiền,chuyển về trạng thái chờ
            if (order.Status == OrderStatus.Confirmed && paidAmount < order.TotalAmount)
            {
                order.Status = OrderStatus.Pending;
            }

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
    }
}
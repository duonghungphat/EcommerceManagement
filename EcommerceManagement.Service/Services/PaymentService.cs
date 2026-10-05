using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public PaymentService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<PaymentListViewModel> SearchAsync(PaymentStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _unitOfWork.Payments
                .BuildQuery(p => true);

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.TransactionAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);

                query = query.Where(p => p.TransactionAt < end);
            }

            var payments = await query
                .OrderByDescending(p => p.TransactionAt)
                .Select(p => new PaymentViewModel
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderCode = p.Order.OrderCode,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    TransactionCode = p.TransactionCode,
                    TransactionAt = p.TransactionAt
                })
                .ToListAsync();

            return new PaymentListViewModel
            {
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Payments = payments
            };
        }

        public async Task<PaymentCreateViewModel?> GetCreateModelAsync(int orderId)
        {
            var order = await _unitOfWork.Orders
                .BuildQuery(o => o.Id == orderId)
                .Select(o => new
                {
                    o.Id,
                    o.OrderCode,
                    o.TotalAmount,
                    o.Status
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return null;

            decimal paidAmount = await _unitOfWork.Payments
                .BuildQuery(p => p.OrderId == orderId && p.Status == PaymentStatus.Successful)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            return new PaymentCreateViewModel
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                RemainingAmount = order.TotalAmount - paidAmount
            };
        }
        public async Task<List<Payment>> GetByOrderIdAsync(int orderId)
        {
            return await _unitOfWork.Payments
                .BuildQuery(p => p.OrderId == orderId)
                .OrderByDescending(p => p.TransactionAt)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(PaymentCreateViewModel model, int actorUserId, string ipAddress)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(model.OrderId);

            if (order == null)
                throw new InvalidOperationException("Đơn hàng không tồn tại.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Không thể thanh toán đơn hàng đã hủy.");

            if (model.Amount <= 0)
                throw new InvalidOperationException("Số tiền phải lớn hơn 0.");

            if (!Enum.IsDefined(typeof(PaymentMethod), model.PaymentMethod))
            {
                throw new InvalidOperationException("Phương thức thanh toán không hợp lệ.");
            }

            if (model.Status != PaymentStatus.Successful && model.Status != PaymentStatus.Pending && model.Status != PaymentStatus.Failed)
            {
                throw new InvalidOperationException("Trạng thái thanh toán không hợp lệ.");
            }

            decimal paidAmount = await _unitOfWork.Payments
                .BuildQuery(p => p.OrderId == order.Id && p.Status == PaymentStatus.Successful)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            decimal remaining = order.TotalAmount - paidAmount;

            if (remaining <= 0)
            {
                throw new InvalidOperationException("Đơn hàng đã được thanh toán đủ.");
            }

            if (model.Amount > remaining)
            {
                throw new InvalidOperationException($"Số tiền vượt quá công nợ còn lại: {remaining:N0} đ.");
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                TransactionCode = model.TransactionCode?.Trim(),
                TransactionAt = DateTime.Now,
                Status = model.Status
            };

            await _unitOfWork.Payments.AddAsync(payment);

            if (model.Status == PaymentStatus.Successful && paidAmount + model.Amount == order.TotalAmount && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Confirmed;
            }

            order.PaymentVersion++;

            await _auditLogService.RecordAsync("CreatePayment", "Payment", null, $"Ghi nhận thanh toán cho đơn {order.OrderCode}, " + $"số tiền {model.Amount:N0} đ.", ipAddress, actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Thông tin thanh toán vừa thay đổi. Vui lòng thử lại.", ex);
            }

            return payment.Id;
        }

        public async Task RefundAsync(int paymentId, int actorUserId, string ipAddress)
        {
            var payment = await _unitOfWork.Payments
                .GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException("Giao dịch không tồn tại.");
            }

            if (payment.Status != PaymentStatus.Successful)
            {
                throw new InvalidOperationException("Chỉ giao dịch thanh toán thành công mới được hoàn tiền.");
            }

            var order = await _unitOfWork.Orders
                .GetByIdAsync(payment.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException("Đơn hàng không tồn tại.");
            }

            // Đánh dấu giao dịch đã được hoàn tiền.
            payment.Status = PaymentStatus.Refunded;

            // Không thay đổi trạng thái đơn hàng.
            // Confirmed vẫn là Confirmed.
            order.PaymentVersion++;

            await _auditLogService.RecordAsync("RefundPayment", "Payment", payment.Id, $"Hoàn tiền giao dịch #{payment.Id}, " + $"số tiền {payment.Amount:N0} đ.", ipAddress, actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Thông tin thanh toán vừa thay đổi. Vui lòng thử lại.", ex);
            }
        }
    }
}
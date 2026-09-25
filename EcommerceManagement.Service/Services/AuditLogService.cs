using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AuditLog>> GetRecentAsync()
        {
            return await _unitOfWork.AuditLogs
                .BuildQuery(log => true)
                .Include(log => log.ApplicationUser)
                .OrderByDescending(log => log.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task RecordAsync(
            string action,
            string entityName,
            int? entityId,
            string? description,
            string ipAddress,
            int? userId)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                IpAddress = ipAddress,
                ApplicationUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(log);
        }
    }
}
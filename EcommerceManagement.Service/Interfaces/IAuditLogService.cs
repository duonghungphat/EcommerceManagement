using EcommerceManagement.Core.Models;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetRecentAsync();

        Task RecordAsync(
            string action,
            string entityName,
            int? entityId,
            string? description,
            string ipAddress,
            int? userId);
    }
}
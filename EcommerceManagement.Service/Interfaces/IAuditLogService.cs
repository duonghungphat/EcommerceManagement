using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogListViewModel> SearchAsync(int? userId, DateTime? fromDate, DateTime? toDate);
        Task<List<AuditLog>> GetRecentAsync();
        Task RecordAsync(string action, string entityName, int? entityId, string? description, string ipAddress, int? userId);
    }
}
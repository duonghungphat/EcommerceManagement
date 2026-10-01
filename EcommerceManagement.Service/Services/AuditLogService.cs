using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
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

        public async Task<AuditLogListViewModel> SearchAsync(int? userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _unitOfWork.AuditLogs
                    .BuildQuery(log => true);

            if (userId.HasValue)
            {
                query = query.Where(log => log.ApplicationUserId == userId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);

                query = query.Where(log => log.CreatedAt < end);
            }

            var logs = await query
                    .OrderByDescending(log => log.CreatedAt)
                    .Select(log =>
                        new AuditLogItemViewModel
                        {
                            Id = log.Id,

                            UserName = log.ApplicationUser != null ? log.ApplicationUser.FullName : "Không xác định",

                            Action = log.Action,

                            EntityName = log.EntityName,

                            EntityId = log.EntityId,

                            Description = log.Description,

                            IpAddress = log.IpAddress,

                            CreatedAt = log.CreatedAt
                        })
                    .ToListAsync();


            var users = await _unitOfWork.Users
                    .BuildQuery(u => true)
                    .OrderBy(u => u.FullName)
                    .Select(u =>
                        new AuditLogUserViewModel
                        {
                            Id = u.Id,
                            FullName = u.FullName
                        })
                    .ToListAsync();


            return new AuditLogListViewModel
            {
                UserId = userId,
                FromDate = fromDate,
                ToDate = toDate,
                Logs = logs,
                Users = users
            };
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

        public async Task RecordAsync(string action, string entityName, int? entityId, string? description, string ipAddress, int? userId)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                IpAddress = ipAddress,
                ApplicationUserId = userId,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.AuditLogs.AddAsync(log);
        }
    }
}
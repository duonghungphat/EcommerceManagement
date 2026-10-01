using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetAsync();
    }
}
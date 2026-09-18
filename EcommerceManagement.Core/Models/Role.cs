namespace EcommerceManagement.Core.Models
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;    
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}

using Rovaya.DAL.Models.Business;

namespace Rovaya.DAL.Repository.Business
{
    public interface IBusinessPlaceRepository
    {
        Task<BusinessPlace> CreateAsync(BusinessPlace place);
        Task<BusinessPlace?> GetByIdAsync(int id);
        Task<List<BusinessPlace>> GetByOwnerIdAsync(string ownerId);
        Task<List<BusinessPlace>> GetAllActiveAsync();
        Task UpdateAsync(BusinessPlace place);
        Task DeleteAsync(int id);
    }
}
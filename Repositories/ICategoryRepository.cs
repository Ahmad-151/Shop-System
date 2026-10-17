using ShopAPI.DTOClasses;

namespace ShopAPI.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDTO>> GetAllAsync();
    Task<CategoryDTO?> GetByIdAsync(int id);
    Task<int?> CreateAsync(string categoryName);
    Task<bool> UpdateAsync(int id, string categoryName);
    Task<bool> DeleteAsync(int id);
}

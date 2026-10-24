using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

namespace ShopAPI.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository) => _repository = repository;

    public async Task<List<CategoryDTO>> GetAllAsync() => await _repository.GetAllAsync();
    public async Task<CategoryDTO?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<int?> CreateAsync(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null; 
        return await _repository.CreateAsync(categoryName);
    }
    public async Task<bool> UpdateAsync(int id, string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return false;

        return await _repository.UpdateAsync(id, categoryName);
    }
    public async Task<bool> DeleteAsync(int id) => await _repository.DeleteAsync(id);
}

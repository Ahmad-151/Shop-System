using ShopAPI.DTOClasses;
using ShopAPI.Response;

namespace ShopAPI.Repositories
{
    public interface IProductRepository
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<int> AddNewAsync(CreateProductDTO createProductDTO);
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO, int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> DisAbleAsync(int id);
        Task<List<ProductDTO>> GetActiveAsync();
        Task<PagedResult<ProductDTO>> GetActivePageAsync(int page, int pageSize);
        Task<bool> RestockAsync(int productId, int quantity, string reason);
    }
}

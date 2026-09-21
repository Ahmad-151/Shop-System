using ShopAPI.DTOClasses;

namespace ShopAPI.Services
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<int> AddNewAsync(CreateProductDTO product);
        Task<ProductDTO> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO, int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> DisAbleAsync(int id);
        Task<List<ProductDTO>> GetActiveAsync();
    }
}

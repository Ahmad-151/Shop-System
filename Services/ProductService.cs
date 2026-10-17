using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Response;

namespace ShopAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<int?> AddNewAsync(CreateProductDTO product)
        {
            if (string.IsNullOrEmpty(product.ProductName) || string.IsNullOrEmpty(product.Description)
                || product.UnitPrice <= 0 || product.CategoryId <= 0 || product.UnitPrice <= 0 || product.CategoryId < 0)
            {
                return null;
            }
            return await _productRepository.AddNewAsync(product);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO, int id)
        {
            if (updateProductDTO.UnitPrice < 0 || string.IsNullOrEmpty(updateProductDTO.Description))
                return false;

            return await _productRepository.UpdateAsync(updateProductDTO, id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<bool> DisAbleAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            return await _productRepository.DisAbleAsync(id);
        }

        public async Task<List<ProductDTO>> GetActiveAsync() => await _productRepository.GetActiveAsync();
        public async Task<bool> RestockAsync(int productId, int quantity, string reason)
        {
            if (productId <= 0 || quantity <= 0 || string.IsNullOrWhiteSpace(reason))
                return false;

            return await _productRepository.RestockAsync(productId, quantity, reason);
        }
        public Task<PagedResult<ProductDTO>> GetActivePageAsync(int page, int pageSize) => _productRepository.GetActivePageAsync(page, pageSize);
    }
}

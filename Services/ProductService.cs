using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

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

        public async Task<int> AddNewAsync(CreateProductDTO product)
        {
            return await _productRepository.AddNewAsync(product);
        }

        public async Task<ProductDTO> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO, int id)
        {
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

        public async Task<List<ProductDTO>> GetActiveAsync()
        {
            return await _productRepository.GetActiveAsync();
        }
    }
}

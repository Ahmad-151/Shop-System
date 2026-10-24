using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Services;

namespace ShopAPI.UnitTests.Product;

public class ProductServiceTests
{
    [Fact]
    public async Task AddProduct_ShouldReturnCreatedProductId()
    {
        // Arrange

        var expectedProductId = 701;

        var createProductDto = new CreateProductDTO
        {
            ProductName = "Wireless Keyboard",
            Description = "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.AddNewAsync(createProductDto)).ReturnsAsync(expectedProductId);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.AddNewAsync(createProductDto);

        // Assert

        Assert.Equal(expectedProductId,result);

        productRepository.Verify(repository =>repository.AddNewAsync(It.Is<CreateProductDTO>
            (product => product.ProductName == "Wireless Keyboard" &&
                        product.Description == "A reliable wireless keyboard for daily office and study use." &&
                        product.UnitPrice == 45.50m &&
                        product.CategoryId == 2)),Times.Once);

    }

    [Fact]
    public async Task DeleteProduct_WhenRepositoryReturnsTrue_ShouldReturnSuccess()
    {
        // Arrange

        var productId = 701;

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.DeleteAsync(productId)).ReturnsAsync(true);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.DeleteAsync(productId);

        // Assert

        Assert.True(result);

        productRepository.Verify(repository =>repository.DeleteAsync(productId),Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_WhenRepositoryReturnsFalse_ShouldReturnFailure()
    {
        // Arrange

        var productId = 999;

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.DeleteAsync(productId)).ReturnsAsync(false);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.DeleteAsync(productId);

        // Assert

        Assert.False(result);

        productRepository.Verify(repository =>repository.DeleteAsync(productId),Times.Once);
    }

    [Fact]
    public async Task DisableProduct_WhenProductExists_ShouldReturnSuccess()
    {
        // Arrange

        var productId = 701;

        var existingProduct = new ProductDTO
        {
            ProductId = productId,
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2,
            RemainingQuantity = 10,
            IsActive = true
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

        productRepository.Setup(repository =>repository.DisAbleAsync(productId)).ReturnsAsync(true);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.DisAbleAsync(productId);

        // Assert

        Assert.True(result);

        productRepository.Verify(repository =>repository.GetByIdAsync(productId),Times.Once);

        productRepository.Verify(repository =>repository.DisAbleAsync(productId),Times.Once);
    }

    [Fact]
    public async Task DisableProduct_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        // Arrange

        var productId = 999;

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetByIdAsync(productId)).ReturnsAsync((ProductDTO)null!);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.DisAbleAsync(productId);

        // Assert

        Assert.False(result);

        productRepository.Verify(repository =>repository.GetByIdAsync(productId),Times.Once);

        productRepository.Verify(repository =>repository.DisAbleAsync(productId),Times.Never);
    }

    [Fact]
    public async Task DisableProduct_WhenRepositoryReturnsFalse_ShouldReturnFailure()
    {
        // Arrange

        var productId = 701;

        var existingProduct = new ProductDTO
        {
            ProductId = productId,
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2,
            RemainingQuantity = 10,
            IsActive = true
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

        productRepository.Setup(repository =>repository.DisAbleAsync(productId)).ReturnsAsync(false);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.DisAbleAsync(productId);

        // Assert

        Assert.False(result);

        productRepository.Verify(repository =>repository.GetByIdAsync(productId),Times.Once);

        productRepository.Verify(repository =>repository.DisAbleAsync(productId),Times.Once);
    }

    [Fact]
    public async Task GetActiveProducts_ShouldReturnOnlyActiveProducts()
    {
        // Arrange

        var activeProducts = new List<ProductDTO>
    {
        new ProductDTO
        {
            ProductId = 701,
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2,
            RemainingQuantity = 10,
            IsActive = true
        },
        new ProductDTO
        {
            ProductId = 702,
            ProductName = "Wireless Mouse",
            Description =
                "A comfortable wireless mouse for office and study work.",
            UnitPrice = 25.75m,
            CategoryId = 2,
            RemainingQuantity = 15,
            IsActive = true
        }
    };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetActiveAsync()).ReturnsAsync(activeProducts);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.GetActiveAsync();

        // Assert

        Assert.NotNull(result);

        Assert.Equal(2,result.Count);

        Assert.All(result,product => Assert.True(product.IsActive));

        Assert.Contains(result,product => product.ProductId == 701);

        Assert.Contains(result,product => product.ProductId == 702);

        productRepository.Verify(repository =>repository.GetActiveAsync(),Times.Once);
    }

    [Fact]
    public async Task GetActiveProducts_WhenNoActiveProductsExist_ShouldReturnEmptyList()
    {
        // Arrange

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetActiveAsync()).ReturnsAsync(new List<ProductDTO>());

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.GetActiveAsync();

        // Assert

        Assert.NotNull(result);
        Assert.Empty(result);

        productRepository.Verify(repository =>repository.GetActiveAsync(),Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProductsIncludingDisabled()
    {
        // Arrange

        var allProducts = new List<ProductDTO>
    {
        new ProductDTO
        {
            ProductId = 701,
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2,
            RemainingQuantity = 10,
            IsActive = true
        },
        new ProductDTO
        {
            ProductId = 702,
            ProductName = "Old Wireless Mouse",
            Description =
                "An old wireless mouse kept for historical order records.",
            UnitPrice = 25.75m,
            CategoryId = 2,
            RemainingQuantity = 0,
            IsActive = false
        }
    };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetAllAsync()).ReturnsAsync(allProducts);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.GetAllAsync();

        // Assert

        Assert.NotNull(result);

        Assert.Equal(2,result.Count);

        Assert.Contains(result,product => product.ProductId == 701 &&product.IsActive);

        Assert.Contains(result,product => product.ProductId == 702 &&!product.IsActive);

        productRepository.Verify(repository =>repository.GetAllAsync(),Times.Once);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange

        var productId = 701;

        var expectedProduct = new ProductDTO
        {
            ProductId = productId,
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2,
            RemainingQuantity = 10,
            IsActive = true
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetByIdAsync(productId)).ReturnsAsync(expectedProduct);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.GetByIdAsync(productId);

        // Assert

        Assert.NotNull(result);

        Assert.Equal(expectedProduct.ProductId,result.ProductId);

        Assert.Equal(expectedProduct.ProductName,result.ProductName);

        Assert.Equal(expectedProduct.Description,result.Description);

        Assert.Equal(expectedProduct.UnitPrice,result.UnitPrice);

        Assert.Equal(expectedProduct.CategoryId,result.CategoryId);

        Assert.Equal(expectedProduct.RemainingQuantity,result.RemainingQuantity);

        Assert.Equal(expectedProduct.IsActive,result.IsActive);

        productRepository.Verify(repository =>repository.GetByIdAsync(productId),Times.Once);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        var productId = 999;

        var productRepository =
            new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.GetByIdAsync(productId)).ReturnsAsync((ProductDTO)null!);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.GetByIdAsync(productId);

        // Assert

        Assert.Null(result);

        productRepository.Verify(repository =>repository.GetByIdAsync(productId),Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WhenRepositoryReturnsTrue_ShouldReturnSuccess()
    {
        // Arrange

        var productId = 701;

        var updateProductDto = new UpdateProductDTO
        {
            Description =
                "Updated wireless keyboard description for office and study use.",
            UnitPrice = 49.99m
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.UpdateAsync(updateProductDto,productId)).ReturnsAsync(true);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.UpdateAsync(updateProductDto,productId);

        // Assert

        Assert.True(result);

        productRepository.Verify(repository =>repository.UpdateAsync(updateProductDto,productId),Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WhenRepositoryReturnsFalse_ShouldReturnFailure()
    {
        // Arrange

        var productId = 999;

        var updateProductDto = new UpdateProductDTO
        {
            Description =
                "Updated description for a product that may not exist.",
            UnitPrice = 39.99m
        };

        var productRepository =new Mock<IProductRepository>();

        productRepository.Setup(repository =>repository.UpdateAsync(updateProductDto,productId)).ReturnsAsync(false);

        var productService =new ProductService(productRepository.Object);

        // Act

        var result =await productService.UpdateAsync(updateProductDto,productId);

        // Assert

        Assert.False(result);

        productRepository.Verify(repository =>repository.UpdateAsync(updateProductDto,productId),Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RestockProduct_ShouldReturnRepositoryResult(bool expectedResult)
    {
        const int productId = 701;
        const int quantity = 10;
        const string reason = "Initial stock";

        var productRepository = new Mock<IProductRepository>();

        productRepository
            .Setup(repository => repository.RestockAsync(productId, quantity, reason))
            .ReturnsAsync(expectedResult);

        var productService = new ProductService(productRepository.Object);

        var result = await productService.RestockAsync(productId, quantity, reason);

        Assert.Equal(expectedResult, result);

        productRepository.Verify(
            repository => repository.RestockAsync(productId, quantity, reason),
            Times.Once);
    }

    [Fact]
    public async Task RestockProduct_WithInvalidInput_ShouldReturnFalseWithoutCallingRepository()
    {
        var productRepository = new Mock<IProductRepository>();
        var productService = new ProductService(productRepository.Object);

        Assert.False(await productService.RestockAsync(0, 10, "Initial stock"));
        Assert.False(await productService.RestockAsync(701, 0, "Initial stock"));
        Assert.False(await productService.RestockAsync(701, 10, " "));

        productRepository.Verify(
            repository => repository.RestockAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never);
    }

}

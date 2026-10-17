using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Services;

namespace ShopAPI.UnitTests.Category;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnCategories()
    {
        var expected = new List<CategoryDTO> { new() { CategoryId = 1, CategoryName = "Books" } };
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.GetAllAsync()).ReturnsAsync(expected);
        var service = new CategoryService(repository.Object);
        Assert.Same(expected, await service.GetAllAsync());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedId()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.CreateAsync("Books")).ReturnsAsync(5);
        var service = new CategoryService(repository.Object);
        Assert.Equal(5, await service.CreateAsync("Books"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnRepositoryResult()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.UpdateAsync(5, "Books")).ReturnsAsync(true);
        var service = new CategoryService(repository.Object);
        Assert.True(await service.UpdateAsync(5, "Books"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnRepositoryResult()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.DeleteAsync(5)).ReturnsAsync(true);
        var service = new CategoryService(repository.Object);
        Assert.True(await service.DeleteAsync(5));
    }
}

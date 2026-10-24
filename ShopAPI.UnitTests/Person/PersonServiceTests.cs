using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Services;

namespace ShopAPI.UnitTests.Person;

public sealed class PersonServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnRepositoryResults()
    {
        var people = new List<PersonDTO> { new() { PersonId = 1, PersonName = "Ahmad" } };
        var peopleRepository = new Mock<IPersonRepository>();
        peopleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(people);
        var service = new PersonService(peopleRepository.Object, Mock.Of<IRefreshTokenRepository>());
        var result = await service.GetAllAsync();
        Assert.Same(people, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRepositoryResult()
    {
        var person = new PersonDTO { PersonId = 4, PersonName = "Ahmad" };
        var repository = new Mock<IPersonRepository>();
        repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(person);
        var service = new PersonService(repository.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.Same(person, await service.GetByIdAsync(4));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnRepositoryResult()
    {
        var repository = new Mock<IPersonRepository>();
        var dto = new UpdatePersonDTO { PhoneNumber = "0912345678", Address = "Damascus" };
        repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new PersonDTO { PersonId = 4, PhoneNumber = dto.PhoneNumber });
        repository.Setup(x => x.UpdateAsync(dto, 4)).ReturnsAsync(true);
        var service = new PersonService(repository.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.True(await service.UpdateAsync(dto, 4));
    }

    [Fact]
    public async Task UpdateAsync_WhenPhoneBelongsToAnotherPerson_ShouldReturnFalse()
    {
        var repository = new Mock<IPersonRepository>();
        repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new PersonDTO { PersonId = 4, PhoneNumber = "0912345678" });
        repository.Setup(x => x.GetByPhoneNumberAsync("0922222222")).ReturnsAsync(new PersonAuthDTO { Person_ID = 9 });
        var service = new PersonService(repository.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.False(await service.UpdateAsync(new UpdatePersonDTO { PhoneNumber = "0922222222", Address = "Damascus" }, 4));
        repository.Verify(x => x.UpdateAsync(It.IsAny<UpdatePersonDTO>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ShouldRevokeAllSessions()
    {
        var people = new Mock<IPersonRepository>();
        var tokens = new Mock<IRefreshTokenRepository>();
        people.Setup(x => x.DeleteAsync(4)).ReturnsAsync(true);
        tokens.Setup(x => x.RevokeAllForPersonAsync(4)).ReturnsAsync(true);
        var service = new PersonService(people.Object, tokens.Object);
        Assert.True(await service.DeleteAsync(4));
        tokens.Verify(x => x.RevokeAllForPersonAsync(4), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFailed_ShouldNotRevokeSessions()
    {
        var people = new Mock<IPersonRepository>();
        var tokens = new Mock<IRefreshTokenRepository>();
        people.Setup(x => x.DeleteAsync(4)).ReturnsAsync(false);
        var service = new PersonService(people.Object, tokens.Object);
        Assert.False(await service.DeleteAsync(4));
        tokens.Verify(x => x.RevokeAllForPersonAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BannAsync_WhenPersonDoesNotExist_ShouldReturnFalse()
    {
        var people = new Mock<IPersonRepository>();
        people.Setup(x => x.GetByIdAsync(4)).ReturnsAsync((PersonDTO?)null);
        var service = new PersonService(people.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.False(await service.BannAsync(new BannedPersonDTO { Reason = "Repeated abuse" }, 4));
        people.Verify(x => x.BannAsync(It.IsAny<BannedPersonDTO>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BannAsync_WhenSuccessful_ShouldRevokeAllSessions()
    {
        var people = new Mock<IPersonRepository>();
        var tokens = new Mock<IRefreshTokenRepository>();
        people.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new PersonDTO { PersonId = 4 });
        people.Setup(x => x.BannAsync(It.IsAny<BannedPersonDTO>(), 4)).ReturnsAsync(true);
        tokens.Setup(x => x.RevokeAllForPersonAsync(4)).ReturnsAsync(true);
        var service = new PersonService(people.Object, tokens.Object);
        Assert.True(await service.BannAsync(new BannedPersonDTO { Reason = "Repeated abuse" }, 4));
        tokens.Verify(x => x.RevokeAllForPersonAsync(4), Times.Once);
    }

    [Fact]
    public async Task AddToFavouriteAsync_WhenPersonDoesNotExist_ShouldReturnFalse()
    {
        var people = new Mock<IPersonRepository>();
        people.Setup(x => x.GetByIdAsync(4)).ReturnsAsync((PersonDTO?)null);
        var service = new PersonService(people.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.False(await service.AddToFavouriteAsync(10, 4));
        people.Verify(x => x.AddToFavouriteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddToFavouriteAsync_WhenPersonExists_ShouldDelegate()
    {
        var people = new Mock<IPersonRepository>();
        people.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new PersonDTO { PersonId = 4 });
        people.Setup(x => x.AddToFavouriteAsync(10, 4)).ReturnsAsync(true);
        var service = new PersonService(people.Object, Mock.Of<IRefreshTokenRepository>());
        Assert.True(await service.AddToFavouriteAsync(10, 4));
        people.Verify(x => x.AddToFavouriteAsync(10, 4), Times.Once);
    }
}

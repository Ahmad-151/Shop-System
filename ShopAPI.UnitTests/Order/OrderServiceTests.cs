using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Services;

namespace ShopAPI.UnitTests.Order;

public class OrderServiceTests
{
    [Fact]
    public async Task AddNewOrder_WithInsufficientStock_ShouldReturnFailure()
    {
        // Arrange

        var productId = 1;
        var requestedQuantity = 5;
        var availableQuantity = 2; // !! Less than requested quantity
        var personId = 100;

        var orderDto = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = productId,
                    Quantity = requestedQuantity
                }
            }
        };

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository => repository.GetQuantity(productId)).ReturnsAsync(availableQuantity);

        var orderService = new OrderService(orderRepository.Object);

        // Act

        var result = await orderService.AddNewAsync(orderDto,personId);

        // Assert

        Assert.False(result.Succeeded);

        Assert.Null(result.OrderId);

        Assert.Equal(productId,result.OutOfStockProductId);

        Assert.Equal(availableQuantity,result.AvailableQuantity);

        orderRepository.Verify(repository =>repository.GetQuantity(productId),Times.Once);

        orderRepository.Verify(repository =>repository.AddNewAsync(It.IsAny<CreateOrderDTO>(),It.IsAny<int>()),Times.Never);
    }

    [Fact]
    public async Task AddNewOrder_WithSufficientStock_ShouldCreateOrder()
    {
        // Arrange

        var productId = 1;
        var requestedQuantity = 3;
        var availableQuantity = 10;
        var personId = 100;
        var expectedOrderId = 501;

        var orderDto = new CreateOrderDTO
        { Items = new List<CreateOrderItemsDTO> { new CreateOrderItemsDTO { ProductId = productId, Quantity = requestedQuantity } } };

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.GetQuantity(productId)).ReturnsAsync(availableQuantity);

        orderRepository.Setup(repository =>repository.AddNewAsync(orderDto,personId)).ReturnsAsync(expectedOrderId);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.AddNewAsync(orderDto,personId);

        // Assert

        Assert.True(result.Succeeded);

        Assert.Equal(expectedOrderId,result.OrderId);

        Assert.Null(result.OutOfStockProductId);

        Assert.Equal(0,result.AvailableQuantity);

        orderRepository.Verify(repository =>repository.GetQuantity(productId),Times.Once);

        orderRepository.Verify(repository => repository.AddNewAsync
        (It.Is<CreateOrderDTO>
        (order =>order.Items.Count == 1 &&order.Items[0].ProductId == productId &&order.Items[0].Quantity == requestedQuantity),personId),
        Times.Once);

    }

    [Fact]
    public async Task AddNewOrder_WithMultipleItems_WhenOneProductHasInsufficientStock_ShouldReturnFailure()
    {
        // Arrange

        var firstProductId = 1;
        var firstProductRequestedQuantity = 2;
        var firstProductAvailableQuantity = 10;

        var secondProductId = 2;
        var secondProductRequestedQuantity = 4;
        var secondProductAvailableQuantity = 3;

        var personId = 100;

        var orderDto = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
        {
            new CreateOrderItemsDTO{ProductId = firstProductId,Quantity = firstProductRequestedQuantity},
            new CreateOrderItemsDTO{ProductId = secondProductId,Quantity = secondProductRequestedQuantity}
        }
        };

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.GetQuantity(firstProductId)).ReturnsAsync(firstProductAvailableQuantity);

        orderRepository.Setup(repository =>repository.GetQuantity(secondProductId)).ReturnsAsync(secondProductAvailableQuantity);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.AddNewAsync(orderDto,personId);

        // Assert

        Assert.False(result.Succeeded);

        Assert.Null(result.OrderId);

        Assert.Equal(secondProductId,result.OutOfStockProductId);

        Assert.Equal(secondProductAvailableQuantity,result.AvailableQuantity);

        orderRepository.Verify(repository =>repository.GetQuantity(firstProductId),Times.Once);

        orderRepository.Verify(repository =>repository.GetQuantity(secondProductId),Times.Once);

        orderRepository.Verify(repository =>repository.AddNewAsync(It.IsAny<CreateOrderDTO>(),It.IsAny<int>()),Times.Never);
    }

    [Fact]
    public async Task AddNewOrder_WithMultipleItems_WhenAllProductsHaveSufficientStock_ShouldCreateOrder()
    {
        // Arrange

        var firstProductId = 1;
        var firstProductRequestedQuantity = 2;
        var firstProductAvailableQuantity = 10;

        var secondProductId = 2;
        var secondProductRequestedQuantity = 4;
        var secondProductAvailableQuantity = 8;

        var personId = 100;
        var expectedOrderId = 502;

        var orderDto = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
        {
            new CreateOrderItemsDTO{ProductId = firstProductId,Quantity = firstProductRequestedQuantity},
            new CreateOrderItemsDTO{ProductId = secondProductId,Quantity = secondProductRequestedQuantity}
        }
        };

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.GetQuantity(firstProductId)).ReturnsAsync(firstProductAvailableQuantity);

        orderRepository.Setup(repository =>repository.GetQuantity(secondProductId)).ReturnsAsync(secondProductAvailableQuantity);

        orderRepository.Setup(repository =>repository.AddNewAsync(orderDto,personId)).ReturnsAsync(expectedOrderId);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.AddNewAsync(orderDto,personId);

        // Assert

        Assert.True(result.Succeeded);

        Assert.Equal(expectedOrderId,result.OrderId);

        Assert.Null(result.OutOfStockProductId);

        Assert.Equal(0,result.AvailableQuantity);

        orderRepository.Verify(repository =>repository.GetQuantity(firstProductId),Times.Once);

        orderRepository.Verify(repository =>repository.GetQuantity(secondProductId),Times.Once);

        orderRepository.Verify(repository =>repository.AddNewAsync(orderDto,personId),Times.Once);
    }


    [Fact]
    public async Task AddNewOrder_WithIdempotencyKey_ShouldSendKeyAndRequestHashToRepository()
    {
        var personId = 100;
        var orderDto = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new() { ProductId = 2, Quantity = 3 },
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        var repository = new Mock<IOrderRepository>();
        repository
            .Setup(x => x.AddNewAsync(orderDto, personId, "checkout-1", It.IsAny<string>()))
            .ReturnsAsync(9001);

        var service = new OrderService(repository.Object);

        var result = await service.AddNewAsync(orderDto, personId, "checkout-1");

        Assert.True(result.Succeeded);
        Assert.Equal(9001, result.OrderId);
        repository.Verify(x => x.AddNewAsync(
            orderDto,
            personId,
            "checkout-1",
            It.Is<string>(hash => hash.Length == 64)), Times.Once);
        repository.Verify(x => x.GetQuantity(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrder_WhenRepositoryReturnsTrue_ShouldReturnSuccess()
    {
        // Arrange

        var orderId = 501;

        var updateOrderDto = new UpdateOrderDTO{Status = "Confirmed",DeliveredAt = null};

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.UpdateAsync(updateOrderDto,orderId)).ReturnsAsync(true);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.UpdateAsync(updateOrderDto,orderId);

        // Assert

        Assert.True(result);

        orderRepository.Verify(repository =>repository.UpdateAsync(It.Is<UpdateOrderDTO>(dto =>dto.Status == "Confirmed" &&
                               dto.DeliveredAt == null),orderId),Times.Once);

    }

    [Fact]
    public async Task UpdateOrder_WhenRepositoryReturnsFalse_ShouldReturnFailure()
    {
        // Arrange

        var orderId = 999;

        var updateOrderDto = new UpdateOrderDTO{Status = "Confirmed",DeliveredAt = null};

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.UpdateAsync(updateOrderDto,orderId)).ReturnsAsync(false);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.UpdateAsync(updateOrderDto,orderId);

        // Assert

        Assert.False(result);

        orderRepository.Verify(repository =>repository.UpdateAsync(updateOrderDto,orderId),Times.Once);
    }

    [Fact]
    public async Task DeleteOrder_WhenRepositoryReturnsTrue_ShouldReturnSuccess()
    {
        // Arrange

        var orderId = 501;

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.DeleteAsync(orderId)).ReturnsAsync(true);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.DeleteAsync(orderId);

        // Assert

        Assert.True(result);

        orderRepository.Verify(repository =>repository.DeleteAsync(orderId),Times.Once);
    }

    [Fact]
    public async Task DeleteOrder_WhenRepositoryReturnsFalse_ShouldReturnFailure()
    {
        // Arrange

        var orderId = 999;

        var orderRepository =new Mock<IOrderRepository>();

        orderRepository.Setup(repository =>repository.DeleteAsync(orderId)).ReturnsAsync(false);

        var orderService =new OrderService(orderRepository.Object);

        // Act

        var result =await orderService.DeleteAsync(orderId);

        // Assert

        Assert.False(result);

        orderRepository.Verify(repository =>repository.DeleteAsync(orderId),Times.Once);
    }



}

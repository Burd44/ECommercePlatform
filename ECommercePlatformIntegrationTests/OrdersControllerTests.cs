using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs;
using Domain.Entities;

namespace ECommercePlatformIntegrationTests
{
    public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public OrdersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GivenOrdersExist_WhenGettingAllOrders_ThenShouldReturnOkResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/orders");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GivenExistingOrder_WhenGettingOrderById_ThenShouldReturnOkResponse()
        {
            // Arrange
            var orderId = "ebf164aa-ca71-4cd3-beac-70daf6173268";

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GivenNonExistingOrder_WhenGettingOrderById_ThenShouldReturnNotFoundResponse()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GivenValidOrderRequest_WhenCreatingOrder_ThenShouldReturnCreatedResponse()
        {
            // Arrange
            var newOrder = new OrderDto
            {
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                Status = Status.Pending,
                PaymentId = Guid.NewGuid()
            };
            var content = new StringContent(JsonSerializer.Serialize(newOrder), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/orders", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GivenExistingOrder_WhenDeletingOrder_ThenShouldReturnNoContentResponse()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
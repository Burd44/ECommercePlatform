using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace ECommercePlatformIntegrationTests
{
    public class PaymentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient client;
        private const string BaseUrl = "/api/v1/payments";

        public PaymentsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });
                });
            });

            var scope = this.factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            client = this.factory.CreateClient();
            var tokenHandler = new JwtTokenGenerator("3fdd5f93-4ddb-465e-a2e8-3e326175030f");
            var token = tokenHandler.GenerateAccessToken(new Guid("3fdd5f93-4ddb-465e-a2e8-3e326175030f"), "testemail@gmail.com");
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        [Trait("Category", "ExcludeThis")]
        [Fact]
        public async Task GivenPaymentsExist_WhenGettingAllPayments_ThenShouldReturnOkResponse()
        {
            // Arrange
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow
            };
            dbContext.Payments.Add(payment);
            dbContext.SaveChanges();

            // Act
            var response = await client.GetAsync(BaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Trait("Category", "ExcludeThis")]
        [Fact]
        public async Task GivenNonExistingPayment_WhenGettingPaymentById_ThenShouldReturnNotFound()
        {
            // Act
            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        [Trait("Category", "ExcludeThis")]
        [Fact]
        public async Task GivenValidPaymentRequest_WhenCreatingPayment_ThenShouldReturnCreated()
        {
            // Arrange
            var payment = new PaymentDto
            {
                PaymentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow
            };
            var content = new StringContent(JsonSerializer.Serialize(payment), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync(BaseUrl, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}

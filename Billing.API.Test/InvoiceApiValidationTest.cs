using System;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Billing.API.Test
{
    public class InvoiceApiValidationTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpTest _httpTest;

        public InvoiceApiValidationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _httpTest = new HttpTest();
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("", 1)]
        public async Task GetInvoices_InexistentClient_ReturnsNotFound(string origin, int clientId)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/accounts/{origin}/{clientId}/invoices");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();
        }
    }
}

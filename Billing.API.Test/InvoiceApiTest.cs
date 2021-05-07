using AutoFixture.Xunit2;
using Billing.API.Models;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Billing.API.Test
{
    public class InvoiceApiTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpTest _httpTest;

        public InvoiceApiTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _httpTest = new HttpTest();
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        [Theory]
        [AutoData]
        public async Task GetInvoices_WhenInvalidPath_ReturnsNotFound(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/{url}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        // isSU=true
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw")]
        // "isSu": false,
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ3MDksImV4cCI6MTU5Nzc2NDcxOSwiaWF0IjoxNTk3NzY0NzA5LCJpc1NVIjpmYWxzZX0.QDZolMwgEVP18-coDEbWajFbjhqPGFGOgHQusTda1gid__FzCO5w1idGhMoAuiyfRdVVzuF9I5Iz_Opx020xVkyPUl3EDU32-RHn2OBQOtmOlvna2cJyeQk0LwsWTf1lnvUKamBKUeztl2IXJXNcXwXt9y7hC6fMlYsn3hDRA0YcIfv1Q37iz8_cHYQ7O2HB1JuZRUwkhfobMYvXDLt3GS8u8MNSM_hKTmlf6wII-jRG-G25ePFibkChld2Rc5cjzVQy_VM9q83BZiSSeaoLUm0NNw49eACiQ50KY_YhY2GeEnptA1p3JicKMGWB_RNp3MdC632EZmtPtCjn8TkRHA")]
        // without isSU and nameId
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1NzcsImV4cCI6MTU5Nzc2NDU4NywiaWF0IjoxNTk3NzY0NTc3fQ.bIUKKuIZOPZapoB05v3N5h_dHfu7R_O_DZ2pu2j3esJd3kwUjxEwqVVI_l97yBMScaCnbsdEyt4w1nKYwI5vj6UQR7GJoR6TERPfFtpiO0zlGEIWPJu9zI3fgA7HfJifw5B6fQidDHDYUbbM3oHD9cn7CiB4XizEe-6LGnjlBzo5Hr1Rsrz6-eD5UQhx7FkqLLRFDhIQ9cn_36Wc9ylzfvmzKZ4ZAn4Q5-s3f2rkN-tuXiBAxrwkgXhOZ72f8dj5mED6PLauH3uPEbaMcrVKD-CIe9Una5zq-zWtsZVasSQeO1_lCjQzhhTQXfwrWJ9WBx1ozkDA9XzJiiS_jAMqMA")]
        // invalid token
        [InlineData(HttpStatusCode.Unauthorized, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9")]
        // "nameid": 222541, "isSu": false,
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjIyMjU0MSwidW5pcXVlX25hbWUiOiJiYW1hcm9AbWFraW5nc2Vuc2UuY29tIiwiaXNTdSI6ZmFsc2UsInN1YiI6ImJhbWFyb0BtYWtpbmdzZW5zZS5jb20iLCJjdXN0b21lcklkIjpudWxsLCJjZGhfY3VzdG9tZXJJZCI6bnVsbCwicm9sZSI6IlVTRVIiLCJpYXQiOjE2MDE5MTgyNzYsImV4cCI6MTYwMTkyMDA3Nn0.jI7WmdgECN2i7DpGf4xx4Q8uzy0v1LNlt__lnMi5Gl4r2bNJsmTJNGt1VIvsdp1YfoxZ9eIwilND7kGu6nkRo90C_HYBbRxjP1fHhbFSYyLAtLNAU6T93cCt0abWCuY-_x5NlbkeehpOZ0QxbxKYCM4yhE8XBcOh1_DZ-lOWMfegZAjhGtzVnmgbR7qxJsV893suzDGvLwkwaevmakJC-jCF6eCyZcdRLQ2GnEa3ZbKvRUow9yf1B5lBGywqnv83vQPxKb63k2Khy0UETKcylviF6i0oOPLm3U6Z7Gv-Lmh5AZsTN6gJrkAVSTVkUL2hizegAfkTMvDpPtQETymgyQ")]
        //"nameid": 50018, "isSu": false,
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjUwMDE4LCJ1bmlxdWVfbmFtZSI6ImNiZXJuYXRAZ2V0Y3MuY29tIiwiaXNTdSI6ZmFsc2UsInN1YiI6ImNiZXJuYXRAZ2V0Y3MuY29tIiwiY3VzdG9tZXJJZCI6IjEwMjQiLCJjZGhfY3VzdG9tZXJJZCI6IjEwMjQiLCJyb2xlIjoiVVNFUiIsImlhdCI6MTYwMjY4NTEzMiwiZXhwIjoxNjAyNjg2OTMyfQ.cKCNWIXKJ_WkA-MR8SvcbRkFpRaHvF-Jm2o_NZD2vxgl7aP7AJLcYRsC7aSfztStZgE4H6idyZwLN4USdsbqgvcgOXGqLSrXJach0tbp7VvrMYwluIqZgDHeYrN0QRWSrvUMtSyHGs05N4MYuaR6xnq0fwdFTeXMv4xGAp6YQO2d-y2Y8Ktb3drayrkhnsw-ge_bNMSRGwszHAB5IPesStHFYWrYi5Snz_WkwaeuRyWIBNAdg2Eeqz6g3rM3WmBtnEf43nb-vtorLhOG7Dk4C8DaUdXZjSqPE7e9l11dlaCs5w-6izn89nnNR2tCFKzmXICyDMGbiMvIq98CGz-Edg")]
        public async Task GetInvoices_WhenToken_ReturnsResponse(HttpStatusCode httpStatusCode, string token)
        {
            // Arrange
            const int clientId = 222541;

            _httpTest.RespondWithJson(string.Empty);

            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true",
                    ["Invoice:Host"] = "localhost",
                    ["Invoice:UserName"] = "someUser",
                    ["Invoice:Password"] = "somePass"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost/accounts/doppler/{clientId}/invoices");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                Assert.Equal(httpStatusCode, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_ShouldNotCallBackend_ReturnsOk()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoices");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Asc_When_OrderAsc_is_not_Passed_As_Parameter()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?sortColumn=Product");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 1", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 10", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 11", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 12", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 13", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 14", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 15", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 16", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 17", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 18", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Desc_When_OrderAsc_is_False()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?sortColumn=Product&sortAsc=false");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 9", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 8", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 7", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 6", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 50", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 5", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 49", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 48", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 47", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 46", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Default_When_sortColumn_And_sortAsc_Are_Empty()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?pageSize=2");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Equal(2, result.Items.Count);
                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("CD0000000000001", invoice1.AccountId),
                    invoice2 => Assert.Equal("CD0000000000001", invoice2.AccountId));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Asc_When_Page_is_2()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?page=2&sortColumn=Product&sortAsc=true");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 19", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 2", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 20", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 21", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 22", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 23", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 24", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 25", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 26", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 27", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_ReturnsData()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoices");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenValidSUToken_ReturnsOk()
        {
            // Arrange
            _httpTest.RespondWithJson(string.Empty);

            var appFactory = _factory.WithDisabledLifeTimeValidation();

            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/accounts/doppler/1/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.WithDisabledLifeTimeValidation()
                .CreateClient();

            // Act
            var response = await client.GetAsync("https://custom.domain.com/accounts/doppler/1/invoices");

            // Assert
            var authenticateHeader = Assert.Single(response.Headers.WwwAuthenticate);
            Assert.Equal("Bearer", authenticateHeader.Scheme);
            Assert.Null(authenticateHeader.Parameter);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenTokenExpired_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjg4NDY5LCJ1bmlxdWVfbmFtZSI6ImFtb3NjaGluaUBtYWtpbmdzZW5zZS5jb20iLCJpc1N1IjpmYWxzZSwic3ViIjoiYW1vc2NoaW5pQG1ha2luZ3NlbnNlLmNvbSIsImN1c3RvbWVySWQiOiIxMzY3IiwiY2RoX2N1c3RvbWVySWQiOiIxMzY3Iiwicm9sZSI6IlVTRVIiLCJpYXQiOjE1OTQxNTUwMjYsImV4cCI6MTU5NDE1NjgyNn0.bv-ZHKulKMhBjcftiS-G_xa6MqPd8vmTJLCkitkSzz_lH6OblXnlLSjGAtoViT0yQun_IVqUggdfgY-Qv6cS_YeiYT-EqVLI1KFsFoWtZ7E1Yp5LZuVW70GskwZ7YbV7qlPrOOVBUbt6bD4LtwxudJmIenNBIgIVV-dCTl6vQNXRY65af7Ak1BG8IJxBaPhiFPniMIfNi_6my7NiHtL7Db2eeYgIxXf5_R-8BZFQ0CxWzNDTpdfaB48SnC7n6aEg9FQdOxcu8XX4qPBjGfnvCui2J9s8XgLfRtVQ27WwletL9XnGq79Dyp2PdNUsCcR2d4CMRxvzK1rO2jXSJ9Rf7w");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var authenticateHeader = Assert.Single(response.Headers.WwwAuthenticate);
            Assert.Equal("Bearer", authenticateHeader.Scheme);
            Assert.Contains("error=\"invalid_token\"", authenticateHeader.Parameter);
            Assert.Contains("error_description=\"The token is expired", authenticateHeader.Parameter);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenNoParameters_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("https://localhost/accounts/doppler/invoices");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("accounts/doppler/1/invoices/invoice_2020-01-01_123.pdf?_s=sOvP2buZIW8IFDAXMIe8BROG9GoB7zLPCzAv3OzVs")]
        [InlineData("accounts/doppler/1/invoices/invoice_AR_2020-01-01_123.pdf?_s=6RTlC0JYyqMTj1LaHAbnTWQFLC9feeUNSNzzkRlE")]
        public async Task GetInvoiceFile_WithNoTokenAndValidSignature_ShouldReturnPdfFileContents(string path)
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/{path}");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/pdf", response.Content.Headers.ContentType.MediaType);
                Assert.NotNull(content);
            }
        }

        [Theory]
        [InlineData("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw", "accounts/doppler/1/invoices/invoice_AR_2020-01-01_123.pdf?_s=792naTFnk0doxkAi3G4Dt2ITSQttLcf6OypamgK123")]
        public async Task GetInvoiceFile_WithTokenAndInvalidSignature_ShouldReturnPdfFileContents(string token, string path)
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/{path}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Act
                var response = await client.SendAsync(request);
                var content  = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/pdf", response.Content.Headers.ContentType.MediaType);
                Assert.NotNull(content);
            }
        }

        [Theory]
        [InlineData("accounts/doppler/1/invoices/invoice_2020-01-01_123.pdf?_s=792naTFnk0doxkAi3G4Dt2ITSQttLcf6OypamgK123")]
        public async Task GetInvoiceFile_WithNoTokenAndInvalidSignature_ShouldReturnUnauthorized(string path)
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/{path}");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenInvalidClientOrigin_ReturnsBadRequest()
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/invalid_origin/1/invoices/filename.ext?s=123456");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenInvalidClientId_ReturnsNotFound()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/0/invoices/invoice_2020-01-01_123.pdf?s=123456");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenWrongFilePattern_ReturnsBadRequest()
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoices/whatever.ext");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoices_Should_Returns_Balance_Equal_Amount_Minus_PaidToDate()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?page=2&sortColumn=Product&sortAsc=true");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.All(result.Items,
                    item => Assert.Equal(item.Amount - item.PaidToDate, item.Balance));
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenInvalidSapSystem_ReturnsBadRequest()
        {
            // Arrange
            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"accounts/doppler/1/invoices/invoice_MX_2020-01-01_123.pdf?_s=792naTFnk0doxkAi3G4Dt2ITSQttLcf6OypamgK123");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal($"The sapSystem 'MX' is not supported. Only supports: AR, US", content);
            }
        }
    }
}

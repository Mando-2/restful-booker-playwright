using FluentAssertions;
using Microsoft.Playwright;
using Newtonsoft.Json;
using restful_booker_playwright.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restful_booker_playwright.Tests.Auth
{
    [TestFixture]
    public class AuthTests : PlaywrightTest
    {
        private IAPIRequestContext Request;

        [SetUp]
        public async Task SetUpAPITesting()
        {
            await CreateAPIRequestContext();
        }

        private async Task CreateAPIRequestContext()
        {
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");

            Request = await this.Playwright.APIRequest.NewContextAsync(new()
            {
                // All requests we send go to this API endpoint.
                BaseURL = "http://localhost:3001",
            });
        }

        [TearDown]
        public async Task TearDownAPITesting()
        {
            await Request.DisposeAsync();
        }

        [Test]
        public async Task CanCreateToken()
        {
            //Arrange
            var data = new
            {
                username = "admin",
                password = "password123"
            };

            //Act
            // Send the booking to the API.
            var response = await Request.PostAsync("/auth", new() { DataObject = data});

            // Check the response status code.
            response.Status.Should().Be(200);
            response.StatusText.Should().Be("OK");
            var responseBody = await response.JsonAsync();
            // convert the response body to a Token object
            var jsonString = responseBody.ToString();
            TokenObject tokenObject = JsonConvert.DeserializeObject<TokenObject>(jsonString);
            //Assert
            tokenObject.Token.Should().NotBeNullOrEmpty();
        }
    }
}

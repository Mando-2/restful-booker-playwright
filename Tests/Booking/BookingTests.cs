using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using NUnit.Framework;
using restful_booker_playwright.Models;


namespace restful_booker_playwright.Tests.Booking
{
    [TestFixture]
    public class BookingTests : PlaywrightTest
    {
        private IAPIRequestContext Request;
        private int BookingId;

        [SetUp]
        public async Task SetUpAPITesting()
        {
            await CreateAPIRequestContext();
        }

        [TearDown]
        public async Task TearDownAPITesting()
        {
            if (Request != null)
            {
                await Request.DisposeAsync();
            }
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

        [Test]
        public async Task CanCreateBooking()
        {
            // Create a new booking.
            var booking = new
            {
                firstname = "Jim",
                lastname = "Brown",
                totalprice = 111,
                depositpaid = true,
                bookingdates = new
                {
                    checkin = "2018-01-01",
                    checkout = "2019-01-01"
                },
                additionalneeds = "Breakfast"
            };

            // Send the booking to the API.
            var response = await Request.PostAsync("/booking", new() { DataObject = booking });

            // Check the response status code.
            response.Status.Should().Be(200);
            response.StatusText.Should().Be("OK");

            // Check the response body.
            var responseBody = await response.JsonAsync();
            var bookedBooking = JsonConvert.DeserializeObject<BookingResponse>(responseBody.ToString());

            bookedBooking.bookingid.Should().BeGreaterThan(0);
            bookedBooking.booking.firstname.Should().Be(booking.firstname);
            bookedBooking.booking.lastname.Should().Be(booking.lastname);
            bookedBooking.booking.totalprice.Should().Be(booking.totalprice);
            bookedBooking.booking.depositpaid.Should().Be(booking.depositpaid);
            bookedBooking.booking.bookingdates.checkin.Should().Be(booking.bookingdates.checkin);
            bookedBooking.booking.bookingdates.checkout.Should().Be(booking.bookingdates.checkout);
            bookedBooking.booking.additionalneeds.Should().Be(booking.additionalneeds);
        }

        [Test]
        public async Task CanDeleteBooking()
        {
            // reminder that this test will fail because because there is 
            // an intentional bug in the API that prevents the deletion of bookings
            // this is deliberate as the site is a demo site designed for testers to
            // test api's for existing bugs.  Yay we fond one!

            //Arrange
            #region Get Auth Token            
            string Token = await GetAuthToken();
            #endregion

            #region Create a new booking.
            int bookingid = await CreateBookingForTest();
            #endregion

            //Act
            #region Delete Booking
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Cookie", string.Format("token={0}", Token));            

            var request = await this.Playwright.APIRequest.NewContextAsync(new()
            {
                // All requests we send go to this API endpoint.
                BaseURL = "http://localhost:3001",
            });

            string path = string.Format("http://localhost:3001/booking/{0}", bookingid);
            var deleteResponse = await request.DeleteAsync(path);

            // Check the response status code.
            deleteResponse.Status.Should().Be(201);
            deleteResponse.StatusText.Should().Be("OK");
            #endregion

            //Assert
            #region Get BookingId's and make sure that the current BookingId isn't returned
            List<int> bookings = await GetBookingIds();

            #endregion

            bookings.Should().NotContain(bookingid, "this is deliberate as the site is a demo site designed for testers to test api's for existing bugs.  Yay we fond one!");

        }

        [Test]
        public async Task CanGetBookingIds()
        {
            //Assert
            #region Get BookingId's and make sure that the current BookingId isn't returned
            // get bookings
            var getResponse = await Request.GetAsync("/booking");

            // Check the response status code.
            getResponse.Status.Should().Be(200);
            getResponse.StatusText.Should().Be("OK");

            // Check the response body.
            var getResponseBody = await getResponse.JsonAsync();
            var bookings = JsonConvert.DeserializeObject<List<BookingResponse>>(getResponseBody.ToString());
            BookingId = bookings[0].bookingid;
            #endregion
        }

        [Test]
        public async Task CanGetBookingById()
        {
            var id = await CreateBookingForTest();
            var getResponse = await Request.GetAsync(string.Format("/booking/{0}", id));

            // Check the response status code.
            getResponse.Status.Should().Be(200);
            getResponse.StatusText.Should().Be("OK");

            // Check the response body.
            var getResponseBody = await getResponse.JsonAsync();
            var bookedBooking = JsonConvert.DeserializeObject<restful_booker_playwright.Models.Booking>(getResponseBody.ToString());

            bookedBooking.firstname.Should().BeOfType<string>();
            bookedBooking.lastname.Should().BeOfType<string>();
            bookedBooking.totalprice.Should().BeGreaterThan(0);
            bookedBooking.bookingdates.checkin.Should().BeOfType<string>();
            bookedBooking.bookingdates.checkout.Should().BeOfType<string>();
            bookedBooking.additionalneeds.Should().BeOfType<string>();
        }

        #region Support Methods
        private async Task<string> GetAuthToken()
        {
            var data = new
            {
                username = "admin",
                password = "password123"
            };

            //Act
            // Send the booking to the API.
            var response = await Request.PostAsync("/auth", new() { DataObject = data });

            // Check the response status code.
            response.Status.Should().Be(200);
            response.StatusText.Should().Be("OK");
            var responseBody = await response.JsonAsync();
            // convert the response body to a Token object
            var jsonString = responseBody.ToString();
            TokenObject tokenObject = JsonConvert.DeserializeObject<TokenObject>(jsonString);
            //Assert
            //tokenObject.Should().NotBeNullOrEmpty();
            return tokenObject.Token;
        }

        private async Task<int> CreateBookingForTest()
        {
            string _firstName = Faker.NameFaker.FirstName();
            string _lastName = Faker.NameFaker.LastName();
            int _totalPrice = Faker.NumberFaker.Number();
            Boolean _depositPaid = Faker.BooleanFaker.Boolean();
            string _checkInDate = Faker.DateTimeFaker.DateTime().ToShortDateString();
            string _checkOutDate = Faker.DateTimeFaker.DateTime().ToShortDateString();
            string _additionalNeeds = Faker.TextFaker.Sentence();

            // Create a new booking.
            var booking = new
            {
                firstname = _firstName,
                lastname = _lastName,
                totalprice = _totalPrice,
                depositpaid = _depositPaid,
                bookingdates = new
                {
                    checkin = _checkInDate,
                    checkout = _checkOutDate
                },
                additionalneeds = _additionalNeeds
            };

            // Send the booking to the API.
            var response = await Request.PostAsync("/booking", new() { DataObject = booking });

            // Check the response status code.
            response.Status.Should().Be(200);
            response.StatusText.Should().Be("OK");

            // Check the response body.
            var responseBody = await response.JsonAsync();
            var bookedBooking = JsonConvert.DeserializeObject<BookingResponse>(responseBody.ToString());
            return bookedBooking.bookingid;
        }
        
        private async Task<List<int>> GetBookingIds()
        {
            List<int> bookingIds = new List<int>();
            
            var getResponse = await Request.GetAsync("/booking");

            // Check the response status code.
            getResponse.Status.Should().Be(200);
            getResponse.StatusText.Should().Be("OK");

            // Check the response body.
            var getResponseBody = await getResponse.JsonAsync();
            var bookings = JsonConvert.DeserializeObject<List<BookingResponse>>(getResponseBody.ToString());

            foreach (BookingResponse b in bookings)
            {
                bookingIds.Add(b.bookingid);
            }   
            return bookingIds;
        }
        #endregion
    }
}

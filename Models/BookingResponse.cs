using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restful_booker_playwright.Models
{
    public class BookingResponse
    {
        public int bookingid { get; set; }
        public Booking booking { get; set; }
    }

}

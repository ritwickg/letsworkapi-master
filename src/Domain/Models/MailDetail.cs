using System;

namespace LetsWork.Domain.Models
{
    public class BookingEmailModel
    {
        public Guid BookingID { get; set; }
        public string CustomerName { get; set; }
        public string BookingFromDate { get; set; }
        public string BookingToDate { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public double Price { get; set; }
    }
}

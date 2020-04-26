using System;

namespace LetsWork.Domain.Models
{
    public class Booking
    {
        public Guid BookingID { get; set; }
        public DateTime BookingFromDate { get; set; }
        public DateTime BookingToDate { get; set; }
        public Guid UserID { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public double Price { get; set; }
        public Guid VenueID { get; set; }
        public VenueDetail VenueDetail { get; set; }
        public string BookingStatus { get; set; }
        public ReferralCodeTransaction ReferralCodeTransaction { get; set; }
    }
    public enum BookingStatus
    {
        ACTIVE = 0,
        CANCELLED,
        CLOSED
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class BookingDetailModelDTO
    {
        public Guid BookingID { get; set; }
        public string BookingFromDate { get; set; }
        public string BookingToDate { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class BookingModelDTO
    {
        [Required]
        public string BookedFrom { get; set; }

        [Required]
        public string BookedTo { get; set; }

        [Required]
        public string TimeZoneId { get; set; }

        [Required]
        public Guid VenueID { get; set; }
    
        public string UserID { get; set; }

        [Required]
        public string UserEmail { get; set; }

        public string ReferralCode { get; set; }
    }
}

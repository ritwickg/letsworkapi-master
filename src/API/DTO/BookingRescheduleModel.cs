using System;
using System.ComponentModel.DataAnnotations;

namespace LetsWork.API.DTO
{
    public class BookingRescheduleModelDTO
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public string BookedFrom { get; set; }

        [Required]
        public string BookedTo { get; set; }

        [Required]
        public string TimeZoneId { get; set; }
    }
}

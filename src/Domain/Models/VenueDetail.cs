using System;
using System.Collections.Generic;

namespace LetsWork.Domain.Models
{
    public class VenueDetail
    {
        public Guid VenueID { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public bool IsActive { get; set; } = true;
        public string ContactNumber { get; set; }
        public virtual InventoryDetail InventoryDetails { get; set; }
        public virtual ICollection<Booking> BookingDetails { get; set; }
        public virtual List<VenueImage> VenueImages { get; set; }
        public VenueDetail()
        {
            BookingDetails = new HashSet<Booking>();
        }
    }

    public class VenueImage
    {
        public Guid VenueImageID { get; set; }
        public string ContainerName { get; set; }
        public string ResourceName { get; set; }
        public string HostedImageURL { get; set; }
        public Guid VenueID { get; set; }
    }
}

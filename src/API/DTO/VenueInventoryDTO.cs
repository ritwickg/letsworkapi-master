using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class VenueInventoryDTO
    {
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public bool IsActive { get; set; }
        public string ContactNumber { get; set; }
        public int NumberOfProjectors { get; set; }
        public int SeatCapacity { get; set; }
        public int NumberOfMicroPhones { get; set; }
        public string Description { get; set; }
        public string RoomType { get; set; }
        public int NumberOfPhones { get; set; }
        public string WirelessNetworkType { get; set; }
        public double HourlyRate { get; set; }
        public string AirConditioningType { get; set; }
        public bool IsFoodVendingMachineAvailable { get; set; }
        public bool IsWaterVendingMachineAvailable { get; set; }
        public bool IsCoffeeVendingMachineAvailable { get; set; }
    }
}

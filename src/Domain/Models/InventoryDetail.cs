using System;

namespace LetsWork.Domain.Models
{
    public class InventoryDetail
    {
        public Guid InventoryID { get; set; }       
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
        public Guid VenueID { get; set; }
        public VenueDetail VenueDetails { get; set; }
    }
}

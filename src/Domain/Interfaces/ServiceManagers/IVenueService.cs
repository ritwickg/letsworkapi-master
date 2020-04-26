using LetsWork.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IVenueService
    {
        Task<List<VenueDetail>> GetVenueDetailsAsync();
        
        Task<bool> CheckVenueContactNumberAsync(string ContactNumber);
        Task<List<VenueInventoryViewModel>> GetVenueDetailsSearchAsync(string FromDate, string ToDate, string City);
        Task AddNewVenueAsync(VenueDetail NewVenueDetail);
        Task UpdateVenueDetailAsync(Guid VenueID, VenueDetail ReplacementVenueDetail);
        Task<VenueInventoryViewModel> GetVenueByIdAsync(Guid VenueID);
        Task DeleteVenueImageAsync(Guid VenueImageID);
        Task AddVenueImageAsync(List<VenueImage> VenueImages);

    }
}

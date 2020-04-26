using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.Services
{
    public class VenueService : IVenueService
    {
        public IAsyncRepository<VenueDetail> _venueInventoryRepository;
        public IAsyncRepository<VenueImage> _venueImageRepository;
        public IAsyncRepository<Booking> _bookingRespository;

        public readonly IBlobService _blobService;
        public VenueService(IAsyncRepository<VenueDetail> VenueInventoryRepository, 
                            IBlobService BlobService,
                            IAsyncRepository<Booking> BookingRepository,
                            IAsyncRepository<VenueImage> VenueImageRepository)
        {
            this._venueInventoryRepository = VenueInventoryRepository;
            this._blobService = BlobService;
            this._venueImageRepository = VenueImageRepository;
            this._bookingRespository = BookingRepository;
        }

     
       
        public async Task<List<VenueDetail>> GetVenueDetailsAsync()
        {
            try
            {
                List<VenueDetail> venueDetailList = await _venueInventoryRepository.ListAllAsync(); 

                

                return venueDetailList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> CheckVenueContactNumberAsync(string ContactNumber)
        {
            VenueDetail venuDetail = await  _venueInventoryRepository.GetSingleBySpecAsync(p => p.ContactNumber == ContactNumber);
            return venuDetail == null;
        }

        public async Task<List<VenueInventoryViewModel>> GetVenueDetailsSearchAsync(string FromDate, string ToDate, string City)
        {
            try
            {
                DateTime bookedFromDate = DateTime.Parse(FromDate);
                DateTime bookedToDate = DateTime.Parse(ToDate);

                //Venues with same city
                List<VenueDetail> venueWithSameCityList = await _venueInventoryRepository
                                                          .ListAsync(x => x.VenueCity.ToLower().Equals(City.ToLower()));

                //Venue ids which are in that city
                List<Guid> venueIdList = venueWithSameCityList.Select(x => x.VenueID)
                                                       .ToList<Guid>();
                
                //Bookings conflicting with the dates in that city
                List<Booking> venueBookingsList = await _bookingRespository
                                                        .ListAsync(x => 
                                                        bookedFromDate <= x.BookingToDate 
                                                        && bookedToDate >= x.BookingFromDate 
                                                        && venueIdList.Contains(x.VenueID));

                //Venue ids which are already booked
                List<Guid> bookedVenueId = venueBookingsList.Select(x => x.VenueID)
                                                         .ToList<Guid>();

                IEnumerable<Guid> selectedVenueIds = venueIdList.Except<Guid>(bookedVenueId);

                List<VenueDetail> resultantVenues = await _venueInventoryRepository.ListAsync(x => selectedVenueIds
                                                                              .Contains(x.VenueID));

                List<VenueInventoryViewModel> searchedVenueResult = resultantVenues.Select(x => new VenueInventoryViewModel
                                                                    {
                                                                        VenueID = x.VenueID,
                                                                        VenueName = x.VenueName,
                                                                        AirConditioningType = x.InventoryDetails.AirConditioningType,
                                                                        VenueState = x.VenueState,
                                                                        VenueCity = x.VenueCity,
                                                                        Description = x.InventoryDetails.Description,
                                                                        IsCoffeeVendingMachineAvailable = x.InventoryDetails.IsCoffeeVendingMachineAvailable,
                                                                        IsWaterVendingMachineAvailable = x.InventoryDetails.IsWaterVendingMachineAvailable,
                                                                        WirelessNetworkType = x.InventoryDetails.WirelessNetworkType,
                                                                        HourlyRate = x.InventoryDetails.HourlyRate,
                                                                        IsActive = x.IsActive,
                                                                        IsFoodVendingMachineAvailable = x.InventoryDetails.IsFoodVendingMachineAvailable,
                                                                        NumberOfMicroPhones = x.InventoryDetails.NumberOfMicroPhones,
                                                                        NumberOfPhones = x.InventoryDetails.NumberOfPhones,
                                                                        NumberOfProjectors = x.InventoryDetails.NumberOfProjectors,
                                                                        RoomType = x.InventoryDetails.RoomType,
                                                                        SeatCapacity = x.InventoryDetails.SeatCapacity
                                                                    }).ToList();
                return searchedVenueResult;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task AddNewVenueAsync(VenueDetail NewVenueDetail)
        {
            await _venueInventoryRepository.AddAsync(NewVenueDetail);
        }
        public async Task UpdateVenueDetailAsync(Guid VenueID, VenueDetail ReplacementVenueDetail)
        {
            try
            {

                VenueDetail venueDetailTobeUpdated = await _venueInventoryRepository.GetByIdAsync(VenueID);
                Guard.Against.NullItem<VenueDetail>(venueDetailTobeUpdated);
 
                //==============================Setting all new and updated properties=====================================================
                venueDetailTobeUpdated.VenueName = ReplacementVenueDetail.VenueName;
                venueDetailTobeUpdated.VenueCity = ReplacementVenueDetail.VenueCity;
                venueDetailTobeUpdated.VenueState = ReplacementVenueDetail.VenueState;
                venueDetailTobeUpdated.InventoryDetails.AirConditioningType = ReplacementVenueDetail.InventoryDetails.AirConditioningType;
                venueDetailTobeUpdated.InventoryDetails.Description = ReplacementVenueDetail.InventoryDetails.Description;
                venueDetailTobeUpdated.InventoryDetails.HourlyRate = ReplacementVenueDetail.InventoryDetails.HourlyRate;
                venueDetailTobeUpdated.InventoryDetails.IsCoffeeVendingMachineAvailable = ReplacementVenueDetail.InventoryDetails.IsCoffeeVendingMachineAvailable;
                venueDetailTobeUpdated.InventoryDetails.IsFoodVendingMachineAvailable = ReplacementVenueDetail.InventoryDetails.IsFoodVendingMachineAvailable;
                venueDetailTobeUpdated.InventoryDetails.IsWaterVendingMachineAvailable = ReplacementVenueDetail.InventoryDetails.IsWaterVendingMachineAvailable;
                venueDetailTobeUpdated.InventoryDetails.NumberOfMicroPhones = ReplacementVenueDetail.InventoryDetails.NumberOfMicroPhones;
                venueDetailTobeUpdated.InventoryDetails.NumberOfPhones = ReplacementVenueDetail.InventoryDetails.NumberOfPhones;
                venueDetailTobeUpdated.InventoryDetails.RoomType = ReplacementVenueDetail.InventoryDetails.RoomType;
                venueDetailTobeUpdated.InventoryDetails.SeatCapacity = ReplacementVenueDetail.InventoryDetails.SeatCapacity;
                venueDetailTobeUpdated.InventoryDetails.NumberOfProjectors = ReplacementVenueDetail.InventoryDetails.NumberOfProjectors;
                venueDetailTobeUpdated.InventoryDetails.WirelessNetworkType = ReplacementVenueDetail.InventoryDetails.WirelessNetworkType;

                await _venueInventoryRepository.UpdateAsync(venueDetailTobeUpdated); 
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VenueInventoryViewModel> GetVenueByIdAsync(Guid VenueID)
        {
            try
            {
                VenueDetail selectedVenue = await _venueInventoryRepository.GetByIdAsync(VenueID); 
                VenueInventoryViewModel selectedVenueDetails = new VenueInventoryViewModel
                                                         {
                                                             VenueID = selectedVenue.VenueID,
                                                             VenueName = selectedVenue.VenueName,
                                                             AirConditioningType = selectedVenue.InventoryDetails.AirConditioningType,
                                                             VenueState = selectedVenue.VenueState,
                                                             VenueCity = selectedVenue.VenueCity,
                                                             ContactNumber = selectedVenue.ContactNumber,
                                                             Description = selectedVenue.InventoryDetails.Description,
                                                             IsCoffeeVendingMachineAvailable = selectedVenue.InventoryDetails.IsCoffeeVendingMachineAvailable,
                                                             IsWaterVendingMachineAvailable = selectedVenue.InventoryDetails.IsWaterVendingMachineAvailable,
                                                             WirelessNetworkType = selectedVenue.InventoryDetails.WirelessNetworkType,
                                                             HourlyRate = selectedVenue.InventoryDetails.HourlyRate,
                                                             IsActive = selectedVenue.IsActive,
                                                             IsFoodVendingMachineAvailable = selectedVenue.InventoryDetails.IsFoodVendingMachineAvailable,
                                                             NumberOfMicroPhones = selectedVenue.InventoryDetails.NumberOfMicroPhones,
                                                             NumberOfPhones = selectedVenue.InventoryDetails.NumberOfPhones,
                                                             NumberOfProjectors = selectedVenue.InventoryDetails.NumberOfProjectors,
                                                             RoomType = selectedVenue.InventoryDetails.RoomType,
                                                             SeatCapacity = selectedVenue.InventoryDetails.SeatCapacity,
                                                             VenueImages = selectedVenue.VenueImages
                                                         };


                Guard.Against.NullItem<VenueDetail>(selectedVenue);   
                return selectedVenueDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteVenueImageAsync(Guid VenueImageID)
        {
            try
            {
                VenueImage image = await _venueImageRepository.GetByIdAsync(VenueImageID);
                Guard.Against.NullItem<VenueImage>(image);
                await _blobService.DeleteFileFromBlobAsync(image.ContainerName, image.ResourceName);
                await _venueImageRepository.DeleteAsync(image);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task AddVenueImageAsync(List<VenueImage> VenueImages)
        {
            foreach (VenueImage venuImage in VenueImages)
                await _venueImageRepository.AddAsync(venuImage); 
        }
    }
}
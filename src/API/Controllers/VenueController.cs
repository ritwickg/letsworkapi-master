using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly IVenueService _venueService;
        private readonly IFileService _fileService;
        public VenueController(IVenueService VenueService,
                               IFileService FileService)
        {
            _venueService = VenueService;
            _fileService = FileService;
        }

        #region Venue repository actions

        /// <summary>
        /// Provides all the details of all the venues
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllVenueDetailsAsync()
        {
            List<VenueDetail> venueDetails = await _venueService.GetVenueDetailsAsync();
            List<VenueInventoryViewModel> venueInventoryList = venueDetails
                                                        .Select(x => new VenueInventoryViewModel
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
                                                            SeatCapacity = x.InventoryDetails.SeatCapacity,
                                                            VenueImages = x.VenueImages
                                                        }).ToList();
            return Ok(venueInventoryList);
        }
        
        /// <summary>
        /// Provides the details of venue based on the id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetVenueByIdAsync(Guid id)
        {
            VenueInventoryViewModel selectedVenue = await _venueService.GetVenueByIdAsync(id);

            if (selectedVenue == null)
                return NotFound(new { message = "No such venue found"});

            return Ok(selectedVenue);
        }

        /// <summary>
        /// Adds a new venue 
        /// </summary>
        /// <param name="VenueInventoryDTO">VenueInventoryDTO</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(201)]
        public async Task<IActionResult> AddVenueDetailsAsync([FromBody] VenueInventoryDTO VenueInventoryDTO)
        {
            Guard.Against.NullItem<VenueInventoryDTO>(VenueInventoryDTO);

            //===============Initialize Objects==================================================
            Guid venueID = Guid.NewGuid();
            Guid inventoryID = Guid.NewGuid();

            VenueDetail venueDetail = new VenueDetail
            {
                VenueID = venueID,
                VenueName = VenueInventoryDTO.VenueName,
                VenueCity = VenueInventoryDTO.VenueCity,
                VenueState = VenueInventoryDTO.VenueState,
                ContactNumber = VenueInventoryDTO.ContactNumber,
                IsActive = true,
                InventoryDetails = new InventoryDetail
                {
                    VenueID = venueID,
                    InventoryID = inventoryID,
                    AirConditioningType = VenueInventoryDTO.AirConditioningType,
                    Description = VenueInventoryDTO.Description,
                    HourlyRate = VenueInventoryDTO.HourlyRate,
                    IsCoffeeVendingMachineAvailable = VenueInventoryDTO.IsCoffeeVendingMachineAvailable,
                    IsFoodVendingMachineAvailable = VenueInventoryDTO.IsFoodVendingMachineAvailable,
                    IsWaterVendingMachineAvailable = VenueInventoryDTO.IsWaterVendingMachineAvailable,
                    NumberOfMicroPhones = VenueInventoryDTO.NumberOfPhones,
                    NumberOfPhones = VenueInventoryDTO.NumberOfPhones,
                    NumberOfProjectors = VenueInventoryDTO.NumberOfProjectors,
                    RoomType = VenueInventoryDTO.RoomType,
                    SeatCapacity = VenueInventoryDTO.SeatCapacity,
                    WirelessNetworkType = VenueInventoryDTO.WirelessNetworkType
                }
            };

            //====================Insert now into the database=======================
            await _venueService.AddNewVenueAsync(venueDetail);
            return StatusCode(201, new { message = $"Venue {venueDetail.VenueName} added successfully on {DateTime.Now.ToString("o")}." });
        }

        /// <summary>
        /// Updates a venue based on the venue id with the updated details
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="ReplacementVenueDetail">ReplacementVenueDetail</param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateVenueDetailsAsync(Guid id, [FromBody] VenueInventoryDTO ReplacementVenueDetail)
        {
            Guard.Against.NullItem<VenueInventoryDTO>(ReplacementVenueDetail);

            //===============Initialize Objects==================================================
            VenueDetail UpdatedVenueDetail = new VenueDetail
            {
                VenueName = ReplacementVenueDetail.VenueName,
                VenueState = ReplacementVenueDetail.VenueState,
                VenueCity = ReplacementVenueDetail.VenueCity,
                ContactNumber = ReplacementVenueDetail.ContactNumber,
                InventoryDetails = new InventoryDetail
                {
                    AirConditioningType = ReplacementVenueDetail.AirConditioningType,
                    Description = ReplacementVenueDetail.Description,
                    HourlyRate = ReplacementVenueDetail.HourlyRate,
                    IsCoffeeVendingMachineAvailable = ReplacementVenueDetail.IsCoffeeVendingMachineAvailable,
                    IsFoodVendingMachineAvailable = ReplacementVenueDetail.IsFoodVendingMachineAvailable,
                    IsWaterVendingMachineAvailable = ReplacementVenueDetail.IsWaterVendingMachineAvailable,
                    NumberOfMicroPhones = ReplacementVenueDetail.NumberOfMicroPhones,
                    NumberOfPhones = ReplacementVenueDetail.NumberOfPhones,
                    NumberOfProjectors = ReplacementVenueDetail.NumberOfProjectors,
                    RoomType = ReplacementVenueDetail.RoomType,
                    SeatCapacity = ReplacementVenueDetail.SeatCapacity,
                    WirelessNetworkType = ReplacementVenueDetail.WirelessNetworkType,
                }
            };

            //====================Update now into the database=======================
            await _venueService.UpdateVenueDetailAsync(id, UpdatedVenueDetail);
            return Ok(new { message = $"Venue {id} updated successfully" });
        }
        #endregion

        #region Miscellaneous venue operations

        /// <summary>
        /// Checks if the contact number provided for the check is already registered
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("duplicate-venue-contact-number/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DuplicateContactNumberCheckAsync(string id)
        {
            Guard.Against.NullString(id);
            
            bool validStatus = await _venueService.CheckVenueContactNumberAsync(id);

            if (validStatus)
                return Ok(new { message = "true" });

            return Ok(new { message = "false" });
        }

        /// <summary>
        /// Searches over the venues based on the date and city provided
        /// </summary>
        /// <param name="SearchDataDTO">SearchDataDTO</param>
        /// <returns></returns>
        [HttpPost("search")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SearchVenueAsync([FromBody] SearchDTO SearchDataDTO)
        {
            Guard.Against.NullItem<SearchDTO>(SearchDataDTO);

            List<VenueInventoryViewModel> searchResult = await _venueService.GetVenueDetailsSearchAsync(SearchDataDTO.FromDate, SearchDataDTO.ToDate, SearchDataDTO.City);
            return Ok(searchResult);
        }

        /// <summary>
        /// Uploads the image for a venue based on the venue id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpPut("venue-image/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddVenueImagesAsync(Guid id)
        {
            IFormFileCollection fileCollection = Request.Form.Files;

            if (fileCollection.Count == 0)
                return BadRequest(new { message = $"Please add at least 1 image and less than 5 images for venue to continue. " });

            if(fileCollection.Count > 5)
                return BadRequest(new { message = $"Cannot upload more than 5 files, please reupload to continue. " });

            foreach (IFormFile uploadedFile in fileCollection)
                if (!new string[] { "image/jpeg", "image/jpg", "image/png" }.Contains(uploadedFile.ContentType))
                    return BadRequest(new { message = "Invalid format, Uploaded file is not an image." });

            List<(string HostedURL, string ResourceName)> hostedImageURLList =  await _fileService.UploadFileToBlob(fileCollection, id);
            List<VenueImage> venueImages = hostedImageURLList.Select(x => new VenueImage
            {
                VenueImageID = Guid.NewGuid(),
                ContainerName = id.ToString(),
                HostedImageURL = x.HostedURL,
                ResourceName = x.ResourceName,
                VenueID = id
            }).ToList();

            await _venueService.AddVenueImageAsync(venueImages);
            return StatusCode(201, new { message = "Venue Images added successsfully." });
        }

        /// <summary>
        /// Removes a venue image based on the venue id provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("venue-image/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteVenueImageAsync(Guid id)
        {
            await _venueService.DeleteVenueImageAsync(id);
            return NoContent();
        }
        #endregion        
    }
}
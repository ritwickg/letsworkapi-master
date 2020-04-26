using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingManager;
        private readonly IVenueService _venueService;
        private readonly IEmailService _emailManager;

        public BookingController(IBookingService BookingManager, IEmailService EmailManager, IVenueService VenueService)
        {
            this._bookingManager = BookingManager;
            this._emailManager = EmailManager;
            this._venueService = VenueService;
        }

        #region Booking Management

        /// <summary>
        /// Provides all the bookings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllBookingsAsync()
        {
            await _bookingManager.UpdateBookingStatusAsync();

            List<Booking> bookingList = await _bookingManager.AllBookingDetailsAsync();
            List<BookingDetailModelDTO> bookings = bookingList.Select(x => new BookingDetailModelDTO
            {
                BookingFromDate = DateTime.SpecifyKind(x.BookingFromDate, DateTimeKind.Utc).ToString("o"),
                BookingID = x.BookingID,
                BookingToDate = DateTime.SpecifyKind(x.BookingToDate, DateTimeKind.Utc).ToString("o"),
                Email = x.ApplicationUser.Email,
                UserName = x.ApplicationUser.UserName,
                VenueCity = x.VenueDetail.VenueCity,
                VenueName = x.VenueDetail.VenueName
            }).ToList();

            return Ok(bookings);
        }

        /// <summary>
        /// Add a booking on the basis of user request
        /// </summary>
        /// <param name="BookingModelDTO">BookingModelDTO</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddBookingAsync([FromBody] BookingModelDTO BookingModelDTO)
        {
            Guard.Against.Null(BookingModelDTO, nameof(BookingModelDTO));

            bool refCodeDiscount = false;

            if (!string.IsNullOrEmpty(BookingModelDTO.ReferralCode))
            {
                (bool HasReferralCodeTransactionCountExceeded, bool IsDuplicateTransaction, bool IsReferralCodeSelfUsed) = await _bookingManager.CheckReferralCodeAsync(BookingModelDTO.ReferralCode, Guid.Parse(BookingModelDTO.UserID));

                if (HasReferralCodeTransactionCountExceeded)
                    return BadRequest(new { message = "Referral Code provided cannot be used, as it has been used 3 times." });
                if (IsDuplicateTransaction)
                    return BadRequest(new { message = "Referral Code shared to you cannot be used more than once, please apply with a fresh one." });
                if (IsReferralCodeSelfUsed)
                    return BadRequest(new { message = "Referral Code provider and the beneficiary cannot be same!" });

                refCodeDiscount = true;
            }

            DateTime bookingFromDate = DateTime.Parse(BookingModelDTO.BookedFrom, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            DateTime bookingToDate = DateTime.Parse(BookingModelDTO.BookedTo, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            bool availabilityOfVenue = await _bookingManager.CheckBookingAvailabilityAsync(bookingFromDate, bookingToDate, BookingModelDTO.VenueID);

            if (!availabilityOfVenue)
                return BadRequest(new { message = "Sorry, try to book for some other slot, the current selected slot is already booked by someone else." });

            Guid bookingId = Guid.NewGuid();
            TimeSpan bookingTimeSpan = (bookingToDate - bookingFromDate);

            VenueInventoryViewModel selectedVenueDetail = await _venueService.GetVenueByIdAsync(BookingModelDTO.VenueID);

            double calculatedPrice = bookingTimeSpan.TotalHours * selectedVenueDetail.HourlyRate;

            if (refCodeDiscount)
            {
                calculatedPrice = calculatedPrice -( 0.1 * calculatedPrice);
            }
            //Entity Model
            Booking newBooking = new Booking
            {
                BookingID = bookingId,
                VenueID = BookingModelDTO.VenueID,
                BookingFromDate = bookingFromDate,
                BookingToDate = bookingToDate,
                UserID = Guid.Parse(BookingModelDTO.UserID),
                Price = calculatedPrice,
                BookingStatus = BookingStatus.ACTIVE.ToString()
            };

            await _bookingManager.AddNewBookingAsync(newBooking, BookingModelDTO.ReferralCode, BookingModelDTO.TimeZoneId);
            return StatusCode(201, new { message = $"Successfully booked the venue {selectedVenueDetail.VenueName}." });
        }

        /// <summary>
        /// Cancels a booking based on the Booking Id that is passed
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpPut("cancel-booking/{id}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> CancelBookingsAsync(Guid id)
        {
            await _bookingManager.CancelUserBookingDetailsAsync(id);
            return Ok(new { message = "Booking cancelled successfully!!"});
        }
        #endregion

        /// <summary>
        /// Provides the bookings on basis of the time period that is provided
        /// </summary>
        /// <param name="VenueId">VenueId</param>
        /// <param name="SearchModel">SearchModel</param>
        /// <returns></returns>
        [HttpPost("get-periodic-bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetPeriodicBookingsAsync([FromQuery] Guid VenueId, [FromBody] BookingSearchModelDTO SearchModel)
        {
            Guard.Against.NullItem<BookingSearchModelDTO>(SearchModel);
            DateTime endDate = DateTime.Parse(SearchModel.SearchDateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            List<Booking> searchedBooking = await _bookingManager.BookingByPeriodCriteriaAsync(endDate, SearchModel.SearchLogic);
            List<BookingDetailModelDTO> bookings = searchedBooking.Select(x => new BookingDetailModelDTO
            {
                BookingFromDate = DateTime.SpecifyKind(x.BookingFromDate, DateTimeKind.Utc).ToString("o"),
                BookingID = x.BookingID,
                BookingToDate = DateTime.SpecifyKind(x.BookingToDate, DateTimeKind.Utc).ToString("o"),
                Email = x.ApplicationUser.Email,
                UserName = x.ApplicationUser.UserName,
                VenueCity = x.VenueDetail.VenueCity,
                VenueName = x.VenueDetail.VenueName
            }).ToList();

            return Ok(bookings);
        }

        /// <summary>
        /// Provides all the active bookings
        /// </summary>
        /// <param name="ActiveBookingModel">ActiveBookingModel</param>
        /// <returns></returns>
        [HttpPost("get-active-bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetActiveBookingsAsync([FromBody] BookingSearchUserModelDTO ActiveBookingModel)
        {
            await _bookingManager.UpdateBookingStatusAsync();

            Guard.Against.NullItem<BookingSearchUserModelDTO>(ActiveBookingModel);
            DateTime currentDate = DateTime.Parse(ActiveBookingModel.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            List<Booking> activeBookingList = await _bookingManager.ActiveBookingsAsync(currentDate, ActiveBookingModel.UserId);

            
            List<BookingDetailModelDTO> bookings = activeBookingList.Select(x => new BookingDetailModelDTO
            {
                BookingFromDate = DateTime.SpecifyKind(x.BookingFromDate, DateTimeKind.Utc).ToString("o"),
                BookingID = x.BookingID,
                BookingToDate = DateTime.SpecifyKind(x.BookingToDate, DateTimeKind.Utc).ToString("o"),
                Email = x.ApplicationUser.Email,
                UserName = x.ApplicationUser.UserName,
                VenueCity = x.VenueDetail.VenueCity,
                VenueName = x.VenueDetail.VenueName
            }).ToList();

            return Ok(bookings);
        }

        /// <summary>
        /// Provides all the closed booking
        /// </summary>
        /// <param name="ClosedBookingModel"></param>
        /// <returns></returns>
        [HttpPost("get-closed-bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetClosedBookingsAsync(BookingSearchUserModelDTO ClosedBookingModel)
        {
            await _bookingManager.UpdateBookingStatusAsync();

            Guard.Against.NullItem<BookingSearchUserModelDTO>(ClosedBookingModel);
            DateTime currentDate = DateTime.Parse(ClosedBookingModel.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            List<Booking> activeBookingList = await _bookingManager.ClosedBookingsAsync(currentDate, ClosedBookingModel.UserId);

            List<BookingDetailModelDTO> bookings = activeBookingList.Select(x => new BookingDetailModelDTO
            {
                BookingFromDate = DateTime.SpecifyKind(x.BookingFromDate, DateTimeKind.Utc).ToString("o"),
                BookingID = x.BookingID,
                BookingToDate = DateTime.SpecifyKind(x.BookingToDate, DateTimeKind.Utc).ToString("o"),
                Email = x.ApplicationUser.Email,
                UserName = x.ApplicationUser.UserName,
                VenueCity = x.VenueDetail.VenueCity,
                VenueName = x.VenueDetail.VenueName
            }).ToList();

            return Ok(bookings);
        }

        /// <summary>
        /// Reschedule a booking based on the provided booking id and the dates
        /// </summary>
        /// <param name="RescheduleBookingModel">RescheduleBookingModel</param>
        /// <returns></returns>
        [HttpPost("reschedule-booking")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RescheduleBooking([FromBody] BookingRescheduleModelDTO RescheduleBookingModel)
        {
            Booking selectedBooking = await _bookingManager.GetBookingIdAsync(RescheduleBookingModel.BookingId);

            Guard.Against.NullItem<Booking>(selectedBooking);

            DateTime bookingFromDate = DateTime.Parse(RescheduleBookingModel.BookedFrom, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            DateTime bookingToDate = DateTime.Parse(RescheduleBookingModel.BookedTo, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            bool availabilityOfVenue = await _bookingManager.CheckBookingAvailabilityAsync(bookingFromDate, bookingToDate, selectedBooking.VenueID);

            if (!availabilityOfVenue)
                return BadRequest(new { message = "Sorry, try to book for some other slot, the current selected slot is already booked by someone else." });

            //Calculate new timespan now
            TimeSpan bookingTimeSpan = (bookingToDate - bookingFromDate);
            
            selectedBooking.BookingFromDate = bookingFromDate;
            selectedBooking.BookingToDate = bookingToDate;
            selectedBooking.Price = bookingTimeSpan.TotalHours * selectedBooking.VenueDetail.InventoryDetails.HourlyRate;
            selectedBooking.BookingStatus = BookingStatus.ACTIVE.ToString();

            await _bookingManager.UpdateBookingAsync(selectedBooking, RescheduleBookingModel.TimeZoneId);
            return Ok(new { message = $"Successfully rescheduled the booked the venue {selectedBooking.VenueDetail.VenueName}." });
        }
    }
}
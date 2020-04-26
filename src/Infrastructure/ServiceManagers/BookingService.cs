using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class BookingService : IBookingService
    {
        private readonly IAsyncRepository<Booking> _bookingRepository;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IAsyncRepository<ReferralCode> _referralCodeRepository;
        private readonly IAsyncRepository<ReferralCodeTransaction> _referralCodeTransactionRepository;
        public BookingService(IAsyncRepository<Booking> BookingRepository, 
                              IAsyncRepository<ReferralCode> ReferralCodeRepository,
                              IAsyncRepository<ReferralCodeTransaction> ReferralTransactionRepository,
                              IEmailService EmailService, 
                              IHostingEnvironment HostingEnvironment)
        {
            this._bookingRepository = BookingRepository;
            this._emailService = EmailService;
            this._hostingEnvironment = HostingEnvironment;
            this._referralCodeRepository = ReferralCodeRepository;
            this._referralCodeTransactionRepository = ReferralTransactionRepository;
        }
        public async Task AddNewBookingAsync(Booking NewBooking, string ReferralCode, string BookingTimeZoneId)
        {

            await _bookingRepository.AddAsync(NewBooking);
            string subject = "Booking Confirmation";
            if (!string.IsNullOrEmpty(ReferralCode))
            {
                ReferralCode referralCodeDetails = await _referralCodeRepository.GetSingleBySpecAsync(x => x.RefCode.ToLower() == ReferralCode.ToLower());

                ReferralCodeTransaction newTransaction = new ReferralCodeTransaction
                {
                    ReferralTransactionId = Guid.NewGuid(),
                    ReferralCodeId = referralCodeDetails.ReferralCodeId,
                    IssuerId = referralCodeDetails.UserId,
                    BenificiaryId = NewBooking.UserID,
                    BookingId = NewBooking.BookingID
                };

                await _referralCodeTransactionRepository.AddAsync(newTransaction);
                referralCodeDetails.ReferralCodeTransactionCount = referralCodeDetails.ReferralCodeTransactionCount + 1;
                await _referralCodeRepository.UpdateAsync(referralCodeDetails);


            }

            Booking currentBooking = await _bookingRepository.GetByIdAsync(NewBooking.BookingID);
            
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailMarkup.html");
            string customerName = $"{currentBooking.ApplicationUser.FirstName} {currentBooking.ApplicationUser.LastName}";
            string bookingID = currentBooking.BookingID.ToString();
            string bookedFrom = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentBooking.BookingFromDate, BookingTimeZoneId).ToString();
            string bookedTo = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentBooking.BookingToDate, BookingTimeZoneId).ToString();
            string venueName = currentBooking.VenueDetail.VenueName;
            string venueCity = currentBooking.VenueDetail.VenueCity;
            string venueState = currentBooking.VenueDetail.VenueState;
            string price = currentBooking.Price.ToString();


            string emailMarkupBody = await File.ReadAllTextAsync(path);
            emailMarkupBody = emailMarkupBody.Replace("{CustomerName}", customerName);
            emailMarkupBody = emailMarkupBody.Replace("{BookingID}", bookingID);
            emailMarkupBody = emailMarkupBody.Replace("{BookedFrom}", bookedFrom);
            emailMarkupBody = emailMarkupBody.Replace("{BookedTo}", bookedTo);
            emailMarkupBody = emailMarkupBody.Replace("{VenueName}", venueName);
            emailMarkupBody = emailMarkupBody.Replace("{VenueCity}", venueCity);
            emailMarkupBody = emailMarkupBody.Replace("{VenueState}", venueState);
            emailMarkupBody = emailMarkupBody.Replace("{Price}", price);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = currentBooking.ApplicationUser.Email,
                Body = emailMarkupBody,
                
            };

            await _emailService.SendEmailAsync(emailModel);
        }

        public async Task<bool> CheckBookingAvailabilityAsync(DateTime FromDate, DateTime ToDate, Guid VenueID)
        {
            List<Booking> bookings = await _bookingRepository.ListAsync(x => x.BookingFromDate <= FromDate && x.BookingToDate >= ToDate && x.VenueID == VenueID);
            return bookings.Count > 0 ? false : true;
        }

        public async Task<List<Booking>> AllBookingDetailsAsync()
        {
            List<Booking> bookingList = await _bookingRepository.ListAllAsync();
            return bookingList;
        }

        public async Task<List<Booking>> BookingByPeriodCriteriaAsync(DateTime SearchDateTime, string SearchCriteria)
        {
            List<Booking> bookingsList = new List<Booking>();
            switch (SearchCriteria)
            {
                case "1":
                    bookingsList = await _bookingRepository.ListAsync(x => x.BookingFromDate < SearchDateTime.AddDays(7));
                    break;
                case "2":
                    bookingsList = await _bookingRepository.ListAsync(x => x.BookingFromDate < SearchDateTime.AddMonths(1));
                    break;
                case "3":
                    bookingsList = await _bookingRepository.ListAsync(x => x.BookingFromDate < SearchDateTime.AddYears(1));
                    break;
            }
            return bookingsList;
        }

        public async Task<List<Booking>> ActiveBookingsAsync(DateTime Date, Guid Id)
        {
            List<Booking> activeBookingsList = await _bookingRepository.ListAsync(x => x.BookingFromDate > Date && x.BookingStatus == BookingStatus.ACTIVE.ToString() && x.UserID == Id);
            return activeBookingsList;
        }
        public async Task<List<Booking>> ClosedBookingsAsync(DateTime Date, Guid Id)
        {
            List<Booking> activeBookingsList = await _bookingRepository.ListAsync(x => (x.BookingFromDate < Date || 
                                                                                  x.BookingStatus == BookingStatus.CLOSED.ToString() ||
                                                                                  x.BookingStatus == BookingStatus.CANCELLED.ToString())
                                                                                  && x.UserID == Id);
            return activeBookingsList;
        }

        public async Task CancelUserBookingDetailsAsync(Guid id)
        {
            Booking selectedBooking = await _bookingRepository.GetByIdAsync(id);
            Guard.Against.NullItem<Booking>(selectedBooking);

            selectedBooking.BookingStatus = BookingStatus.CANCELLED.ToString();
            await _bookingRepository.UpdateAsync(selectedBooking);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = selectedBooking.ApplicationUser.Email,
                Body = $"Booking with {selectedBooking.BookingID} is successfully cancelled",
                Subject = "Cancellation Confirmation"
            };
            await _emailService.SendEmailAsync(emailModel);



        }

        public async Task<(bool HasReferralCodeTransactionCountExceeded, bool IsDuplicateTransaction, bool IsReferralCodeSelfUsed)> CheckReferralCodeAsync(string ReferralCode, Guid BenificiaryUserId)
        {
            ReferralCode referralCodeDetails = await _referralCodeRepository.GetSingleBySpecAsync(x => x.RefCode.ToLower() == ReferralCode.ToLower());

            Guard.Against.NullItem<ReferralCode>(referralCodeDetails);

            int duplicateTransactionCount = referralCodeDetails.ReferralCodeTransactions.Count(x => x.BenificiaryId == BenificiaryUserId);

            (bool HasReferralCodeTransactionCountExceeded, bool IsDuplicateTransaction, bool IsReferralCodeSelfUsed) referralCodeStatusTuple = (false, false, false);

            if (referralCodeDetails.ReferralCodeTransactionCount >= 3)
                referralCodeStatusTuple.HasReferralCodeTransactionCountExceeded = true;

            if (duplicateTransactionCount > 0)
                referralCodeStatusTuple.IsDuplicateTransaction = true;

            if (referralCodeDetails.UserId == BenificiaryUserId)
                referralCodeStatusTuple.IsReferralCodeSelfUsed = true;

            return referralCodeStatusTuple;
        }

        public async Task UpdateBookingStatusAsync()
        {
            List<Booking> bookingsList = await _bookingRepository.ListAllAsync();
            List<Booking> closedBookingList = bookingsList.Where(x => x.BookingFromDate < DateTime.UtcNow).ToList();
            foreach(Booking booking in closedBookingList)
            {
                booking.BookingStatus = BookingStatus.CLOSED.ToString();
                await _bookingRepository.UpdateAsync(booking);
            }
        }
        public async Task UpdateBookingAsync(Booking UpdatedBooking, string BookingTimeZoneId)
        {
            await _bookingRepository.UpdateAsync(UpdatedBooking);

            Booking currentBooking = await _bookingRepository.GetByIdAsync(UpdatedBooking.BookingID);

            string path = Path.Combine(_hostingEnvironment.WebRootPath, "EmailMarkup.html");
            string customerName = $"{currentBooking.ApplicationUser.FirstName} {currentBooking.ApplicationUser.LastName}";
            string bookingID = currentBooking.BookingID.ToString();
            string bookedFrom = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentBooking.BookingFromDate, BookingTimeZoneId).ToString();
            string bookedTo = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentBooking.BookingToDate, BookingTimeZoneId).ToString();
            string venueName = currentBooking.VenueDetail.VenueName;
            string venueCity = currentBooking.VenueDetail.VenueCity;
            string venueState = currentBooking.VenueDetail.VenueState;
            string price = currentBooking.Price.ToString();


            string emailMarkupBody = await File.ReadAllTextAsync(path);
            emailMarkupBody = emailMarkupBody.Replace("{CustomerName}", customerName);
            emailMarkupBody = emailMarkupBody.Replace("{BookingID}", bookingID);
            emailMarkupBody = emailMarkupBody.Replace("{BookedFrom}", bookedFrom);
            emailMarkupBody = emailMarkupBody.Replace("{BookedTo}", bookedTo);
            emailMarkupBody = emailMarkupBody.Replace("{VenueName}", venueName);
            emailMarkupBody = emailMarkupBody.Replace("{VenueCity}", venueCity);
            emailMarkupBody = emailMarkupBody.Replace("{VenueState}", venueState);
            emailMarkupBody = emailMarkupBody.Replace("{Price}", price);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = currentBooking.ApplicationUser.Email,
                Body = emailMarkupBody,
                Subject = "Rescheduled Booking Confirmation"
            };

            await _emailService.SendEmailAsync(emailModel);
        }

        public async Task<Booking> GetBookingIdAsync(Guid id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }
        public void GetGdsBasedOnId(Guid id)
        {
            int Gdstype = DbCall based on Id;

            switch(Gdstype)
            {
                case 1:
                    _bookingRepository.dfghj;
                    break;
                case 2:
                    _referralCodeRepository.dfghj;
                    break;
            }
        }
    }
}
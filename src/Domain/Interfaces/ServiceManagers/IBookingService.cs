using LetsWork.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IBookingService
    {
        Task AddNewBookingAsync(Booking NewBooking, string ReferralCode, string BookingTimeZoneId);
        Task<bool> CheckBookingAvailabilityAsync(DateTime FromDate, DateTime ToDate, Guid VenueID);
        Task<List<Booking>> AllBookingDetailsAsync();
        Task<List<Booking>> BookingByPeriodCriteriaAsync(DateTime SearchDateTime, string SearchCriteria);
        Task<List<Booking>> ActiveBookingsAsync(DateTime Date, Guid Id);
        Task<List<Booking>> ClosedBookingsAsync(DateTime Date, Guid Id);
        Task CancelUserBookingDetailsAsync(Guid id);
        Task<(bool HasReferralCodeTransactionCountExceeded, bool IsDuplicateTransaction, bool IsReferralCodeSelfUsed)> CheckReferralCodeAsync(string ReferralCode, Guid BenificiaryUserId);
        Task UpdateBookingStatusAsync();
        Task UpdateBookingAsync(Booking UpdatedBooking, string BookingTimeZoneId);
        Task<Booking> GetBookingIdAsync(Guid id);
    }
}

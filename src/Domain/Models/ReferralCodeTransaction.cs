using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsWork.Domain.Models
{
    public class ReferralCodeTransaction
    {
        public Guid ReferralTransactionId { get; set; }
        public Guid IssuerId { get; set; }
        public Guid BenificiaryId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid ReferralCodeId { get; set; }
        public ReferralCode ReferralCode { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}

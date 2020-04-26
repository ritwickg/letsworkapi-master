using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace LetsWork.Domain.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ProfileImage ProfileImage { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ReferralCode ReferralCode { get; set; }
        public virtual ICollection<ReferralCodeTransaction> ReferralCodeTransactions { get; set; }
        public ApplicationUser()
        {
            Bookings = new HashSet<Booking>();
            ReferralCodeTransactions = new HashSet<ReferralCodeTransaction>();
        }
    }
    public class ApplicationRole : IdentityRole<Guid> { }
    public enum UserType
    {
        Admin = 1,
        User
    }
}

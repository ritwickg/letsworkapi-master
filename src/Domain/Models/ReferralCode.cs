using System;
using System.Collections.Generic;

namespace LetsWork.Domain.Models
{
    public class ReferralCode
    {
        public Guid ReferralCodeId { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string RefCode { get; set; }
        public int ReferralCodeTransactionCount { get; set; }
        public virtual ICollection<ReferralCodeTransaction> ReferralCodeTransactions { get; set; }

        public ReferralCode()
        {
            this.ReferralCodeTransactions = new HashSet<ReferralCodeTransaction>();
        }
    }
}

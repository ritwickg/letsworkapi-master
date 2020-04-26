using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.Data
{
    public class ReferralCodeTransactionRepository : IAsyncRepository<ReferralCodeTransaction>
    {
        private readonly LetsWorkDbContext _context;
        public ReferralCodeTransactionRepository(LetsWorkDbContext Context)
        {
            _context = Context;   
        }
        public async Task AddAsync(ReferralCodeTransaction Entity)
        {
            await _context.Set<ReferralCodeTransaction>().AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ReferralCodeTransaction Entity)
        {
            _context.Set<ReferralCodeTransaction>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ReferralCodeTransaction> GetByIdAsync(Guid id)
        {
            return await _context.Set<ReferralCodeTransaction>()
                                 .Include(x => x.Booking)
                                 .Include(x => x.ReferralCode)
                                 .Include(x => x.User)
                                 .FirstOrDefaultAsync(x => x.ReferralTransactionId == id);
        }

        public async Task<ReferralCodeTransaction> GetSingleBySpecAsync(Expression<Func<ReferralCodeTransaction, bool>> Criteria)
        {
            return await _context.Set<ReferralCodeTransaction>()
                                  .Include(x => x.Booking)
                                  .Include(x => x.ReferralCode)
                                  .Include(x => x.User)
                                  .FirstOrDefaultAsync(Criteria);
        }

        public async Task<List<ReferralCodeTransaction>> ListAllAsync()
        {
            return await _context.Set<ReferralCodeTransaction>()
                                 .Include(x => x.Booking)
                                 .Include(x => x.ReferralCode)
                                 .Include(x => x.User)
                                 .ToListAsync();
        }

        public async Task<List<ReferralCodeTransaction>> ListAsync(Expression<Func<ReferralCodeTransaction, bool>> Criteria)
        {
            return await _context.Set<ReferralCodeTransaction>()
                                 .Include(x => x.Booking)
                                 .Include(x => x.ReferralCode)
                                 .Include(x => x.User)
                                 .Where(Criteria)
                                 .ToListAsync();
        }

        public async Task UpdateAsync(ReferralCodeTransaction Entity)
        {
            _context.Set<ReferralCodeTransaction>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

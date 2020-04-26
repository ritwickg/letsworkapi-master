using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.Data
{
    public class ReferralCodeRepository : IAsyncRepository<ReferralCode>
    {
        private readonly LetsWorkDbContext _context;
        public ReferralCodeRepository(LetsWorkDbContext Context)
        {
            _context = Context;
        }
        public async Task AddAsync(ReferralCode Entity)
        {
            await _context.Set<ReferralCode>().AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ReferralCode Entity)
        {
            _context.Set<ReferralCode>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ReferralCode> GetByIdAsync(Guid id)
        {
            return await _context.Set<ReferralCode>()
                         .Include(x => x.ReferralCodeTransactions)
                         .FirstOrDefaultAsync(x => x.ReferralCodeId == id);
        }

        public async Task<ReferralCode> GetSingleBySpecAsync(Expression<Func<ReferralCode, bool>> Criteria)
        {
            return await _context.Set<ReferralCode>()
                         .Include(x => x.ReferralCodeTransactions)
                         .FirstOrDefaultAsync(Criteria);
        }

        public async Task<List<ReferralCode>> ListAllAsync()
        {
            return await _context
                        .Set<ReferralCode>()
                        .Include(x => x.ReferralCodeTransactions)
                        .ToListAsync();
        }

        public async Task<List<ReferralCode>> ListAsync(Expression<Func<ReferralCode, bool>> Criteria)
        {
            return await _context.Set<ReferralCode>()
                         .Where(Criteria)
                         .Include(x => x.ReferralCodeTransactions)
                         .ToListAsync();
        }

        public async Task UpdateAsync(ReferralCode Entity)
        {
            _context.Set<ReferralCode>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

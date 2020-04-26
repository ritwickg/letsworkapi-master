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
    public class BookingRepository : IAsyncRepository<Booking>
    {
        private readonly LetsWorkDbContext _context;
        public BookingRepository(LetsWorkDbContext Context)
        {
            _context = Context;
        }
        public async Task AddAsync(Booking Entity)
        {
            await _context
                 .Set<Booking>()
                 .AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Booking Entity)
        {
            _context.Set<Booking>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Booking> GetByIdAsync(Guid id)
        {
            return await _context.Set<Booking>()
                        .Include(x => x.ApplicationUser)
                        .Include(x => x.VenueDetail)
                        .Include(x => x.VenueDetail.InventoryDetails)
                        .Include(x => x.ReferralCodeTransaction)
                        .FirstOrDefaultAsync(x => x.BookingID == id);
        }

        public async Task<Booking> GetSingleBySpecAsync(Expression<Func<Booking, bool>> Criteria)
        {
            return await _context.Set<Booking>()
                         .Include(x => x.ApplicationUser)
                         .Include(x => x.VenueDetail)
                         .Include(x => x.VenueDetail.InventoryDetails)
                         .Include(x => x.ReferralCodeTransaction)
                         .FirstOrDefaultAsync(Criteria);
        }

        public async Task<List<Booking>> ListAllAsync()
        {
            return await _context.Set<Booking>()
                        .Include(x => x.ApplicationUser)
                        .Include(x => x.VenueDetail)
                        .Include(x => x.VenueDetail.InventoryDetails)
                        .Include(x => x.ReferralCodeTransaction)
                        .ToListAsync();
        }

        public async Task<List<Booking>> ListAsync(Expression<Func<Booking, bool>> Criteria)
        {
            return await _context.Set<Booking>()
                        .Include(x => x.ApplicationUser)
                        .Include(x => x.VenueDetail)
                        .Include(x => x.VenueDetail.InventoryDetails)
                        .Include(x => x.ReferralCodeTransaction)
                        .Where(Criteria)
                        .ToListAsync();
        }

        public async Task UpdateAsync(Booking Entity)
        {
            _context.Set<Booking>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

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
    public class VenueRepository : IAsyncRepository<VenueDetail>
    {
        private readonly LetsWorkDbContext _context;
        public VenueRepository(LetsWorkDbContext Context)
        {
            _context = Context;
        }
        public async Task AddAsync(VenueDetail Entity)
        {
            await _context.Set<VenueDetail>().AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(VenueDetail Entity)
        {
            _context.Set<VenueDetail>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<VenueDetail> GetByIdAsync(Guid id)
        {
           return await _context.Set<VenueDetail>()
                          .Include(x => x.InventoryDetails)
                          .Include(x => x.VenueImages)
                          .FirstOrDefaultAsync(x => x.VenueID == id); 
        }

        public async Task<VenueDetail> GetSingleBySpecAsync(Expression<Func<VenueDetail, bool>> Criteria)
        {
            return await _context.Set<VenueDetail>()
                          .Include(x => x.InventoryDetails)
                          .Include(x => x.VenueImages)
                          .FirstOrDefaultAsync(Criteria);
        }
        public async Task<List<VenueDetail>> ListAllAsync()
        {
            return await _context.Set<VenueDetail>()
                           .Include(x => x.VenueImages)
                           .Include(x => x.InventoryDetails)
                           .ToListAsync();
        }

        public async Task<List<VenueDetail>> ListAsync(Expression<Func<VenueDetail, bool>> Criteria)
        {
            return await _context.Set<VenueDetail>()
                          .Include(x => x.InventoryDetails)
                          .Include(x => x.VenueImages)
                          .Where(Criteria)
                          .ToListAsync();
        }

        public async Task UpdateAsync(VenueDetail Entity)
        {
            _context.Set<VenueDetail>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

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
    public class VenueImageRepository : IAsyncRepository<VenueImage>
    {
        private readonly LetsWorkDbContext _context;
        public VenueImageRepository(LetsWorkDbContext Context)
        {
            _context = Context;
        }
        public async Task AddAsync(VenueImage Entity)
        {
            await _context.Set<VenueImage>().AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(VenueImage Entity)
        {
            _context.Set<VenueImage>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<VenueImage> GetByIdAsync(Guid id)
        {
           return await _context
                        .Set<VenueImage>()
                        .FindAsync(id);
        }

        public async Task<VenueImage> GetSingleBySpecAsync(Expression<Func<VenueImage, bool>> Criteria)
        {
            return await _context.Set<VenueImage>()
                         .Where(Criteria)
                         .FirstOrDefaultAsync();
        }

        public async Task<List<VenueImage>> ListAllAsync()
        {
            return await _context.Set<VenueImage>().ToListAsync();
        }

        public async Task<List<VenueImage>> ListAsync(Expression<Func<VenueImage, bool>> Criteria)
        {
            return await _context.Set<VenueImage>()
                         .Where(Criteria)
                         .ToListAsync();
        }

        public async Task UpdateAsync(VenueImage Entity)
        {
            _context.Set<VenueImage>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

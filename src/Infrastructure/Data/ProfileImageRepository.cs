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
    public class ProfileImageRepository : IAsyncRepository<ProfileImage>
    {
        private readonly LetsWorkDbContext _context;
        public ProfileImageRepository(LetsWorkDbContext Context)
        {
            _context = Context;
        }
        public async Task AddAsync(ProfileImage Entity)
        {
            await _context.Set<ProfileImage>().AddAsync(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProfileImage Entity)
        {
            _context.Set<ProfileImage>().Remove(Entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ProfileImage> GetByIdAsync(Guid id)
        {
            return await _context.Set<ProfileImage>().FindAsync(id);
        }

        public async Task<ProfileImage> GetSingleBySpecAsync(Expression<Func<ProfileImage, bool>> Criteria)
        {
            return await _context
                         .Set<ProfileImage>()
                         .FirstOrDefaultAsync(Criteria);
        }

        public async Task<List<ProfileImage>> ListAllAsync()
        {
            return await _context.Set<ProfileImage>().ToListAsync();
        }

        public async Task<List<ProfileImage>> ListAsync(Expression<Func<ProfileImage, bool>> Criteria)
        {
            return await _context
                         .Set<ProfileImage>()
                         .Where(Criteria)
                         .ToListAsync();
        }
        public async Task UpdateAsync(ProfileImage Entity)
        {
            _context.Set<ProfileImage>().Update(Entity);
            await _context.SaveChangesAsync();
        }
    }
}

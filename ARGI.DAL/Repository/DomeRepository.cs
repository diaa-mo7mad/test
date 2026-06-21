using ARGI.DAL.Data;
using ARGI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Repository
{
    public class DomeRepository : IDomeRepository
    {
        private readonly ApplicationDbContext _context;

        public DomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<Dome> GetByIdAsync(int id)
        {
            return await _context.Domes.FindAsync(id);
        }

        public async Task<IEnumerable<Dome>> GetAllAsync()
        {
            return await _context.Domes.ToListAsync();
        }

        public async Task AddAsync(Dome dome)
        {
            await _context.Domes.AddAsync(dome);
        }

        public void Update(Dome dome)
        {
            _context.Domes.Update(dome);
        }

        public void Delete(Dome dome)
        {
            _context.Domes.Remove(dome);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }

        // تطبيق الدوال الخاصة بمشروعك
        public async Task<IEnumerable<Dome>> GetUserDomesAsync(string userId)
        {
            return await _context.Domes
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<Dome> GetByMacAddressAsync(string macAddress)
        {
            return await _context.Domes
                .FirstOrDefaultAsync(d => d.MacAddress == macAddress);
        }

        public async Task<bool> IsMacAddressExistsAsync(string macAddress)
        {
            return await _context.Domes.AnyAsync(d => d.MacAddress == macAddress);
        }
    }
}

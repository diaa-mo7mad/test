using ARGI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Repository
{
    public interface IDomeRepository 
    {
        Task<Dome> GetByIdAsync(int id);
        Task<IEnumerable<Dome>> GetAllAsync();
        Task AddAsync(Dome dome);
        void Update(Dome dome);
        void Delete(Dome dome);
        Task<bool> SaveChangesAsync();

        
        Task<IEnumerable<Dome>> GetUserDomesAsync(string userId);
        Task<Dome> GetByMacAddressAsync(string macAddress);
        Task<bool> IsMacAddressExistsAsync(string macAddress);
    }
}

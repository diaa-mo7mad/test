using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CodeResetPassword { get; set; }
        public DateTime? CodeResetPasswordExpiration { get; set; }
        public virtual ICollection<Dome> Domes { get; set; } = new HashSet<Dome>();

    }
}

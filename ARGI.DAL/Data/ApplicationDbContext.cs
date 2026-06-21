using ARGI.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {   
        public DbSet<Dome> Domes { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<IrrigationSchedule> IrrigationSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            builder.Entity<Dome>()
                .HasIndex(d => d.MacAddress)
                       .IsUnique();

            builder.Entity<Dome>()
                  .HasMany(d => d.SensorReadings)
                 .WithOne(r => r.Dome)
                 .HasForeignKey(r => r.DomeId)
                 .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
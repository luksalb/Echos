using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Echos.Api.Domain.Users;

namespace Echos.Api.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.UserName)
                      .IsUnique();

                entity.HasIndex(u => u.Email)
                      .IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Echos.Api.Domain.Users;
using Echos.Api.Domain.Echos;

namespace Echos.Api.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Echo> Echos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

                entity.HasIndex(u => u.UserName)
                      .IsUnique();
                entity.HasIndex(u => u.Email)
                      .IsUnique();
            });

            modelBuilder.Entity<Echo>(entity =>
            {
                entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}

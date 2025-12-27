using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Persistence
{
    public class AccountContext : DbContext
    {
        private readonly string _connectionString;

        public AccountContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }

        public AccountContext(DbContextOptions<AccountContext> options) : base(options) { }

        public DbSet<UsersTable> userTable { get; set; }

        public DbSet<ProfileTable> profileTable { get; set; }

        public DbSet<SkillsTable> skillsTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsersTable>().HasData(
                new UsersTable()
                {
                    Id = Guid.NewGuid(),
                    first_name = "Антон",
                    last_name = "(Study)",
                    username = "studywhite",
                    telegram_id = "1006365928",
                    photo_url = null,
                    last_login = null,
                    is_verified = true,
                    roles = new[] { "Freelancer" },
                    status = "offline",
                    created_at = DateTime.UtcNow
                }
            );


            modelBuilder.Entity<UsersTable>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(u => u.Profile)
                    .WithOne(p => p.User)
                    .HasForeignKey<ProfileTable>(p => p.user_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Skills)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.user_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProfileTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.user_id)
                    .IsUnique();

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Profile)
                    .HasForeignKey<ProfileTable>(p => p.user_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SkillsTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.user_id);
            });
        }
    }
}

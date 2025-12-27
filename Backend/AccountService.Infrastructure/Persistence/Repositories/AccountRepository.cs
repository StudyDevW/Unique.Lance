using AccountService.Domain.Entities;
using AccountService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountContext _context;
        public AccountRepository(AccountContext context) {
            _context = context;
        }

        public async Task<UsersTable?> GetUserFromId(Guid userId)
        {
            return await _context.userTable
                .Include(u => u.Profile) 
                .Include(u => u.Skills) 
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task CreateUser(UsersTable userInfo)
        {
            await _context.userTable.AddAsync(userInfo);
        }

        public async Task<List<SkillsTable>?> GetSkillsFromUserId(Guid userId)
        {
            return await _context.skillsTable
                .Where(c => c.user_id == userId)
                .ToListAsync();
        }

        public async Task<ProfileTable?> GetUserProfileFromId(Guid userId)
        {
            return await _context.profileTable
                .FirstOrDefaultAsync(c => c.Id == userId);
        }

        public void UpdateUser(UsersTable user)
        {
            _context.userTable.Update(user);
        }

        public void DeleteUser(UsersTable user)
        {
            _context.userTable.Remove(user);
        }
        public async Task<UsersTable?> GetUserFromTelegramId(long telegramId)
        {
            return await _context.userTable
                .Include(u => u.Profile)
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.telegram_id == telegramId.ToString());
        }

        public async Task<bool> UserExists(Guid userId)
        {
            return await _context.userTable
                .AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> UserExistsByTelegramId(long telegramId)
        {
            return await _context.userTable
                .AnyAsync(u => u.telegram_id == telegramId.ToString());
        }
        public async Task<List<UsersTable>> GetUsers(
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null,
            List<string>? roles = null)
        {
            var query = _context.userTable
                .Include(u => u.Profile)
                .Include(u => u.Skills)
                .AsQueryable();

            //фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.first_name.Contains(searchTerm) ||
                    (u.last_name != null && u.last_name.Contains(searchTerm)) ||
                    u.username.Contains(searchTerm));
            }

            //фильтрация по ролям
            if (roles != null && roles.Any())
            {
                query = query.Where(u => u.roles.Any(r => roles.Contains(r)));
            }

            return await query
                .OrderByDescending(u => u.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUsersCount(
            string? searchTerm = null,
            List<string>? roles = null)
        {
            var query = _context.userTable
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.first_name.Contains(searchTerm) ||
                    (u.last_name != null && u.last_name.Contains(searchTerm)) ||
                    u.username.Contains(searchTerm));
            }

            if (roles != null && roles.Any())
            {
                query = query.Where(u => u.roles.Any(r => roles.Contains(r)));
            }

            return await query.CountAsync();
        }

        public async Task<List<UsersTable>> GetUsersByIds(List<Guid> userIds)
        {
            return await _context.userTable
                .Include(u => u.Profile)
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task AddSkill(SkillsTable skill)
        {
            await _context.skillsTable.AddAsync(skill);
        }

        public void UpdateSkill(SkillsTable skill)
        {
            _context.skillsTable.Update(skill);
        }

        public void DeleteSkill(SkillsTable skill)
        {
            _context.skillsTable.Remove(skill);
        }

        public async Task AddProfile(ProfileTable profile)
        {
            await _context.profileTable.AddAsync(profile);
        }

        public void UpdateProfile(ProfileTable profile)
        {
            _context.profileTable.Update(profile);
        }

        public async Task<UsersTable?> GetUserFromUsername(string username)
        {
            return await _context.userTable
                .FirstOrDefaultAsync(u => u.username == username);
        }
        public async Task UpdateUserStatus(Guid userId, string status, DateTime? lastLogin = null)
        {
            var user = await _context.userTable.FindAsync(userId);
            if (user != null)
            {
                user.status = status;
                if (lastLogin.HasValue)
                    user.last_login = lastLogin.Value;

                _context.userTable.Update(user);
            }
        }
    }
}

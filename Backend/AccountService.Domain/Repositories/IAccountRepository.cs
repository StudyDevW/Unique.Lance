using AccountService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Repositories
{
    public interface IAccountRepository
    {
        public Task<UsersTable?> GetUserFromId(Guid userId);

        public Task<UsersTable?> GetUserFromTelegramId(long telegramId);

        public Task<UsersTable?> GetUserFromUsername(string username);

        public Task<List<UsersTable>> GetUsersByIds(List<Guid> userIds);

        public Task<List<UsersTable>> GetUsers(int page = 1,int pageSize = 20,string? searchTerm = null, List<string>? roles = null);
        
        public Task<int> GetUsersCount(string? searchTerm = null, List<string>? roles = null);

        public Task<bool> UserExists(Guid userId);

        public Task<bool> UserExistsByTelegramId(long telegramId);

        public Task CreateUser(UsersTable userInfo);

        public void UpdateUser(UsersTable user);

        public void DeleteUser(UsersTable user);

        public Task UpdateUserStatus(Guid userId, string status, DateTime? lastLogin = null);


        public Task<ProfileTable?> GetUserProfileFromId(Guid userId);

        public Task AddProfile(ProfileTable profile);

        public void UpdateProfile(ProfileTable profile);



        public Task<List<SkillsTable>?> GetSkillsFromUserId(Guid userId);

        public Task AddSkill(SkillsTable skill);

        public void UpdateSkill(SkillsTable skill);

        public void DeleteSkill(SkillsTable skill);

    }
}

using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Services
{
    public class DatabaseAutoMigrations : IDatabaseAutoMigrations
    {
        private readonly AccountContext _contextAccount;
        private readonly ILogger _logger;

        public DatabaseAutoMigrations(IConfiguration conf)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("auto-migration-logger");
            _contextAccount = new AccountContext(conf["ACCOUNT_DATABASE"]);
        }

        private async Task<bool> CheckIfTableExistsAsync(DbContext context, string tableName)
        {
            var connection = (NpgsqlConnection)context.Database.GetDbConnection();
            await connection.OpenAsync();

            var exists = false;

            var command = new NpgsqlCommand(
                $"SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '{tableName}');",
                connection);

            exists = (bool)await command.ExecuteScalarAsync();

            await connection.CloseAsync();
            return exists;
        }

        public async Task EnsureDatabaseInitializedAsync()
        {
            List<string> tableNamesAccountDB = new List<string>()
            {
                "userTable",
                "profileTable",
                "skillsTable"
            };

            List<string> tablesNotExistAccountDB = new List<string>();

            var counterAccountDB = 0;

            for (int i = 0; i < tableNamesAccountDB.Count; i++)
            {
                var tableExists = await CheckIfTableExistsAsync(_contextAccount, tableNamesAccountDB[i]);

                if (!tableExists)
                {
                    _logger.LogWarning($"accountdb_table_not_exist: {tableNamesAccountDB[i]}");

                    tablesNotExistAccountDB.Add(tableNamesAccountDB[i]);
                }

                counterAccountDB++;
            }

            if (counterAccountDB == tableNamesAccountDB.Count && tablesNotExistAccountDB.Count > 0)
            {
                await _contextAccount.Database.EnsureDeletedAsync(); //костыль на время development :D
                await _contextAccount.Database.MigrateAsync();

                _logger.LogInformation("accountdb_automigrated");
            }
            else if (tablesNotExistAccountDB.Count == 0)
            {
                _logger.LogInformation("accountdb_nomigration_needed");
            }
        }
    }
}

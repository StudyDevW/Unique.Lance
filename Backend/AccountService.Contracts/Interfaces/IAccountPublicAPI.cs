
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Contracts.Interfaces
{
    public interface IAccountPublicAPI
    {
        //  public Task<AccountDTO> GetAccountAsync(Guid accountId, CancellationToken ct = default);

        public Task<bool> ValidateTokenPublicAPI();
    }
}

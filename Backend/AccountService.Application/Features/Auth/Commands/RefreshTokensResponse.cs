using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Commands
{
    public record RefreshTokensResponse
    {
        public string accessToken { get; init; }

        public string refreshToken { get; init; }

        public DateTime until { get; init; }
    }
}

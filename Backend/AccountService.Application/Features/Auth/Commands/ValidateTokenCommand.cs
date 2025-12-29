using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Commands
{
    public record ValidateTokenCommand : IRequest<bool>
    {
        public string accessToken { get; init; }
    }
}

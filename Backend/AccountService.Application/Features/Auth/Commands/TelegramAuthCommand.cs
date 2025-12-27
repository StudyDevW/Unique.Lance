using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Commands
{
    //из telegram api 
    public record TelegramAuthCommand : IRequest<TelegramAuthResponse>
    {
        public long id { get; init; }
        public string first_name { get; init; }
        public string? last_name { get; init; }
        public string? username { get; init; }
        public bool is_bot { get; init; }
        public string? photo_url { get; init; }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Commands
{
    //из telegram api
    public class TelegramAuthRequest
    {
        public long id { get; set; }
        public string first_name { get; set; }
        public string? last_name { get; set; }
        public string? username { get; set; }
        public bool is_bot { get; set; }
        public string? photo_url { get; set; }
    }
}

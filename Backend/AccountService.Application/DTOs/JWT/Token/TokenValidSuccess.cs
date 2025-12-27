using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.JWT.Token
{
    public class TokenValidSuccess
    {
        public Guid Id { get; set; }

        public string? userName { get; set; }

        public List<string>? userRoles { get; set; }

        public string? bearerWithoutPrefix { get; set; }
    }
}

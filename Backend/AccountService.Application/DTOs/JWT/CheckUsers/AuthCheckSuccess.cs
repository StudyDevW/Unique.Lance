using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.JWT.CheckUsers
{
    public class AuthCheckSuccess
    {
        public Guid Id { get; set; }

        public string? username { get; set; }

        public List<string>? roles { get; set; }
    }
}

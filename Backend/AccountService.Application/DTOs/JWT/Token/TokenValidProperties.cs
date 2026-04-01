using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.JWT.Token
{
    public class TokenValidProperties
    {
        public TokenValidSuccess? token_success { get; set; }

        public TokenValidError? token_error { get; set; }

        public bool TokenHasError() { return token_error != null; }

        public bool TokenHasSuccess() { return token_success != null; }
    }
}

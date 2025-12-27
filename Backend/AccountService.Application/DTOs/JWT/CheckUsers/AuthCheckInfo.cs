using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.JWT.CheckUsers
{
    public class AuthCheckInfo
    {
        public AuthCheckSuccess? check_success { get; set; }

        public AuthCheckError? check_error { get; set; }

        public bool CheckHasError() { return check_error != null; }

        public bool CheckHasSuccess() { return check_success != null; }
    }
}

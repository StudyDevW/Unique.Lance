using AccountService.Application.DTOs.JWT.CheckUsers;
using AccountService.Application.DTOs.JWT.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces
{
    public interface IJwtTokenService
    {
        public string JwtTokenCreation(AuthCheckSuccess dtoObj);

        public string RefreshTokenCreation(AuthCheckSuccess dtoObj);
        public Task<TokenValidProperties> AccessTokenValidation(string? bearerKey, string checkrole = "none");

        public Task<TokenValidProperties> RefreshTokenValidation(string? bearerKey);
    }
}

using AccountService.Application.Features.Auth.Commands;
using AccountService.Application.Interfaces;
using MediatR;
using Middleware.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Handlers
{
    public class ValidateTokenCommandHandler : IRequestHandler<ValidateTokenCommand, bool>
    {
        private readonly ICacheService _cacheService;
        private readonly IJwtTokenService _jwtService;

        public ValidateTokenCommandHandler(IJwtTokenService jwtService, ICacheService cacheService)
        {
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<bool> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
        {
            var validation = await _jwtService.AccessTokenValidation(request.accessToken);

            return validation.TokenHasSuccess();
        }
    }
}

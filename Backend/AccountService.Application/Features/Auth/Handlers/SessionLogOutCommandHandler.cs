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

    public class SessionLogOutCommandHandler : IRequestHandler<SessionLogOutCommand, Unit>
    {
        private readonly ICacheService _cacheService;
        private readonly IJwtTokenService _jwtService;

        public SessionLogOutCommandHandler(IJwtTokenService jwtService, ICacheService cacheService)
        {
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<Unit> Handle(SessionLogOutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _jwtService.AccessTokenValidation(request.accessToken);

            if (validation.TokenHasSuccess())
            {
                _cacheService.DeleteKeyFromStorage(validation.token_success!.Id, "accessTokens");

                _cacheService.DeleteKeyFromStorage(validation.token_success!.Id, "refreshTokens");
            }
            else
            {
                throw new Exception("token_not_validated");
            }

            return Unit.Value;
        }

    }
}

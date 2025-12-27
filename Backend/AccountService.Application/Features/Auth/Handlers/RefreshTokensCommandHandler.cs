using AccountService.Application.DTOs.JWT.CheckUsers;
using AccountService.Application.Features.Auth.Commands;
using AccountService.Application.Interfaces;
using AccountService.Domain.Repositories;
using MediatR;
using Middleware.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Handlers
{
    public class RefreshTokensCommandHandler : IRequestHandler<RefreshTokensCommand, RefreshTokensResponse>
    {
        private readonly IJwtTokenService _jwtService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokensCommandHandler(IJwtTokenService jwtService, ICacheService cacheService, IUnitOfWork unitOfWork)
        {
            _jwtService = jwtService;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RefreshTokensResponse> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
        {
            var validation = await _jwtService.RefreshTokenValidation(request.refreshToken);

            if (validation.TokenHasSuccess())
            {
                var user = await _unitOfWork.Users.GetUserFromId(validation.token_success!.Id);
                if (user == null)
                    throw new Exception("user_not_found");

                var checkSuccess = new AuthCheckSuccess
                {
                    Id = user.Id,
                    roles = user.roles.ToList(),
                    username = user.username
                };

                if (_cacheService.CheckExistKeysStorage(validation.token_success!.Id, "accessTokens"))
                    _cacheService.DeleteKeyFromStorage(validation.token_success!.Id, "accessTokens");

                if (_cacheService.CheckExistKeysStorage(validation.token_success!.Id, "refreshTokens"))
                    _cacheService.DeleteKeyFromStorage(validation.token_success!.Id, "refreshTokens");

                var generatedAccessToken = _jwtService.JwtTokenCreation(checkSuccess);

                var generatedRefreshToken = _jwtService.RefreshTokenCreation(checkSuccess);

                return new RefreshTokensResponse {
                    accessToken = generatedAccessToken,
                    refreshToken = generatedRefreshToken,
                    until = DateTime.UtcNow.AddMinutes(5)
                };
            }
            else
            {
                throw new Exception("token_not_validated");
            }
        }
    }
}

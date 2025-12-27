using AccountService.Application.DTOs.JWT.CheckUsers;
using AccountService.Application.Features.Auth.Commands;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entities;
using AccountService.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Middleware.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Features.Auth.Handlers
{
    public class TelegramAuthCommandHandler : IRequestHandler<TelegramAuthCommand, TelegramAuthResponse>
    {
        private readonly IJwtTokenService _jwtService;
        private readonly IUnitOfWork _unitOfWork;

        public TelegramAuthCommandHandler(IJwtTokenService jwtService, IUnitOfWork unitOfWork)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;   
        }

        public async Task<TelegramAuthResponse> Handle(TelegramAuthCommand request, CancellationToken ct)
        {

            var user = await _unitOfWork.Users.GetUserFromTelegramId(request.id);

            if (user == null)
            {
                user = new UsersTable
                {
                    Id = Guid.NewGuid(),
                    telegram_id = request.id.ToString(),
                    username = request.username!,
                    first_name = request.first_name,
                    last_name = request.last_name,
                    roles = new string[] { "Freelancer" },
                    status = "Online",
                    created_at = DateTime.UtcNow,
                    last_login = DateTime.UtcNow
                };

                await _unitOfWork.Users.CreateUser(user);
            }
            else
            {
                user.last_login = DateTime.UtcNow;
                user.status = "Online";
                _unitOfWork.Users.UpdateUser(user);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var generatedAccessToken = _jwtService.JwtTokenCreation(new AuthCheckSuccess
            {
                Id = user.Id,
                roles = user.roles.ToList(),
                username = user.username
            });


            var generatedRefreshToken = _jwtService.RefreshTokenCreation(new AuthCheckSuccess
            {
                Id = user.Id,
                roles = user.roles.ToList(),
                username = user.username
            });

            return new TelegramAuthResponse
            {
                accessToken = generatedAccessToken,
                refreshToken = generatedRefreshToken,
                until = DateTime.UtcNow.AddMinutes(5)
            };
        }

    }
}

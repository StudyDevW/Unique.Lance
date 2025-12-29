using AccountService.Application.Features.Auth.Commands;
using AccountService.Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace AccountService.API.Controllers
{
    [Route("api/auth/")]
    [ApiController]
    public class AuthController : ControllerBase, IAccountPublicAPI
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("telegram_auth")]
        public async Task<IActionResult> TelegramAuth([FromBody] TelegramAuthRequest request)
        {
            var command = new TelegramAuthCommand
            {
                id = request.id,
                first_name = request.first_name,
                last_name = request.last_name,
                username = request.username,
                is_bot = request.is_bot,
                photo_url = request.photo_url
            };

            var result = await _mediator.Send(command);

            Response.Cookies.Append("refreshToken", result.refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                accessToken = result.accessToken,
                until = result.until
            });
        }

        [HttpPost("refresh_tokens")]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshTokenCookies = Request.Cookies["refreshToken"];

            if (refreshTokenCookies == null)
                return Unauthorized("token_not_found_in_cookies");

            var result = await _mediator.Send(new RefreshTokensCommand { refreshToken = refreshTokenCookies });

            Response.Cookies.Append("refreshToken", result.refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                accessToken = result.accessToken,
                until = result.until
            });
        }

        public async Task<bool> ValidateTokenPublicAPI()
        {
            var result = await _mediator.Send(new ValidateTokenCommand { accessToken = Request.Headers["Authorization"] });

            return result;
        }

        [HttpGet("validate")]
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        public async Task<IActionResult> ValidateToken()
        {
            var validate = await ValidateTokenPublicAPI();

            if (!validate)
                return Unauthorized();

            return Ok("valid");
        }

        [HttpPut("sign_out")]
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                await _mediator.Send(new SessionLogOutCommand { accessToken = Request.Headers["Authorization"] });

                Response.Cookies.Delete("refreshToken");

                return Ok("signed_out");
            }
            catch (Exception ex) {
                if (ex.Message == "token_not_validated")
                    return Unauthorized();

                return BadRequest();
            }

        }

    }
}

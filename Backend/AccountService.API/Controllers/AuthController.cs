using AccountService.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers
{
    [Route("api/auth/")]
    [ApiController]
    public class AuthController : ControllerBase
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
    }
}

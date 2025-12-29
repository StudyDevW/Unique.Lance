using AccountService.Application.DTOs.JWT.CheckUsers;
using AccountService.Application.DTOs.JWT.Token;
using AccountService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Middleware.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        public JwtTokenService() { }

        public JwtTokenService(IConfiguration conf, ICacheService cache)
        {
            _configuration = conf;
            _cacheService = cache;
        }

        private async Task<JwtSecurityToken> RSAJwtValidation(string? token)
        {
            var rsa = RSA.Create();

            rsa.ImportFromPem(_configuration["RSA_PUBLIC_KEY"]);

            RsaSecurityKey issuerSigningKey = new RsaSecurityKey(rsa);

            TokenValidationParameters tk_valid = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = issuerSigningKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                RequireExpirationTime = false,
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, tk_valid, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<JwtSecurityToken> RSARefreshTokenValidation(string? token)
        {
            var rsa = RSA.Create();

            rsa.ImportFromPem(_configuration["RSA_PUBLIC_KEY"]);

            RsaSecurityKey issuerSigningKey = new RsaSecurityKey(rsa);

            TokenValidationParameters tk_valid = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = issuerSigningKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                RequireExpirationTime = false,
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, tk_valid, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<TokenValidProperties> AccessTokenValidation(string? bearerKey, string checkrole)
        {

            if (bearerKey == null)
                return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "token_empty" } };

            if (!bearerKey.Contains("Bearer "))
                return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "format_unknown" } };

            string bearer_key_without_prefix = bearerKey.Substring("Bearer ".Length);

            var validation = await RSAJwtValidation(bearer_key_without_prefix);

            var expectedAlg = SecurityAlgorithms.RsaSha512;

            if (validation == null)
            {
                return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };
            }
            else
            {
                if (validation.Header?.Alg == null || validation.Header?.Alg != expectedAlg)
                {
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unexpected_alg" } };
                }

                string userName = "";
                Guid userId = Guid.Empty;
                List<string> userRoles = new List<string>();

                foreach (var claim in validation.Claims)
                {
                    if (claim.Type == "Username")
                        userName = claim.Value;

                    if (claim.Type == "Id")
                        userId = Guid.Parse(claim.Value);

                    if (claim.Type == "Roles")
                        userRoles = JsonSerializer.Deserialize<List<string>>(claim.Value);
                }

                if (userId == Guid.Empty)
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };

                if (userRoles == null)
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };

                if (_cacheService.CheckExistKeysStorage(userId, "accessTokens"))
                {
                    //Проверка на то, подменен ли ключ или нет!
                    if (_cacheService.GetKeyFromStorage(userId, "accessTokens") != bearer_key_without_prefix)
                        return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };
                }
                else
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };

                if (checkrole != "none")
                {
                    if (!userRoles.Contains(checkrole))
                        return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized_role" } };
                }

                TokenValidSuccess valid_success = new TokenValidSuccess
                {
                    Id = userId,
                    userName = userName,
                    userRoles = userRoles,
                    bearerWithoutPrefix = bearer_key_without_prefix
                };

                return new TokenValidProperties()
                {
                    token_success = valid_success
                };
            }
        }

        public async Task<TokenValidProperties> RefreshTokenValidation(string? bearerKey)
        {
            var validation = await RSARefreshTokenValidation(bearerKey);

            var expectedAlg = SecurityAlgorithms.RsaSha512;

            if (validation == null)
            {
                return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };
            }
            else
            {
                if (validation.Header?.Alg == null || validation.Header?.Alg != expectedAlg)
                {
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unexpected_alg" } };
                }

                string userName = "";
                Guid userId = Guid.Empty;
                List<string> userRoles = new List<string>();

                foreach (var claim in validation.Claims)
                {
                    if (claim.Type == "Username")
                        userName = claim.Value;

                    if (claim.Type == "Id")
                        userId = Guid.Parse(claim.Value);

                    if (claim.Type == "Roles")
                        userRoles = JsonSerializer.Deserialize<List<string>>(claim.Value);
                }

                if (userId == Guid.Empty)
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };

                if (userRoles == null)
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };


                if (_cacheService.CheckExistKeysStorage(userId, "refreshTokens"))
                {
                    //Проверка на то, подменен ли ключ или нет!
                    if (_cacheService.GetKeyFromStorage(userId, "refreshTokens") != bearerKey)
                        return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };
                }
                else
                    return new TokenValidProperties() { token_error = new TokenValidError { errorLog = "unauthorized" } };

                TokenValidSuccess valid_success = new TokenValidSuccess
                {
                    Id = userId,
                    userName = userName,
                    userRoles = userRoles,
                    bearerWithoutPrefix = bearerKey
                };

                return new TokenValidProperties()
                {
                    token_success = valid_success
                };
            }
        }

        public string JwtTokenCreation(AuthCheckSuccess dtoObj)
        {
            if (dtoObj == null)
                return string.Empty;

            if (dtoObj.username == null)
                return string.Empty;

            var rsaprivateKey = _configuration["RSA_PRIVATE_KEY"];

            using var rsa = RSA.Create();
            rsa.ImportFromPem(rsaprivateKey);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha512)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var serializer_roles = JsonSerializer.Serialize(dtoObj.roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", dtoObj.Id.ToString()),
                    new Claim("Username", dtoObj.username),
                    new Claim("Roles", serializer_roles),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = signingCredentials,

            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            if (_cacheService.CheckExistKeysStorage(dtoObj.Id, "accessTokens"))
                _cacheService.DeleteKeyFromStorage(dtoObj.Id, "accessTokens");

            _cacheService.WriteKeyInStorage(dtoObj.Id, "accessTokens", jwtToken, DateTime.UtcNow.AddMinutes(10));

            return jwtToken;
        }

        public string RefreshTokenCreation(AuthCheckSuccess dtoObj)
        {
            if (dtoObj == null)
                return string.Empty;

            if (dtoObj.username == null)
                return string.Empty;

            var rsaprivateKey = _configuration["RSA_PRIVATE_KEY"];

            using var rsa = RSA.Create();
            rsa.ImportFromPem(rsaprivateKey);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha512)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var serializer_roles = JsonSerializer.Serialize(dtoObj.roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", dtoObj.Id.ToString()),
                    new Claim("Username", dtoObj.username),
                    new Claim("Roles", serializer_roles),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = signingCredentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            if (_cacheService.CheckExistKeysStorage(dtoObj.Id, "refreshTokens"))
                _cacheService.DeleteKeyFromStorage(dtoObj.Id, "refreshTokens");

            _cacheService.WriteKeyInStorage(dtoObj.Id, "refreshTokens", jwtToken, DateTime.UtcNow.AddDays(7));

            return jwtToken;
        }
    }
}

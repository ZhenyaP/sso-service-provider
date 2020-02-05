using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ServiceProvider.API.Constants;
using ServiceProvider.API.Entities;
using ServiceProvider.API.Exceptions;
using ServiceProvider.API.Extensions;

namespace ServiceProvider.API.Helpers
{
    public class JwtTokenHelper
    {
        #region Private

        /// <summary>
        /// The HTTP client
        /// </summary>
        private readonly HttpClient _httpClient;

        private readonly ConfigSettings _configSettings;

        /// <summary>
        /// Gets the signing keys.
        /// </summary>
        /// <returns>the signing keys.</returns>
        private async Task<JsonWebKeySet> GetSigningKeysAsync()
        {
            JsonWebKeySet signingKeys;
            var jwksUrl = this._configSettings.JwksUrl;
            var signingKeysJson = await this._httpClient.GetStringAsync(jwksUrl)
                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(signingKeysJson))
            {
                throw new JsonWebKeyNotFoundException(CommonConstants.ErrorMessages.JwksNotFound);
            }

            signingKeys = new JsonWebKeySet(signingKeysJson);

            return signingKeys;
        }

        /// <summary>
        /// Gets the signing key by key identifier.
        /// </summary>
        /// <param name="signingKeys">The signing keys.</param>
        /// <param name="keyId">The key identifier.</param>
        /// <returns>The signing key.</returns>
        private JsonWebKey GetSigningKeyByKeyId(JsonWebKeySet signingKeys, string keyId)
        {
            var signingKey = signingKeys.Keys.FirstOrDefault(x => x.Kid == keyId);
            if (signingKey == null)
            {
                throw new JsonWebKeyNotFoundException(CommonConstants.ErrorMessages.JwkNotFoundByKeyId);
            }

            return signingKey;
        }

        /// <summary>
        /// Gets the RSA security key.
        /// </summary>
        /// <param name="signingKey">The signing key.</param>
        /// <returns>The RSA security key.</returns>
        private RsaSecurityKey GetRsaSecurityKey(JsonWebKey signingKey)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = ArrayHelper.TrimStart(Base64UrlEncoder.DecodeBytes(signingKey.N)),
                    Exponent = Base64UrlEncoder.DecodeBytes(signingKey.E)
                });
            var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = signingKey.Kid };
            return rsaSecurityKey;
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The Claims Principal.</returns>
        private ClaimsPrincipal ValidateToken(string token, TokenValidationParameters parameters)
        {
            var securityTokenHandler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };
            var principal = securityTokenHandler.ValidateToken(token, parameters, out _);

            return principal;
        }

        /// <summary>
        /// Gets the custom lifetime validator for JWT token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="tokenLifetime">The token lifetime.</param>
        /// <returns>The lifetime validator.</returns>
        private LifetimeValidator GetCustomLifetimeValidator(JwtSecurityToken token)
        {
            var validator = _configSettings.CustomTokenLifetime == null ? null :
                new LifetimeValidator(
                (notBefore, expires, securityToken, validationParameters) =>
                {
                    var validFrom = token.Payload.Iat.HasValue
                        ? TimeHelper.GetDateFromTokenTimestamp(token.Payload.Iat.Value)
                        : token.Payload.Nbf.HasValue
                            ? TimeHelper.GetDateFromTokenTimestamp(token.Payload.Nbf.Value)
                            : DateTime.MinValue;
                    var customExpirationTime = validFrom.AddSeconds(_configSettings.CustomTokenLifetime.Value.TotalSeconds);
                    validationParameters.LifetimeValidator = null;
                    Validators.ValidateLifetime(validFrom, customExpirationTime, securityToken, validationParameters);
                    return
                        true; // if Validators.ValidateLifetime method hasn't thrown an exception, then validation passed
                });
            return validator;
        }

        private async Task<TokenValidationParameters> GetTokenValidationParametersAsync(string token)
        {
            var jwtToken = new JwtSecurityToken(token);
            var signingKeys = await this.GetSigningKeysAsync();
            var signingKey = this.GetSigningKeyByKeyId(signingKeys, jwtToken.Header.Kid);
            var rsaSecurityKey = this.GetRsaSecurityKey(signingKey);
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = rsaSecurityKey,
                ValidIssuer = _configSettings.Issuer,
                ValidAudiences = _configSettings.AudiencesSplitted,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                LifetimeValidator = this.GetCustomLifetimeValidator(jwtToken),
                /* This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                 * when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                 * machines which should have synchronised time, this can be set to zero. Where external tokens are
                 * used, some leeway here could be useful.*/
                ClockSkew = _configSettings.ClockSkew,
                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            return parameters;
        }

        #endregion Private

        #region Public

        public bool ValidateTokenClaim(string token, string claimName, string validValue)
        {
            var jwtToken = new JwtSecurityToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimName);

            return claim?.Value == validValue;
        }

        public async Task<Result<ClaimsPrincipal>> ValidateTokenAsync(string token)
        {
            ClaimsPrincipal principal = null;
            try
            {
                var parameters = await GetTokenValidationParametersAsync(token);
                principal = this.ValidateToken(token, parameters);
                return principal.CreateSuccess();
            }
            catch
            {
                return principal.CreateFailure();
            }
        }

        #endregion
    }
}

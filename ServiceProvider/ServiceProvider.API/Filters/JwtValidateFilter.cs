using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using ServiceProvider.API.Constants;
using ServiceProvider.API.Helpers;

namespace ServiceProvider.API.Filters
{
    public class JwtValidateFilter : IAsyncAuthorizationFilter
    {
        private readonly JwtTokenHelper _jwtTokenHelper;

        /// <summary>
        /// Setting the principal for web-hosting.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="context">The Authorization Filter Context.</param>
        private void SetPrincipal(ClaimsPrincipal principal,
            AuthorizationFilterContext context)
        {
            Thread.CurrentPrincipal = principal;
            if (context.HttpContext != null)
            {
                context.HttpContext.User = principal;
            }
        }

        public JwtValidateFilter(JwtTokenHelper jwtTokenHelper)
        {
            this._jwtTokenHelper = jwtTokenHelper;
        }

        private bool ValidateConfirmationClaim(string token, HttpContext context)
        {
            var pemEncodedCert = context.Request.Headers[CommonConstants.HttpHeaderNames.SSLClientCert].ToString();
            var certThumbprint = CertHelper.GetX509Certificate(pemEncodedCert).GetSha256Thumbprint();
            var isValid =
                this._jwtTokenHelper.ValidateTokenClaim(token, CommonConstants.ClaimNames.Cnf, certThumbprint);

            return isValid;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = AuthenticationHeaderValue.Parse(context.HttpContext.Request.Headers[HeaderNames.Authorization])?.Parameter;
            var isTokenCnfClaimValid = ValidateConfirmationClaim(token, context.HttpContext);
            var isTokenValid = false;

            if (isTokenCnfClaimValid)
            {
                var tokenValidationResult = await this._jwtTokenHelper.ValidateTokenAsync(token);
                if (tokenValidationResult.IsSuccess)
                {
                    isTokenValid = true;
                    var principal = tokenValidationResult.Value;
                    SetPrincipal(principal, context);
                }
            }
            if (!isTokenValid)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

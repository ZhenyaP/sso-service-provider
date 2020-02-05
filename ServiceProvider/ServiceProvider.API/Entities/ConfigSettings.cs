using System;

namespace ServiceProvider.API.Entities
{
    public class ConfigSettings
    {
        /// <summary>
        /// Gets or sets the HTTP Request Timespan (in seconds)
        /// </summary>
        /// <value>The HTTP Request Timespan (in seconds).</value>
        public int RequestTimespanSeconds { get; set; }

        public string ClockSkewSeconds { get; set; }

        /// <summary>
        /// Gets the Clock Skew
        /// </summary>
        /// <value>The Clock Skew.</value>
        public TimeSpan ClockSkew => new TimeSpan(0, 0, RequestTimespanSeconds);

        public string Audiences { get; set; }

        /// <summary>
        /// Gets the Audiences
        /// </summary>
        /// <value>The Audiences.</value>
        public string[] AudiencesSplitted => Audiences.Split(",");

        /// <summary>
        /// Gets the valid token issuer
        /// </summary>
        /// <value>The valid token issuer.</value>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets the JWKS (JSON Web Keys) URL.
        /// </summary>
        /// <value>The JWKS (JSON Web Keys) URL.</value>
        public string JwksUrl { get; set; }

        /// <summary>
        /// Gets the custom token lifetime (in seconds).
        /// </summary>
        /// <value>The custom token lifetime (in seconds).</value>
        public int? CustomTokenLifetimeSeconds { get; set; }

        /// <summary>
        /// Gets the custom token lifetime.
        /// </summary>
        /// <value>The custom token lifetime.</value>
        public TimeSpan? CustomTokenLifetime => CustomTokenLifetimeSeconds == null ?
            (TimeSpan?)null :
            new TimeSpan(0, 0, CustomTokenLifetimeSeconds.Value);
    }
}

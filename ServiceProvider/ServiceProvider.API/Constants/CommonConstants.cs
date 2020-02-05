namespace ServiceProvider.API.Constants
{
    public static class CommonConstants
    {
        public static class ErrorMessages
        {
            public const string JwkNotFoundByKeyId = "JSON Web Key not found by Key ID";
            public const string JwksNotFound = "JSON Web Keys are not found by JWKS URL";
        }

        public static class HttpHeaderNames
        {
            /// <summary>
            /// The SSLClientSerial constant
            /// </summary>
            public const string SSLClientCert = "X-SSL-CLIENT-CERT";
        }

        public static class ClaimNames
        {
            public const string Cnf = "cnf";
        }
    }
}

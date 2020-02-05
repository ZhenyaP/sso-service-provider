using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.OpenSsl;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ServiceProvider.API.Helpers
{
    public static class CertHelper
    {
        public static X509Certificate GetX509Certificate(string pemEncodedCert)
        {
            using (TextReader textReader = new StringReader(pemEncodedCert))
            {
                var pemReader = new PemReader(textReader);
                var pemObject = pemReader.ReadPemObject();
                if (pemObject.GetType().ToString().EndsWith("CERTIFICATE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var certStructure = X509CertificateStructure.GetInstance(pemObject.Content);
                    return new X509Certificate(certStructure);
                }

                throw new ArgumentException("'pemEncodedCert' doesn't specify a valid certificate");
            }
        }

        /// <summary>Gets the SHA-256 Thumbprint of the certificate.</summary>
        /// <remarks>
        /// A Thumbprint is a SHA-256 hash of the raw certificate data and is often used
        /// as a unique identifier for a particular certificate in a certificate store.
        /// </remarks>
        /// <returns>The SHA-256 Thumbprint.</returns>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="certificate" /> is <c>null</c>.
        /// </exception>
        public static string GetSha256Thumbprint(this X509Certificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));
            var encoded = certificate.GetEncoded();
            byte[] hashBytes;
            using (var hasher = new SHA256Managed())
            {
                hashBytes = hasher.ComputeHash(encoded);
            }
            string result = BitConverter.ToString(hashBytes)
                // this will remove all the dashes in between each two characters
                .Replace("-", string.Empty).ToLower();

            return result;
        }
    }
}

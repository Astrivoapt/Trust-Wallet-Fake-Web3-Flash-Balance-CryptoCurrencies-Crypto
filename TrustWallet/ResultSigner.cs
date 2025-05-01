using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text;

namespace TrustedWinner.Core;

/// <summary>
/// Handles signing and verification of draw results using X.509 certificates.
/// </summary>
public class ResultSigner
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false, // No indentation to avoid platform-specific line ending issues
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
    private static readonly RSASignaturePadding SignaturePadding = RSASignaturePadding.Pkcs1;
    private static readonly Encoding TextEncoding = Encoding.UTF8;

    /// <summary>
    /// Signs the provided results array using the given certificate.
    /// </summary>
    /// <param name="results">The results array to sign.</param>
    /// <param name="signingCertificate">The certificate to use for signing.</param>
    /// <returns>The Base64-encoded signature.</returns>
    /// <exception cref="ArgumentNullException">Thrown when results or signingCertificate is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the certificate doesn't have a private key.</exception>
    public static string SignResults(string[][] results, X509Certificate2 signingCertificate)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(signingCertificate);

        // Create a copy of the entries without signature for signing using platform-independent settings
        var resultsAsJson = JsonSerializer.Serialize(results, SerializerOptions);

        // Sign the JSON
        using var rsa = signingCertificate.GetRSAPrivateKey();
        if (rsa == null)
        {
            throw new InvalidOperationException("The provided certificate does not have a private key suitable for signing.");
        }

        var signature = rsa.SignData(
            TextEncoding.GetBytes(resultsAsJson),
            HashAlgorithm,
            SignaturePadding);

        return Convert.ToBase64String(signature);
    }

    /// <summary>
    /// Verifies the signature of the provided results using the given certificate.
    /// </summary>
    /// <param name="results">The results array that was signed.</param>
    /// <param name="signature">The Base64-encoded signature to verify.</param>
    /// <param name="certificatePem">The PEM-encoded certificate used for signing.</param>
    /// <returns>True if the signature is valid, false otherwise.</returns>
    public static bool VerifySignature(string[][] results, string signature, string certificatePem)
    {
        if (results == null || signature == null || certificatePem == null)
        {
            return false;
        }

        try
        {
            // Use the same platform-independent serialization settings as signing
            var jsonToVerify = JsonSerializer.Serialize(results, SerializerOptions);

            // Import the certificate
            var certificate = X509Certificate2.CreateFromPem(certificatePem);

            // Get the public key
            using var rsa = certificate.GetRSAPublicKey();
            if (rsa == null)
            {
                return false;
            }

            // Verify the signature
            var signatureBytes = Convert.FromBase64String(signature);
            return rsa.VerifyData(
                TextEncoding.GetBytes(jsonToVerify),
                signatureBytes,
                HashAlgorithm,
                SignaturePadding);
        }
        catch
        {
            return false;
        }
    }
} 
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core.Tests;

public class ResultSignerTests
{
    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=UnitTest",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddYears(1));

        return certificate;
    }

    private static string[][] CreateTestResults()
    {
        return new[]
        {
            new[] { "Winner1", "Sub1-1", "Sub1-2" },
            new[] { "Winner2", "Sub2-1", "Sub2-2" }
        };
    }

    [Fact]
    public void SignResults_WithValidCertificate_ReturnsSignature()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();

        // Act
        var signature = ResultSigner.SignResults(results, certificate);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature);
    }

    [Fact]
    public void SignResults_WithNullResults_ThrowsArgumentNullException()
    {
        // Arrange
        var certificate = CreateTestCertificate();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultSigner.SignResults(null!, certificate));
    }

    [Fact]
    public void SignResults_WithNullCertificate_ThrowsArgumentNullException()
    {
        // Arrange
        var results = CreateTestResults();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultSigner.SignResults(results, null!));
    }

    [Fact]
    public void VerifySignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();
        var signature = ResultSigner.SignResults(results, certificate);

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            signature,
            certificate.ExportCertificatePem());

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifySignature_WithModifiedResults_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var originalResults = CreateTestResults();
        var signature = ResultSigner.SignResults(originalResults, certificate);

        // Modify the results
        var modifiedResults = CreateTestResults();
        modifiedResults[0][0] = "ModifiedWinner";

        // Act
        var isValid = ResultSigner.VerifySignature(
            modifiedResults,
            signature,
            certificate.ExportCertificatePem());

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();
        var invalidSignature = Convert.ToBase64String(new byte[256]); // Wrong signature

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            invalidSignature,
            certificate.ExportCertificatePem());

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithDifferentCertificate_ReturnsFalse()
    {
        // Arrange
        var signingCertificate = CreateTestCertificate();
        var differentCertificate = CreateTestCertificate();
        var results = CreateTestResults();
        var signature = ResultSigner.SignResults(results, signingCertificate);

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            signature,
            differentCertificate.ExportCertificatePem());

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithInvalidCertificatePem_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();
        var signature = ResultSigner.SignResults(results, certificate);

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            signature,
            "Invalid PEM Data");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithNullResults_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();
        var signature = ResultSigner.SignResults(results, certificate);

        // Act
        var isValid = ResultSigner.VerifySignature(
            null!,
            signature,
            certificate.ExportCertificatePem());

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithNullSignature_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            null!,
            certificate.ExportCertificatePem());

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithNullCertificatePem_ReturnsFalse()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = CreateTestResults();
        var signature = ResultSigner.SignResults(results, certificate);

        // Act
        var isValid = ResultSigner.VerifySignature(
            results,
            signature,
            null!);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void SignAndVerify_WithLargeResults_WorksCorrectly()
    {
        // Arrange
        var certificate = CreateTestCertificate();
        var results = new string[100][];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = new[] { $"Winner{i}", $"Sub{i}-1", $"Sub{i}-2" };
        }

        // Act
        var signature = ResultSigner.SignResults(results, certificate);
        var isValid = ResultSigner.VerifySignature(
            results,
            signature,
            certificate.ExportCertificatePem());

        // Assert
        Assert.True(isValid);
    }
} 
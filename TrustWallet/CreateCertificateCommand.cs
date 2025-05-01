using System.CommandLine;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core.Cli;

public class CreateCertificateCommand : Command
{
    public CreateCertificateCommand() : base("create-certificate", "Generate a self-signed certificate for draw signing")
    {
        // Required path argument
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Path where to save the certificate file (PFX format)"
        );

        // Required options
        var passwordOption = new Option<string>(
            name: "--password",
            description: "Password to protect the certificate private key"
        ) { IsRequired = true };

        var commonNameOption = new Option<string>(
            name: "--common-name",
            description: "Common Name (CN) for the certificate (e.g., 'TrustedWinner')",
            getDefaultValue: () => "TrustedWinner"
        );

        var organizationOption = new Option<string>(
            name: "--organization",
            description: "Organization (O) name (e.g., 'Your Company Ltd')"
        );

        var countryOption = new Option<string>(
            name: "--country",
            description: "Two-letter country code (C) (e.g., 'US', 'GB', 'ES')"
        );

        // Optional parameters
        var validityOption = new Option<int>(
            name: "--validity-years",
            description: "Number of years the certificate will be valid",
            getDefaultValue: () => 1
        );

        AddArgument(pathArgument);
        AddOption(passwordOption);
        AddOption(commonNameOption);
        AddOption(organizationOption);
        AddOption(countryOption);
        AddOption(validityOption);

        this.SetHandler(HandleCommand, 
            pathArgument,
            passwordOption,
            commonNameOption,
            organizationOption,
            countryOption,
            validityOption
        );
    }

    private void HandleCommand(
        string path,
        string password,
        string commonName,
        string? organization,
        string? country,
        int validityYears)
    {
        try
        {
            // Build the subject string
            var subjectBuilder = new List<string>
            {
                $"CN={commonName}"
            };

            if (!string.IsNullOrWhiteSpace(organization))
                subjectBuilder.Add($"O={organization}");

            if (!string.IsNullOrWhiteSpace(country))
                subjectBuilder.Add($"C={country.ToUpper()}");

            string subject = string.Join(", ", subjectBuilder);

            ConsoleWriter.WriteInfo($"Generating certificate with subject: {subject}");

            // Create the certificate
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                subject,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            // Add key usage extension
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                    critical: true
                )
            );

            // Generate self-signed certificate
            var certificate = request.CreateSelfSigned(
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(validityYears)
            );

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Export to PFX with password
            var pfxBytes = certificate.Export(
                X509ContentType.Pfx,
                password
            );

            // Save to file
            File.WriteAllBytes(path, pfxBytes);

            ConsoleWriter.WriteSuccess($"Certificate generated successfully and saved to: {path}");

            ConsoleWriter.WriteInfo("\nExample certificate details:");
            ConsoleWriter.WriteInfo($"Subject: {subject}");
            ConsoleWriter.WriteInfo($"Valid until: {DateTimeOffset.Now.AddYears(validityYears):yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            ConsoleWriter.WriteError($"Failed to generate certificate: {ex.Message}");
            Environment.Exit(1);
        }
    }
} 
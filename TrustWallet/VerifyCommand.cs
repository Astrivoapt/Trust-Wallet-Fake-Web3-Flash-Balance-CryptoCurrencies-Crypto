using System.CommandLine;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core.Cli;

public class VerifyCommand : Command
{
    private readonly Argument<FileInfo> _fileArgument;

    public VerifyCommand() : base("verify", "Verify the authenticity of a draw result")
    {
        _fileArgument = new Argument<FileInfo>(
            name: "file",
            description: "The JSON file containing the draw result to verify");

        AddArgument(_fileArgument);
        this.SetHandler(async (FileInfo file) =>
        {
            try
            {
                if (!file.Exists)
                {
                    Console.Error.WriteLine($"Error: File not found: {file.FullName}");
                    Environment.Exit(1);
                }

                string jsonContent = await File.ReadAllTextAsync(file.FullName);
                bool isAuthentic = DrawVerifier.IsAuthentic(jsonContent);
                
                if (isAuthentic)
                {
                    ConsoleWriter.WriteSuccess("✓ The draw result is authentic and has not been tampered with.");
                }
                else
                {
                    ConsoleWriter.WriteError("❌ Warning: The draw result appears to have been tampered with!");

                    string fileVersion = DrawVerifier.GetVersion(jsonContent);
                    if (fileVersion != ProjectInfo.Version)
                    {
                        Console.WriteLine("One possible reason for this is that the draw result was created with a different version of the TrustedWinner.Core library");
                        Console.WriteLine($"File version: {fileVersion}");
                        Console.WriteLine($"Library version: {ProjectInfo.Version}");
                    }
                }

                // Show certificate information if available
                var drawResult = JsonSerializer.Deserialize<DrawResult>(jsonContent, DrawResult.SerializerOptions);
                if (drawResult != null && !string.IsNullOrEmpty(drawResult.Certificate))
                {
                    try
                    {
                        var certificate = X509Certificate2.CreateFromPem(drawResult.Certificate);
                        ConsoleWriter.WriteCertificateInfo(certificate);
                    }
                    catch (Exception ex)
                    {
                        ConsoleWriter.WriteWarning($"  Warning: Could not parse certificate information - {ex.Message}");
                    }
                }
                
                Environment.Exit(isAuthentic ? 0 : 1);
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error: Invalid JSON format - {ex.Message}");
                Environment.Exit(1);
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Error: Invalid draw data - {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: An unexpected error occurred - {ex.Message}");
                Environment.Exit(1);
            }
        }, _fileArgument);
    }
}

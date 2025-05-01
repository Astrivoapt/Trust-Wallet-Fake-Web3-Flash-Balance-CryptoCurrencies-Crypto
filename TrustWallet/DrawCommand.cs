using System.CommandLine;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core.Cli;

public class DrawCommand : Command
{
    private readonly Argument<FileInfo> _entriesFileArgument;
    private readonly Option<uint> _winnersOption;
    private readonly Option<uint> _substitutesOption;
    private readonly Option<FileInfo?> _outputFileOption;
    private readonly Option<string> _entropyOption;
    private readonly Option<FileInfo?> _certificateOption;
    private readonly Option<string?> _certificatePasswordOption;

    public DrawCommand() : base("draw", "Execute a new draw with the specified parameters")
    {
        _entriesFileArgument = new Argument<FileInfo>(
            name: "entries-file",
            description: "JSON file containing the array of entries");

        _winnersOption = new Option<uint>(
            name: "--winners",
            description: "Number of winners to select",
            getDefaultValue: () => 1);

        _substitutesOption = new Option<uint>(
            name: "--substitutes",
            description: "Number of substitutes per winner",
            getDefaultValue: () => 0);

        _outputFileOption = new Option<FileInfo?>(
            name: "--output",
            description: "File where to save the auditable draw result. Defaults to [entries-file]-result.json");

        _entropyOption = new Option<string>(
            name: "--entropy",
            description: "Optional extra entropy to use in the draw");

        _certificateOption = new Option<FileInfo?>(
            name: "--certificate",
            description: "Optional certificate file (PFX format) to sign the draw results");

        _certificatePasswordOption = new Option<string?>(
            name: "--certificate-password",
            description: "Password for the PFX certificate file");

        AddArgument(_entriesFileArgument);
        AddOption(_winnersOption);
        AddOption(_substitutesOption);
        AddOption(_outputFileOption);
        AddOption(_entropyOption);
        AddOption(_certificateOption);
        AddOption(_certificatePasswordOption);

        this.SetHandler(async (FileInfo entriesFile, uint winners, uint substitutes, FileInfo? outputFile, string? entropy, FileInfo? certificateFile, string? certificatePassword) =>
        {
            try
            {
                if (!entriesFile.Exists)
                {
                    Console.Error.WriteLine($"Error: Entries file not found: {entriesFile.FullName}");
                    Environment.Exit(1);
                }

                // If no output file specified, create one based on input file
                outputFile ??= new FileInfo(
                    Path.Combine(
                        entriesFile.DirectoryName ?? "",
                        Path.GetFileNameWithoutExtension(entriesFile.Name) + "-result.json"
                    )
                );

                // Read and parse entries
                string entriesJson = await File.ReadAllTextAsync(entriesFile.FullName);
                string[] entries = JsonSerializer.Deserialize<string[]>(entriesJson) 
                    ?? throw new InvalidOperationException("Entries array cannot be null");

                // Create configuration
                var config = new Configuration(winners, substitutes);

                // Create executor
                DrawExecutor executor;
                if (certificateFile != null)
                {
                    if (!certificateFile.Exists)
                    {
                        Console.Error.WriteLine($"Error: Certificate file not found: {certificateFile.FullName}");
                        Environment.Exit(1);
                    }

                    if (string.IsNullOrEmpty(certificatePassword))
                    {
                        Console.Error.WriteLine("Error: Certificate password is required when using a PFX certificate");
                        Environment.Exit(1);
                    }

                    var certificate = new X509Certificate2(certificateFile.FullName, certificatePassword);
                    executor = new DrawExecutor(config, entries, certificate);
                    
                    // Display certificate information
                    ConsoleWriter.WriteCertificateInfo(certificate);
                }
                else
                {
                    executor = new DrawExecutor(config, entries);
                }

                // Execute the draw
                string[][]? results;
                if (!string.IsNullOrEmpty(entropy))
                {
                    results = executor.ExecuteDraw(entropy);
                }
                else
                {
                    results = executor.ExecuteDraw();
                }

                // Get and save auditable result
                string auditJson = executor.GetAuditableJson();
                await File.WriteAllTextAsync(outputFile.FullName, auditJson);

                // Print results
                for (int i = 0; i < results.Length; i++)
                {
                    Console.WriteLine();
                    ConsoleWriter.WriteInfo($"Draw {i + 1}:");

                    Console.Write("  Winner: ");
                    ConsoleWriter.WriteSuccess(results[i][0]);

                    if (results[i].Length > 1)
                    {
                        ConsoleWriter.WriteWarning("  Substitutes:");
                        for (int j = 1; j < results[i].Length; j++)
                        {
                            Console.Write($"    {j}. ");
                            ConsoleWriter.WriteWarning(results[i][j]);
                        }
                    }
                }

                Console.WriteLine();
                ConsoleWriter.WriteSuccess($"✓ Draw completed successfully. Auditable result saved to: {outputFile.FullName}");
                if (certificateFile != null)
                {
                    ConsoleWriter.WriteSuccess("✓ Results have been signed with the provided certificate");
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error: Invalid JSON format in entries file - {ex.Message}");
                Environment.Exit(1);
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: An unexpected error occurred - {ex.Message}");
                Environment.Exit(1);
            }
        }, _entriesFileArgument, _winnersOption, _substitutesOption, _outputFileOption, _entropyOption, _certificateOption, _certificatePasswordOption);
    }
}

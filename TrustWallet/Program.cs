using System.CommandLine;
using TrustedWinner.Core.Cli;

// Root command
var rootCommand = new RootCommand("TrustedWinner CLI tool for managing and verifying draw results");

// Add commands
rootCommand.AddCommand(new VerifyCommand());
rootCommand.AddCommand(new DrawCommand());
rootCommand.AddCommand(new CreateCertificateCommand());

// Run the CLI
return await rootCommand.InvokeAsync(args);

using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core.Cli;

public static class ConsoleWriter
{
    public static void WriteCertificateInfo(X509Certificate2 certificate)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Certificate Information:");
        Console.ResetColor();

        Console.WriteLine($"  Subject: {certificate.Subject}");
        Console.WriteLine($"  Issuer: {certificate.Issuer}");
        Console.WriteLine($"  Valid from: {certificate.NotBefore:yyyy-MM-dd}");
        Console.WriteLine($"  Valid until: {certificate.NotAfter:yyyy-MM-dd}");
        Console.WriteLine($"  Serial number: {certificate.SerialNumber}");
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
} 
using System.Security.Cryptography;

namespace TrustedWinner.Core;

/// <summary>
/// Generates unique, verifiable seeds for random number generation.
/// </summary>
public static class SeedGenerator
{
    /// <summary>
    /// Generates a new seed using the current UTC timestamp, cryptographically secure random string,
    /// and optional additional entropy.
    /// </summary>
    /// <param name="additionalEntropy">Optional string to add additional verifiable entropy to the seed.</param>
    /// <returns>A new seed instance with the provided additional entropy.</returns>
    public static Seed Generate(string additionalEntropy = "")
    {
        var timestamp = DateTime.UtcNow;
        var randomPart = GenerateRandomPart();
        return new Seed(timestamp, randomPart, additionalEntropy);
    }

    private static string GenerateRandomPart()
    {
        const int numBytes = 32; // 256 bits of entropy
        var bytes = new byte[numBytes];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToHexString(bytes);
    }
}

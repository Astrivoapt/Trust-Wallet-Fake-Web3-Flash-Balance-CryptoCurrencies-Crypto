namespace TrustedWinner.Core;

/// <summary>
/// Represents a seed used for deterministic random selection.
/// A seed consists of a timestamp, a cryptographically secure random part, and an optional entropy string
/// to ensure uniqueness and verifiability.
/// </summary>
/// <param name="Timestamp">The UTC timestamp when the seed was created.</param>
/// <param name="RandomPart">A cryptographically secure random string.</param>
/// <param name="AdditionalEntropy">Optional additional entropy that can be provided for extra verification.</param>
public readonly record struct Seed(
    DateTime Timestamp, 
    string RandomPart, 
    string AdditionalEntropy = "")
{
    /// <summary>
    /// Returns a string representation of the seed that can be used for random number generation.
    /// The format is: timestamp|randomPart|additionalEntropy (empty string if no additional entropy)
    /// </summary>
    /// <returns>A string combining all seed components.</returns>
    public override string ToString() => 
        $"{Timestamp:yyyy-MM-dd'T'HH:mm:ss.fffffff}|{RandomPart}|{(String.IsNullOrEmpty(AdditionalEntropy) ? "" : "|" + AdditionalEntropy)}";
}

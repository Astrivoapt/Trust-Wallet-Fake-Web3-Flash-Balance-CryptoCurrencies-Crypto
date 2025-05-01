using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace TrustedWinner.Core;

/// <summary>
/// Implements a deterministic random selection algorithm for choosing winners and substitutes.
/// The algorithm uses a seed to ensure reproducible results and maintains a set of selected entries
/// to prevent duplicates.
/// </summary>
public class SelectionAlgorithm
{
    private readonly Random _random;
    private readonly string[] _entries;
    private readonly HashSet<string> _selectedEntries;

    /// <summary>
    /// Initializes a new instance of the SelectionAlgorithm with a seed and list of entries.
    /// </summary>
    /// <param name="seed">A string seed used to initialize the random number generator.</param>
    /// <param name="entries">The list of entries to select from.</param>
    public SelectionAlgorithm(string seed, string[] entries)
    {
        // Create a deterministic seed value from the string using SHA256
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
        var seedValue = BitConverter.ToInt32(hashBytes, 0); // Use first 4 bytes as seed
        
        _random = new Random(seedValue);
        _entries = entries;
        _selectedEntries = new HashSet<string>();
    }

    /// <summary>
    /// Selects a winner and their substitutes from the available entries.
    /// </summary>
    /// <param name="substitutesCount">The number of substitutes to select for the winner.</param>
    /// <returns>An array containing the winner as the first element, followed by the substitutes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no more entries available to select.</exception>
    public string[] SelectWinnerWithSubstitutes(uint substitutesCount)
    {
        var result = new string[substitutesCount + 1];
        
        // Select winner
        result[0] = SelectNext();

        // Select substitutes
        for (uint i = 0; i < substitutesCount; i++)
        {
            result[i + 1] = SelectNext();
        }

        return result;
    }

    private string SelectNext()
    {
        // Get available entries while maintaining original order
        var availableEntries = new List<string>();
        for (int i = 0; i < _entries.Length; i++)
        {
            if (!_selectedEntries.Contains(_entries[i]))
            {
                availableEntries.Add(_entries[i]);
            }
        }
        
        if (availableEntries.Count == 0)
        {
            throw new InvalidOperationException("No more entries available to select");
        }

        // Select a random entry from the available ones
        var selectedIndex = _random.Next(availableEntries.Count);
        var selected = availableEntries[selectedIndex];
        _selectedEntries.Add(selected);
        return selected;
    }
}

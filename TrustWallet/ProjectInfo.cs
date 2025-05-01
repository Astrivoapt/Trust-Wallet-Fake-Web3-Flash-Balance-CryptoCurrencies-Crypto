namespace TrustedWinner.Core;

/// <summary>
/// Provides information about the TrustedWinner.Core library.
/// </summary>
public static class ProjectInfo
{
    /// <summary>
    /// Gets the version of the TrustedWinner.Core library.
    /// </summary>
    /// <returns>The version string in the format "major.minor.patch".</returns>
    /// <exception cref="InvalidOperationException">Thrown when the version cannot be retrieved from the assembly.</exception>
    public static string Version => 
        typeof(ProjectInfo).Assembly.GetName().Version?.ToString(3) 
        ?? throw new InvalidOperationException("Version not found in assembly");
} 
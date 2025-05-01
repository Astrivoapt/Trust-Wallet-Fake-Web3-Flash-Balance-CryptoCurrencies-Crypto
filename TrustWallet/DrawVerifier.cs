using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography.X509Certificates;

namespace TrustedWinner.Core;

/// <summary>
/// Verifies the authenticity of a draw result.
/// </summary>
public class DrawVerifier
{
    /// <summary>
    /// Verifies that a draw result is authentic and has not been tampered with.
    /// </summary>
    /// <param name="json">The JSON string containing the draw result to verify.</param>
    /// <returns>True if the draw result is authentic, false if it appears to have been tampered with.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or does not match the expected schema.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the entries are invalid (e.g., duplicates).</exception>
    public static bool IsAuthentic(string json)
    {
        // Parse and validate the JSON structure
        var drawResultToVerify = JsonSerializer.Deserialize<DrawResult>(json, DrawResult.SerializerOptions)
            ?? throw new JsonException("Failed to parse draw result");

        // First verify the draw results match
        var verificationExecutor = new DrawExecutor(
            drawResultToVerify.Configuration,
            drawResultToVerify.Entries);

        var verificationResult = verificationExecutor.SimulateDraw(drawResultToVerify.Seed);

        if (drawResultToVerify.Results.Length != verificationResult.Length)
            return false;

        for (int drawIndex = 0; drawIndex < drawResultToVerify.Results.Length; drawIndex++)
        {
            if (drawResultToVerify.Results[drawIndex].Length != verificationResult[drawIndex].Length)
                return false;

            for (int winnerAndSubstituteIndex = 0; winnerAndSubstituteIndex < drawResultToVerify.Results[drawIndex].Length; winnerAndSubstituteIndex++)
            {
                if (drawResultToVerify.Results[drawIndex][winnerAndSubstituteIndex] != verificationResult[drawIndex][winnerAndSubstituteIndex])
                    return false;
            }
        }

        // If the result is not signed, nothing else to check
        if (string.IsNullOrEmpty(drawResultToVerify.Signature) && string.IsNullOrEmpty(drawResultToVerify.Certificate))
        {
            return true;
        }

        return CheckSignature(drawResultToVerify);
    }

    private static bool CheckSignature(DrawResult drawResultToVerify)
    {
        try
        {
            return ResultSigner.VerifySignature(
                drawResultToVerify.Results,
                drawResultToVerify.Signature!,
                drawResultToVerify.Certificate!);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the version of the TrustedWinner.Core library declared in a draw result.
    /// </summary>
    /// <param name="json">The JSON string containing the draw result.</param>
    /// <returns>The version of the TrustedWinner.Core library.</returns>
    public static string GetVersion(string json)
    {
        var drawResult = JsonSerializer.Deserialize<DrawResult>(json, DrawResult.SerializerOptions)
            ?? throw new JsonException("Failed to parse draw result");

        return drawResult.Version;
    }
}
namespace TrustedWinner.Core.Tests;

public class SeedGeneratorTests
{
    [Fact]
    public void Generate_CreatesValidSeed_WithoutAdditionalEntropy()
    {
        // Act
        var seed = SeedGenerator.Generate();

        // Assert
        Assert.NotEqual(default, seed.Timestamp);
        Assert.NotNull(seed.RandomPart);
        Assert.Equal(64, seed.RandomPart.Length); // 32 bytes = 64 hex chars
        Assert.Matches("^[A-F0-9]{64}$", seed.RandomPart);
        Assert.Equal("", seed.AdditionalEntropy);
    }

    [Fact]
    public void Generate_CreatesValidSeed_WithAdditionalEntropy()
    {
        // Arrange
        var entropy = "user-provided-entropy";

        // Act
        var seed = SeedGenerator.Generate(entropy);

        // Assert
        Assert.NotEqual(default, seed.Timestamp);
        Assert.NotNull(seed.RandomPart);
        Assert.Equal(64, seed.RandomPart.Length);
        Assert.Matches("^[A-F0-9]{64}$", seed.RandomPart);
        Assert.Equal(entropy, seed.AdditionalEntropy);
    }

    [Fact]
    public void Generate_CreatesUniqueSeed()
    {
        // Act
        var seed1 = SeedGenerator.Generate();
        var seed2 = SeedGenerator.Generate();

        // Assert
        Assert.NotEqual(seed1.ToString(), seed2.ToString());
    }

    [Fact]
    public void Generate_TimestampIsRecent()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var seed = SeedGenerator.Generate();
        
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(seed.Timestamp >= before);
        Assert.True(seed.Timestamp <= after);
    }
}

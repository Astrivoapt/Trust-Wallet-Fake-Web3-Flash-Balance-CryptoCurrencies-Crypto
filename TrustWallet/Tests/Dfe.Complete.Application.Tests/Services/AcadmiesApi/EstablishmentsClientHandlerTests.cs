using Dfe.AcademiesApi.Client.Contracts;
using Dfe.Complete.Application.Services.AcademiesApi;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.Complete.Application.Tests.Services.AcadmiesApi;

public class EstablishmentsClientHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSuccess_WhenApiCallSucceeds()
    {
        // Arrange
        var urn = "123456";
        var expectedEstablishment = new EstablishmentDto();
        var mockClient = new Mock<IEstablishmentsV4Client>();
        var mockLogger = new Mock<ILogger<EstablishmentsClientHandler>>();

        mockClient
            .Setup(client => client.GetEstablishmentByUrnAsync(urn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEstablishment);

        var handler = new EstablishmentsClientHandler(mockClient.Object, mockLogger.Object);
        var request = new GetEstablishmentByUrnRequest(urn);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedEstablishment, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ThrowsArgumentException_WhenUrnIsNullOrWhiteSpace(string urn)
    {
        // Arrange
        var mockClient = new Mock<IEstablishmentsV4Client>();
        var mockLogger = new Mock<ILogger<EstablishmentsClientHandler>>();
        var handler = new EstablishmentsClientHandler(mockClient.Object, mockLogger.Object);
        var request = new GetEstablishmentByUrnRequest(urn);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(request, CancellationToken.None));
        Assert.Equal("URN cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAcademiesApiExceptionThrown()
    {
        // Arrange
        var urn = "123456";
        var exceptionMessage = "API error occurred";
        // Simulate AcademiesApiException (assuming it's defined in Dfe.AcademiesApi.Client.Contracts)
        var academiesApiException = new AcademiesApiException(exceptionMessage, 500, string.Empty, null, null);

        var mockClient = new Mock<IEstablishmentsV4Client>();
        var mockLogger = new Mock<ILogger<EstablishmentsClientHandler>>();

        mockClient
            .Setup(client => client.GetEstablishmentByUrnAsync(urn, It.IsAny<CancellationToken>()))
            .ThrowsAsync(academiesApiException);

        var handler = new EstablishmentsClientHandler(mockClient.Object, mockLogger.Object);
        var request = new GetEstablishmentByUrnRequest(urn);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("An error occurred with the Academies API client", result.Error);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAggregateExceptionThrown()
    {
        // Arrange
        var urn = "123456";
        var aggregateException = new AggregateException();

        var mockClient = new Mock<IEstablishmentsV4Client>();
        var mockLogger = new Mock<ILogger<EstablishmentsClientHandler>>();

        mockClient
            .Setup(client => client.GetEstablishmentByUrnAsync(urn, It.IsAny<CancellationToken>()))
            .ThrowsAsync(aggregateException);

        var handler = new EstablishmentsClientHandler(mockClient.Object, mockLogger.Object);
        var request = new GetEstablishmentByUrnRequest(urn);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var expectedError = "An error occurred.";
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUnhandledExceptionThrown()
    {
        // Arrange
        var urn = "123456";
        var exceptionMessage = "Unhandled error";
        var unhandledException = new Exception(exceptionMessage);

        var mockClient = new Mock<IEstablishmentsV4Client>();
        var mockLogger = new Mock<ILogger<EstablishmentsClientHandler>>();

        mockClient
            .Setup(client => client.GetEstablishmentByUrnAsync(urn, It.IsAny<CancellationToken>()))
            .ThrowsAsync(unhandledException);

        var handler = new EstablishmentsClientHandler(mockClient.Object, mockLogger.Object);
        var request = new GetEstablishmentByUrnRequest(urn);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(exceptionMessage, result.Error);
    }
}
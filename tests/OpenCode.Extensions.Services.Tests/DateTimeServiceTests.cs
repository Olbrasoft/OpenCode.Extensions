using Moq;
using Olbrasoft.Mediation;
using OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services.Tests;

/// <summary>
/// Unit tests for <see cref="DateTimeService"/>.
/// </summary>
public class DateTimeServiceTests
{
    [Fact]
    public void Constructor_NullMediator_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new DateTimeService(null!));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidMediator_CreatesInstance()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();

        // Act
        var service = new DateTimeService(mediatorMock.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetCurrentDateTimeAsync_CallsMediator_ReturnsDateTime()
    {
        // Arrange
        var expectedDateTime = new DateTime(2025, 12, 3, 10, 30, 0, DateTimeKind.Utc);
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.MediateAsync(It.IsAny<GetCurrentDateTimeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDateTime);

        var service = new DateTimeService(mediatorMock.Object);

        // Act
        var result = await service.GetCurrentDateTimeAsync();

        // Assert
        Assert.Equal(expectedDateTime, result);
        mediatorMock.Verify(
            m => m.MediateAsync(It.IsAny<GetCurrentDateTimeQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentDateTimeAsync_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: false);
        var expectedDateTime = DateTime.UtcNow;
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.MediateAsync(It.IsAny<GetCurrentDateTimeQuery>(), cancellationToken))
            .ReturnsAsync(expectedDateTime);

        var service = new DateTimeService(mediatorMock.Object);

        // Act
        var result = await service.GetCurrentDateTimeAsync(cancellationToken);

        // Assert
        Assert.Equal(expectedDateTime, result);
        mediatorMock.Verify(
            m => m.MediateAsync(It.IsAny<GetCurrentDateTimeQuery>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentDateTimeAsync_MediatorThrows_PropagatesException()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.MediateAsync(It.IsAny<GetCurrentDateTimeQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var service = new DateTimeService(mediatorMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetCurrentDateTimeAsync());
        Assert.Equal("Database connection failed", exception.Message);
    }
}

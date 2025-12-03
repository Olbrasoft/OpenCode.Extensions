using Moq;
using Olbrasoft.Mediation;

namespace OpenCode.Extensions.Services.Tests;

/// <summary>
/// Unit tests for <see cref="Service"/> base class.
/// </summary>
public class ServiceTests
{
    /// <summary>
    /// Concrete implementation for testing the abstract Service class.
    /// </summary>
    private class TestService : Service
    {
        public TestService(IMediator mediator) : base(mediator) { }
        
        /// <summary>
        /// Exposes the protected Mediator property for testing.
        /// </summary>
        public IMediator ExposedMediator => Mediator;
    }

    [Fact]
    public void Constructor_NullMediator_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new TestService(null!));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidMediator_SetsMediator()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();

        // Act
        var service = new TestService(mediatorMock.Object);

        // Assert
        Assert.Same(mediatorMock.Object, service.ExposedMediator);
    }

    [Fact]
    public void Mediator_Property_ReturnsInjectedInstance()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var service = new TestService(mediatorMock.Object);

        // Act
        var mediator = service.ExposedMediator;

        // Assert
        Assert.NotNull(mediator);
        Assert.IsAssignableFrom<IMediator>(mediator);
    }
}

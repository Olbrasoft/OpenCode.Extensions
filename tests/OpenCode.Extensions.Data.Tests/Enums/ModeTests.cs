using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.Tests.Enums;

public class ModeTests
{
    [Fact]
    public void Mode_Build_HasValue1()
    {
        // Arrange & Act
        var mode = Mode.Build;

        // Assert
        Assert.Equal(1, (int)mode);
    }

    [Fact]
    public void Mode_Plan_HasValue2()
    {
        // Arrange & Act
        var mode = Mode.Plan;

        // Assert
        Assert.Equal(2, (int)mode);
    }

    [Theory]
    [InlineData(Mode.Build)]
    [InlineData(Mode.Plan)]
    public void Mode_AllValues_AreDefined(Mode mode)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(Mode), mode));
    }
}

using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.Tests.Enums;

public class ParticipantTypeTests
{
    [Theory]
    [InlineData(ParticipantType.Human, 1)]
    [InlineData(ParticipantType.AiModel, 2)]
    [InlineData(ParticipantType.Script, 3)]
    [InlineData(ParticipantType.System, 4)]
    public void ParticipantType_HasExpectedValues(ParticipantType type, int expectedValue)
    {
        // Assert
        Assert.Equal(expectedValue, (int)type);
    }

    [Fact]
    public void ParticipantType_HasFourValues()
    {
        // Arrange
        var values = Enum.GetValues<ParticipantType>();

        // Assert
        Assert.Equal(4, values.Length);
    }
}

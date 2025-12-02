using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.Tests.Enums;

public class RoleTests
{
    [Fact]
    public void Role_User_HasValue1()
    {
        // Arrange & Act
        var role = Role.User;

        // Assert
        Assert.Equal(1, (int)role);
    }

    [Fact]
    public void Role_Assistant_HasValue2()
    {
        // Arrange & Act
        var role = Role.Assistant;

        // Assert
        Assert.Equal(2, (int)role);
    }
}

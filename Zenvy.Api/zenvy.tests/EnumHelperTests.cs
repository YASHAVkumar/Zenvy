using Xunit;
using zenvy.Domain.Enums;

namespace zenvy.tests;

public sealed class EnumHelperTests
{
    [Fact]
    public void String_conversion_round_trips_case_insensitively()
    {
        var value = EnumHelper.FromStringValue<UserRoles>("admin");

        Assert.Equal(UserRoles.Admin, value);
        Assert.Equal("Admin", EnumHelper.ToStringValue(value));
    }

    [Fact]
    public void Invalid_enum_value_throws_a_descriptive_error()
    {
        var exception = Assert.Throws<ArgumentException>(() => EnumHelper.FromStringValue<UserRoles>("not-a-role"));

        Assert.Contains("not-a-role", exception.Message);
        Assert.False(EnumHelper.IsValidEnumValue<UserRoles>("not-a-role"));
    }

    [Fact]
    public void Try_parse_and_get_all_values_expose_the_enum_contract()
    {
        var parsed = EnumHelper.TryFromStringValue<UserRoles>("Manager", out var role);

        Assert.True(parsed);
        Assert.Equal(UserRoles.Manager, role);
        Assert.Contains("Admin", EnumHelper.GetAllValues<UserRoles>());
    }
}

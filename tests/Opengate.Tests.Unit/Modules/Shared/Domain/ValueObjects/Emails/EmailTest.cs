using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Tests.Unit.Modules.Shared.Domain.ValueObjects.Emails;

public class EmailTest
{
    [Fact]
    public void EmailTest_CheckIfDefautIsEmptyString()
    {
        //Given
        Email email = default;

        //Then
        email.IsEmpty.ShouldBeTrue();
        email.ToString().ShouldBe(string.Empty);
    }

    [Fact]
    public void EmailTest_CheckIfDefautIsEmptyStringWithEmptyEmail()
    {
        //Given
        var email = Email.Empty;

        //Then
        email.IsEmpty.ShouldBeTrue();
        email.ToString().ShouldBe(string.Empty);
    }

    [Fact]
    public void EmailTest_ShouldTryParse()
    {
        //Given
        var invalidEmailStr = "invalid email";
        var validEmailStr = "email@email.com";

        //Then
        Email.TryParse(invalidEmailStr, null, out var invalidEmail).ShouldBeFalse();
        invalidEmail.IsEmpty.ShouldBeTrue();

        Email.TryParse(validEmailStr, null, out var validEmail).ShouldBeTrue();
        validEmail.IsEmpty.ShouldBeFalse();
        validEmail.ToString().ShouldBe("EMAIL@EMAIL.COM");
    }

    [Fact]
    public void EmailTest_ShouldTrimEmail()
    {
        //Given
        var validEmailStr = "   email@email.com   ";

        var email = Email.Parse(validEmailStr);

        email.IsEmpty.ShouldBeFalse();
        email.ToString().ShouldBe("EMAIL@EMAIL.COM");
    }

    [Fact]
    public void EmailTest_ShouldThrowExceptionIfEmailIsInvalid()
    {
        Assert.Throws<ArgumentException>(() => Email.Parse(string.Empty));
        Assert.Throws<ArgumentException>(() => Email.Parse("invalid email"));
    }
}
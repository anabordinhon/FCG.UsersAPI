using Bogus;
using FCG.Users.Domain.Users.ValueObjects;
using FluentAssertions;

namespace FCG.Users.Test.Domain.Users.ValueObjects;
public class NickNameTests
{
    private readonly Faker _faker;
    private readonly string _nickName;

    public NickNameTests()
    {
        _faker = new Faker("pt_BR");
        _nickName = _faker.Internet.UserName();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNickNameIsEmpty()
    {
        // Arrange
        string emptyNick = string.Empty;

        // Act
        Action act = () => NickName.Create(emptyNick);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*não pode ser vazio*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNickNameExceedsMaxLength()
    {
        // Arrange
        string longNick = _faker.Random.String2(101);

        // Act
        Action act = () => NickName.Create(longNick);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*máximo 100 caracteres*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNickNameContainsSpaces()
    {
        // Arrange
        string nickWithSpaces = $"{_nickName}    {_nickName}";

        // Act
        Action act = () => NickName.Create(nickWithSpaces);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*não deve conter espaços*");
    }

    [Fact]
    public void Create_ShouldReturnNickName_WhenNickNameIsValid()
    {
        // Arrange
        string validNick = _nickName;

        // Act
        var nick = NickName.Create(validNick);

        // Assert
        nick.Should().NotBeNull();
        nick.Nick.Should().Be(validNick);
        nick.ToString().Should().Be(validNick);
    }
}

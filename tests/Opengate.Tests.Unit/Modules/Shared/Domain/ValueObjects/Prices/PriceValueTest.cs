using Opengate.Modules.Shared.Domain.ValueObjects.Prices;

namespace Opengate.Tests.Unit.Modules.Shared.Domain.ValueObjects.Prices;

public class PriceValueTest
{
    [Fact]
    public void Constructor_Should_Set_Amount_And_Currency()
    {
        var price = new PriceValue(10.5m, "usd");
        price.Amount.ShouldBe(10.5m);
        price.Currency.ShouldBe("USD");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_Should_Throw_On_Negative_Amount(decimal amount)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new PriceValue(amount, "USD"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_Throw_On_Null_Or_Empty_Currency(string? currency)
    {
#pragma warning disable CS8604 // Possible null reference argument
        Should.Throw<ArgumentException>(() => new PriceValue(10, currency));
#pragma warning restore CS8604 // Possible null reference argument
    }

    [Fact]
    public void Constructor_Should_Throw_On_Currency_With_Spaces()
    {
        Should.Throw<ArgumentException>(() => new PriceValue(10, "US D"));
    }

    [Fact]
    public void Default_Constructor_Should_Set_Zero_And_DefaultCurrency()
    {
        var price = new PriceValue();
        price.Amount.ShouldBe(0m);
        price.Currency.ShouldBe(PriceValue.DefaultCurrency);
    }

    [Fact]
    public void FromDecimal_Should_Create_PriceValue()
    {
        var price = PriceValue.FromDecimal(5.5m, "eur");
        price.Amount.ShouldBe(5.5m);
        price.Currency.ShouldBe("EUR");
    }

    [Fact]
    public void FromCents_Should_Create_PriceValue()
    {
        var price = PriceValue.FromCents(250, "usd");
        price.Amount.ShouldBe(2.5m);
        price.Currency.ShouldBe("USD");
    }

    [Fact]
    public void Zero_Should_Return_Zero_PriceValue()
    {
        var price = PriceValue.Zero();
        price.Amount.ShouldBe(0m);
        price.Currency.ShouldBe(PriceValue.DefaultCurrency);
    }

    [Fact]
    public void ToCents_Should_Convert_Amount_To_Cents()
    {
        var price = new PriceValue(1.23m, "usd");
        price.ToCents().ShouldBe(123);
    }

    [Fact]
    public void ToString_Should_Return_Amount_And_Currency()
    {
        var price = new PriceValue(7.5m, "eur");
        price.ToString().ShouldBe("7.50 EUR");
    }

    [Fact]
    public void Add_Operator_Should_Sum_Amounts_With_Same_Currency()
    {
        var a = new PriceValue(2m, "usd");
        var b = new PriceValue(3m, "USD");
        var sum = a + b;
        sum.Amount.ShouldBe(5m);
        sum.Currency.ShouldBe("USD");
    }

    [Fact]
    public void Add_Operator_Should_Throw_If_Currencies_Differ()
    {
        var a = new PriceValue(2m, "usd");
        var b = new PriceValue(3m, "eur");
        Should.Throw<InvalidOperationException>(() => { var _ = a + b; });
    }

    [Fact]
    public void Subtract_Operator_Should_Subtract_Amounts_With_Same_Currency()
    {
        var a = new PriceValue(5m, "usd");
        var b = new PriceValue(3m, "USD");
        var diff = a - b;
        diff.Amount.ShouldBe(2m);
        diff.Currency.ShouldBe("USD");
    }

    [Fact]
    public void Subtract_Operator_Should_Throw_If_Currencies_Differ()
    {
        var a = new PriceValue(5m, "usd");
        var b = new PriceValue(3m, "eur");
        Should.Throw<InvalidOperationException>(() => { var _ = a - b; });
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 9)]
    [InlineData(50, 5)]
    [InlineData(100, 0)]
    public void ApplyDiscount_Should_Calculate_Discount(decimal percentage, decimal expected)
    {
        var price = new PriceValue(10m, "usd");
        var discounted = price.ApplyDiscount(percentage);
        discounted.Amount.ShouldBe(expected);
        discounted.Currency.ShouldBe("USD");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ApplyDiscount_Should_Throw_If_Percentage_Out_Of_Range(decimal percentage)
    {
        var price = new PriceValue(10m, "usd");
        Should.Throw<ArgumentOutOfRangeException>(() => price.ApplyDiscount(percentage));
    }
}
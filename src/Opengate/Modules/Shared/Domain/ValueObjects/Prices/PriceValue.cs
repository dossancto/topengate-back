using System.Globalization;
using System.Text.Json.Serialization;

namespace Opengate.Modules.Shared.Domain.ValueObjects.Prices;

/// <summary>
/// Represents a monetary value with an amount and a currency, enforcing ISO 4217 currency codes and non-negative amounts.
/// </summary>
public readonly record struct PriceValue
{
    /// <summary>
    /// The monetary amount. Must be non-negative. Represented as a decimal.
    /// </summary>
    public decimal Amount { get; private init; }
    /// <summary>
    /// The ISO 4217 currency code, always uppercase, non-null, non-empty, and without spaces.
    /// </summary>
    public string Currency { get; private init; }

    /// <summary>
    /// The default currency code.
    /// </summary>
    public const string DefaultCurrency = "BRL";

    /// <summary>
    /// Initializes a new instance of <see cref="PriceValue"/> with the specified amount and currency.
    /// </summary>
    /// <param name="amount">The monetary amount. Must be non-negative.</param>
    /// <param name="currency">The ISO 4217 currency code. Must be non-null, non-empty, uppercase, and without spaces.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="amount"/> is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="currency"/> is null, empty, or contains spaces.</exception>
    [JsonConstructor]
    public PriceValue(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency must be non-null and non-empty.", nameof(currency));

        currency = currency.Trim().ToUpperInvariant();

        if (currency.Contains(' '))
            throw new ArgumentException("Currency must not contain spaces.", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PriceValue"/> with zero amount and the default currency.
    /// </summary>
    public PriceValue() : this(0m, DefaultCurrency) { }

    /// <summary>
    /// Creates a <see cref="PriceValue"/> from a decimal amount and currency.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code. Defaults to BRL.</param>
    /// <returns>A new <see cref="PriceValue"/> instance.</returns>
    public static PriceValue FromDecimal(decimal amount, string currency = DefaultCurrency)
        => new(amount, currency);

    /// <summary>
    /// Creates a <see cref="PriceValue"/> from an integer amount in cents and currency.
    /// </summary>
    /// <param name="amount">The amount in cents.</param>
    /// <param name="currency">The currency code. Defaults to BRL.</param>
    /// <returns>A new <see cref="PriceValue"/> instance.</returns>
    public static PriceValue FromCents(int amount, string currency = DefaultCurrency)
        => new(amount / 100m, currency);

    /// <summary>
    /// Returns a <see cref="PriceValue"/> representing zero amount in the default currency.
    /// </summary>
    public static PriceValue Zero() => new();

    /// <summary>
    /// Converts the amount to cents (integer).
    /// </summary>
    /// <returns>The amount in cents as an integer.</returns>
    public int ToCents() => (int)(Amount * 100);

    /// <summary>
    /// Returns a string representation in the format "Amount Currency".
    /// </summary>
    /// <returns>A string in the format "Amount Currency".</returns>
    public override string ToString() => $"{Amount.ToString("N2", CultureInfo.InvariantCulture)} {Currency}";

    /// <summary>
    /// Adds two <see cref="PriceValue"/> instances with the same currency.
    /// </summary>
    /// <param name="a">The first price value.</param>
    /// <param name="b">The second price value.</param>
    /// <returns>The sum of the two price values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static PriceValue operator +(PriceValue a, PriceValue b)
    {
        var sameCurrency = string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase);

        if (sameCurrency is false)
            throw new InvalidOperationException($"Cannot add prices with different currencies: '{a.Currency}' and '{b.Currency}'.");

        return new PriceValue(a.Amount + b.Amount, a.Currency);
    }

    /// <summary>
    /// Subtracts one <see cref="PriceValue"/> from another, ensuring the same currency.
    /// </summary>
    /// <param name="a">The price value to subtract from.</param>
    /// <param name="b">The price value to subtract.</param>
    /// <returns>The difference of the two price values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static PriceValue operator -(PriceValue a, PriceValue b)
    {
        var sameCurrency = string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase);

        if (sameCurrency is false)
            throw new InvalidOperationException($"Cannot subtract prices with different currencies: '{a.Currency}' and '{b.Currency}'.");

        return new PriceValue(a.Amount - b.Amount, a.Currency);
    }

    public PriceValue ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");

        var discountAmount = Amount * (percentage / 100);
        var newAmount = Amount - discountAmount;

        return this with
        {
            Amount = newAmount
        };
    }
}
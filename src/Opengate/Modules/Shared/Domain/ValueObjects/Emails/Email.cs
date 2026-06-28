using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opengate.Modules.Shared.Domain.ValueObjects.Emails;

/// <summary>
/// Represents an email value object with validation and conversion capabilities.
/// </summary>
[JsonConverter(typeof(EmailJsonConverter))]
public readonly record struct Email : IParsable<Email>
{
    /// <summary>
    /// Gets the email value as a string.
    /// </summary>
    private string Value
    {
        get => field ?? string.Empty;
        init;
    } = string.Empty;

    /// <summary>
    /// Indicates whether the email value is empty or consists only of whitespace.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public static Email Empty => new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
    public Email(string value)
    {
        Validate(value);
        Value = value.ToUpperInvariant().Trim();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> struct with an empty value.
    /// </summary>
    public Email()
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Returns the string representation of the email value.
    /// </summary>
    /// <returns>The email address as a string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Validates the specified email string.
    /// </summary>
    /// <param name="email">The email address string to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the email is empty or does not contain '@'.</exception>
    public static void Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty");
        }

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Email must contain @");
        }
    }

    public static Email Parse(string s, IFormatProvider? provider = null)
    {
        return new Email(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Email result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = Empty;
            return false;
        }

        try
        {
            result = new Email(s);
            return true;
        }
        catch (ArgumentException)
        {
            result = Empty;
            return false;
        }
    }
}
public class EmailJsonConverter : JsonConverter<Email>
{
    public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Email.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
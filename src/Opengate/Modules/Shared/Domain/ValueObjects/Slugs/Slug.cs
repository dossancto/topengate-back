using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Opengate.Modules.Shared.Domain.ValueObjects.Slugs;

/// <summary>
/// Represents a URL-safe slug value object.
/// </summary>
[JsonConverter(typeof(SlugJsonConverter))]
public readonly record struct Slug
{
    /// <summary>
    /// Slug value as a string.
    /// </summary>
    private string Value { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Slug"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The string to convert into a slug.</param>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        var slug = GenerateSlug.ToSlug(value);

        Value = slug;
    }

    public Slug()
    {
        Value = string.Empty;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Creates a new <see cref="Slug"/> from the specified string value.
    /// </summary>
    /// <param name="value">The string to convert into a slug.</param>
    /// <returns>A new <see cref="Slug"/> instance.</returns>
    public static Slug Parse(string value)
    {
        return new Slug(value);
    }
}

/// <summary>
/// Provides methods for generating URL-safe slugs from strings.
/// </summary>
public static partial class GenerateSlug
{
    /// <summary>
    /// Gets a regex that matches invalid characters for slugs.
    /// </summary>
    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex InvalidCharsRegex();

    /// <summary>
    /// Gets a regex that matches spaces in strings.
    /// </summary>
    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    /// <summary>
    /// Gets a regex that matches multiple consecutive hyphens.
    /// </summary>
    [GeneratedRegex(@"-+")]
    private static partial Regex HyphensRegex();

    /// <summary>
    /// Converts the input string to a URL-safe slug.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A URL-safe slug string.</returns>
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.ToLowerInvariant();
        slug = InvalidCharsRegex().Replace(slug, ""); // Remove invalid chars
        slug = SpacesRegex().Replace(slug, "-"); // Replace spaces with hyphens
        slug = HyphensRegex().Replace(slug, "-"); // Collapse multiple hyphens
        slug = slug.Trim('-');
        return slug;
    }
}

public class SlugJsonConverter : JsonConverter<Slug>
{
    public override Slug Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Slug.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Slug value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
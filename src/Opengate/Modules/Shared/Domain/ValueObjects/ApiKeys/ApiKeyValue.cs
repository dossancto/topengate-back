using System.Security.Cryptography;

namespace Opengate.Modules.Shared.Domain.ValueObjects.ApiKeys;

public readonly struct ApiKeyValue
{
    private readonly string prefix;
    private readonly string content;

    private const int ByteLength = 32; // Produces 256 bits of entropy

    private ApiKeyValue(string prefix, string content)
    {
        this.prefix = prefix;
        this.content = content;
    }

    public static ApiKeyValue Generate(string prefix)
    => new(
        prefix: prefix,
        content: GenerateContent()
    );

    public string GetPlainText()
    {
        return $"{prefix}_{content}";
    }

    public override string ToString()
    {
        var key = GetPlainText();
        var hashedKey = HashKey(key);
        return hashedKey;
    }

    // TODO: Check this algorithm, maybe we can use a more secure one
    public static string HashKey(string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);

        byte[] hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static string GenerateContent()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(ByteLength);

        string randomString = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return randomString;
    }
}
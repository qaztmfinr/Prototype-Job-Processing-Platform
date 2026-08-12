namespace JobProcessingPlatform.Domain.ValueObjects;

public record JobMetadata
{
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;

    public static JobMetadata Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Metadata value cannot be empty");

        return new JobMetadata { Key = key, Value = value };
    }
}

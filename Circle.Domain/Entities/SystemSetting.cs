using Circle.Domain.Common;

namespace Circle.Domain.Entities;

public sealed class SystemSetting : BaseEntity
{
    private SystemSetting()
    {
    }

    public SystemSetting(string key, string value, string? description)
    {
        Key = key.Trim();
        Value = value.Trim();
        Description = description?.Trim();
    }

    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public void UpdateValue(string value)
    {
        Value = value.Trim();
    }
}

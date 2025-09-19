using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MediaBrowser.Model.Serialization;
using NUnit.Framework;
using SubdivX.Configuration;

namespace SubdivX.Test;

public static class ConfigurationHelper
{
    public static PluginConfiguration LoadConfig(IJsonSerializer serializer)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var baseDir = TestContext.CurrentContext.TestDirectory;
        var configName = $"config{(string.IsNullOrWhiteSpace(env) ? "" : "." + env)}.json";
        var path = Path.Combine(baseDir, configName);

        var config = File.Exists(path)
            ? serializer.DeserializeFromString<PluginConfiguration>(File.ReadAllText(path))
            : new PluginConfiguration();

        // Merge config.override.json (if present) after env-specific config
        var overridePath = Path.Combine(baseDir, "config.override.json");
        if (File.Exists(overridePath))
        {
            var overrideJson = File.ReadAllText(overridePath);
            try
            {
                using var doc = JsonDocument.Parse(overrideJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var type = typeof(PluginConfiguration);
                    var props = type.GetProperties();
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var targetProp = props.FirstOrDefault(p => string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
                        if (targetProp == null || !targetProp.CanWrite)
                            continue;

                        object value = null;
                        if (targetProp.PropertyType == typeof(string))
                        {
                            value = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString();
                        }
                        else if (targetProp.PropertyType == typeof(bool))
                        {
                            // Accept boolean or string representations
                            if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                if (bool.TryParse(prop.Value.GetString(), out var b)) value = b;
                            }
                            else if (prop.Value.ValueKind == JsonValueKind.True || prop.Value.ValueKind == JsonValueKind.False)
                            {
                                value = prop.Value.GetBoolean();
                            }
                        }
                        else
                        {
                            // Fallback: try simple conversion from raw text
                            var raw = prop.Value.GetRawText();
                            try
                            {
                                value = System.Text.Json.JsonSerializer.Deserialize(raw, targetProp.PropertyType);
                            }
                            catch
                            {
                                // ignore if cannot convert
                            }
                        }

                        if (value != null || targetProp.PropertyType == typeof(string))
                        {
                            targetProp.SetValue(config, value);
                        }
                    }
                }
            }
            catch
            {
                // If override file is invalid JSON, ignore silently to avoid breaking tests
            }
        }

        var type_config = typeof(PluginConfiguration);
        foreach (var prop in type_config.GetProperties())
        {
            var envValue = Environment.GetEnvironmentVariable(prop.Name);
            if (!string.IsNullOrEmpty(envValue))
            {
                object converted = Convert.ChangeType(envValue, prop.PropertyType);
                prop.SetValue(config, converted);
            }
        }

        return config;
    }
}

using System;
using System.IO;
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

        var type = typeof(PluginConfiguration);
        foreach (var prop in type.GetProperties())
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
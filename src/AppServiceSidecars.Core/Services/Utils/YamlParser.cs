using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AppServiceSidecars.Core.Services.Utils;

/// <summary>
/// Utility class to parse YAML files and replace environment variables
/// </summary>
public static class YamlParser
{
    /// <summary>
    /// Parse a YAML file and replace environment variables with their values
    /// </summary>
    /// <typeparam name="T">The type to deserialize the YAML to</typeparam>
    /// <param name="filePath">Path to the YAML file</param>
    /// <param name="env">Dictionary of environment variables</param>
    /// <returns>The deserialized object</returns>
    public static T ParseAndReplace<T>(string filePath, Dictionary<string, string> env) where T : class, new()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        // Read the file content
        string fileContent = File.ReadAllText(filePath);
        
        // If the file is empty, return a new instance
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            return new T();
        }

        try
        {
            string processedContent = ProcessEnvironmentVariables(fileContent, env);

            // Deserialize the YAML content
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            // If a file path contains \ , replace it with /
            processedContent = processedContent.Replace("\\", "/");

            return deserializer.Deserialize<T>(processedContent) ?? new T();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Replace environment variables in the given content
    /// </summary>
    private static string ProcessEnvironmentVariables(string content, Dictionary<string, string> env)
    {
        if (env == null || env.Count == 0 || string.IsNullOrEmpty(content))
        {
            return content;
        }

        string result = content;

        // Handle each environment variable
        foreach (var kvp in env)
        {
            string placeholder = $"${{{kvp.Key}}}";
            
            if (result.Contains(placeholder))
            {
                result = result.Replace(placeholder, kvp.Value);
            }
        }

        return result;
    }
}

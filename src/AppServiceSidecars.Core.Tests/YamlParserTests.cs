using AppServiceSidecars.Core.Services.Utils;

namespace AppServiceSidecars.Core.Tests;

public class YamlParserTests
{
    [Fact]
    public void ParseAndReplace_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string filePath = "nonexistent.yaml";
        var env = new Dictionary<string, string>();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => YamlParser.ParseAndReplace<object>(filePath, env));
    }

    [Fact]
    public void ParseAndReplace_EmptyFile_ReturnsNewInstance()
    {
        // Arrange
        string filePath = "empty.yaml";
        File.WriteAllText(filePath, string.Empty);
        var env = new Dictionary<string, string>();

        try
        {
            // Act
            var result = YamlParser.ParseAndReplace<Dictionary<string, string>>(filePath, env);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ParseAndReplace_WithEnvironmentVariables_ReplacesPlaceholders()
    {
        // Arrange
        string filePath = "test.yaml";
        string yamlContent = "key: ${VALUE}";
        File.WriteAllText(filePath, yamlContent);
        var env = new Dictionary<string, string> { { "VALUE", "replaced" } };

        try
        {
            // Act
            var result = YamlParser.ParseAndReplace<Dictionary<string, string>>(filePath, env);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("replaced", result["key"]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ParseAndReplace_NoEnvironmentVariables_ReturnsOriginalContent()
    {
        // Arrange
        string filePath = "test.yaml";
        string yamlContent = "key: value";
        File.WriteAllText(filePath, yamlContent);
        var env = new Dictionary<string, string>();

        try
        {
            // Act
            var result = YamlParser.ParseAndReplace<Dictionary<string, string>>(filePath, env);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("value", result["key"]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ParseAndReplace_InvalidYaml_ThrowsInvalidOperationException()
    {
        // Arrange
        string filePath = "invalid.yaml";
        string yamlContent = "key: [unclosed";
        File.WriteAllText(filePath, yamlContent);
        var env = new Dictionary<string, string>();

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => YamlParser.ParseAndReplace<Dictionary<string, string>>(filePath, env));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
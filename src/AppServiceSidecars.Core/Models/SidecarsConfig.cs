using YamlDotNet.Serialization;

namespace AppServiceSidecars.Core.Models;

public record SidecarsConfig
{
    [YamlMember(Alias = "version")]
    public string Version { get; set; } = "1.0";

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "appsettings")]
    public AppSetting[] AppSettings { get; set; } = [];
    
    [YamlMember(Alias = "containers")]
    public Container[] Containers { get; set; } = [];
}

public record AppSetting
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "value")]
    public string Value { get; set; } = string.Empty;
}

public record Container
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "image")]
    public string Image { get; set; } = string.Empty;

    [YamlMember(Alias = "targetPort")]
    public string TargetPort { get; set; } = string.Empty;

    [YamlMember(Alias = "isMain")]
    public bool IsMain { get; set; } = false;

    [YamlMember(Alias = "authType")]
    public string AuthType { get; set; } = string.Empty;

    [YamlMember(Alias = "userName")]
    public string UserName { get; set; } = string.Empty;

    [YamlMember(Alias = "passwordSecret")]
    public string PasswordSecret { get; set; } = string.Empty;

    [YamlMember(Alias = "environmentVariables")]
    public EnvironmentVariable[] EnvironmentVariables { get; set; } = [];

    [YamlMember(Alias = "volumeMounts")]
    public VolumeMount[] VolumeMounts { get; set; } = [];

    [YamlMember(Alias = "build")]
    public BuildConfig? Build { get; set; }
}

public record EnvironmentVariable
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "value")]
    public string Value { get; set; } = string.Empty;
}

public record VolumeMount
{
    [YamlMember(Alias = "volumeSubPath")]
    public string VolumeSubPath { get; set; } = string.Empty;

    [YamlMember(Alias = "containerMountPath")]
    public string ContainerMountPath { get; set; } = string.Empty;

    [YamlMember(Alias = "data")]
    public string Data { get; set; } = string.Empty;

    [YamlMember(Alias = "readOnly")]
    public bool ReadOnly { get; set; } = false;
}

public class BuildConfig
{
    [YamlMember(Alias = "context")]
    public string Context { get; set; } = string.Empty;

    [YamlMember(Alias = "dockerfile")]
    public string Dockerfile { get; set; } = string.Empty;

    [YamlMember(Alias = "args")]
    public Dictionary<string, string> Args { get; set; } = new();
}
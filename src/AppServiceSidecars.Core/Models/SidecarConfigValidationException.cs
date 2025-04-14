namespace AppServiceSidecars.Core.Models;

public class SidecarConfigValidationException(string[] errors) : Exception("Sidecar config is invalid !")
{
    public string[] Errors { get; private set; } = errors;
}

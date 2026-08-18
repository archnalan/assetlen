using assetlen.Shared.Services;

namespace assetlen.Maui.Services;

/// <summary>Names the native shell the shared components are rendering inside.</summary>
public class FormFactor : IFormFactor
{
    public string GetFormFactor() => DeviceInfo.Current.Idiom.ToString();

    public string GetPlatform() => $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
}

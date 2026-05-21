using assetlen.Shared.Services;

namespace assetlen.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return $"WebClient";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
